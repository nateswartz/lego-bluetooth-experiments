using BluetoothController.Controllers;
using BluetoothController.EventHandlers.Internal;
using BluetoothController.Hubs;
using BluetoothController.Models;
using BluetoothController.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BluetoothController
{
    public class BluetoothLowEnergyAdapter : IBluetoothLowEnergyAdapter
    {
        private BluetoothLEAdvertisementWatcher _watcher;

        private readonly List<IHubController> _controllers;

        private const string _legoHubService = "00001623-1212-EFDE-1623-785FEABCD123";
        private const string _legoHubCharacteristic = "00001624-1212-EFDE-1623-785FEABCD123";

        private readonly object _lock = new();

        private readonly IBluetoothLowEnergyAdapterEventHandler _eventHandler;

        public BluetoothLowEnergyAdapter() : this(new SimpleBluetoothLowEnergyAdapterEventHandler())
        {
        }

        public BluetoothLowEnergyAdapter(IBluetoothLowEnergyAdapterEventHandler eventHandler)
        {
            _eventHandler = eventHandler;
            _controllers = new List<IHubController>();
        }

        public void StartBleDeviceWatcher()
        {
            _watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            _watcher.Received += ReceivedHandler;

            _watcher.Start();
        }

        public void StopBleDeviceWatcher()
        {
            if (_watcher != null)
            {
                _watcher.Received -= ReceivedHandler;
                _watcher.Stop();
                _watcher = null;
            }
        }

        private async void ReceivedHandler(BluetoothLEAdvertisementWatcher _, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            using var device = BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress).AsTask().Result;

            if (!await (IsValidDeviceAsync(device)))
                return;

            IHubController controller;
            lock (_lock)
            {
                if (_controllers.Any(c => c.SelectedBleDeviceId == device.DeviceId))
                    return;
                controller = new HubController(new LegoHub(), device.DeviceId);
                _controllers.Add(controller);
            }

            await _eventHandler.HandleDiscoveryAsync(new DiscoveredDevice
            {
                Name = device.Name,
                BluetoothDeviceId = device.DeviceId
            });
            await ConnectAsync(controller, _eventHandler.HandleConnectAsync);
        }

        private static async Task<bool> IsValidDeviceAsync(BluetoothLEDevice device)
        {
            return device != null &&
                    (await device.GetGattServicesAsync()).Services.Any(s => s.Uuid == new Guid(_legoHubService));
        }

        private async Task ConnectAsync(IHubController controller, Func<IHubController, string, Task> connectionHandler)
        {
            var service = await GetGattDeviceServiceAsync(controller.SelectedBleDeviceId);
            if (service == default)
            {
                await connectionHandler(null, "Failed to connect");
                return;
            }

            var characteristic = (await service.GetCharacteristicsForUuidAsync(new Guid(_legoHubCharacteristic))).Characteristics.Single();

            RegisterEventHandlers(controller);

            await controller.InitializeAsync(_eventHandler.HandleNotificationAsync, new GattCharacteristicWrapper(characteristic));

            await WaitForHubTypeDiscovery(controller);

            await connectionHandler(controller, "");
        }

        private static async Task WaitForHubTypeDiscovery(IHubController controller)
        {
            var counter = 0;
            while (controller.Hub == null && counter < 20)
            {
                await Task.Delay(50);
                counter++;
            }
        }

        private void RegisterEventHandlers(IHubController controller)
        {
            RegisterEventHandlersToDetectHubType(controller);
            controller.AddEventHandler(new DisconnectEventHandler(controller, OnControllerDisconnect));
        }

        private static void RegisterEventHandlersToDetectHubType(IHubController controller)
        {
            controller.AddEventHandler(new SystemTypeUpdateHubTypeEventHandler(controller));
            controller.AddEventHandler(new RemoteButtonStateUpdateHubTypeEventHandler(controller));
            controller.AddEventHandler(new InternalMotorStateUpdateHubTypeEventHandler(controller));
        }

        private async Task OnControllerDisconnect(IHubController hubController)
        {
            lock (_lock)
            {
                _controllers.Remove(hubController);
            }
            await _eventHandler.HandleDisconnectAsync(hubController);
        }

        private static async Task<GattDeviceService> GetGattDeviceServiceAsync(string bluetoothLEDeviceId)
        {
            var bluetoothLEDevice = await GetDeviceAsync(bluetoothLEDeviceId);
            if (bluetoothLEDevice == null)
            {
                return null;
            }

            var result = await bluetoothLEDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success)
            {
                return null;
            }

            return result.Services.FirstOrDefault(s => s.Uuid == new Guid(_legoHubService));
        }

        private static async Task<BluetoothLEDevice> GetDeviceAsync(string bluetoothLEDeviceId)
        {
            try
            {
                return await BluetoothLEDevice.FromIdAsync(bluetoothLEDeviceId);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
