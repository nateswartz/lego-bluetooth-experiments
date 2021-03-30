using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.EventHandlers.Internal;
using BluetoothController.Models;
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

        public bool IsScanning { get; private set; } = false;

        private const string _legoHubService = "00001623-1212-EFDE-1623-785FEABCD123";
        private const string _legoHubCharacteristic = "00001624-1212-EFDE-1623-785FEABCD123";

        private readonly object _lock = new();

        private readonly Func<DiscoveredDevice, Task> _discoveryHandler;
        private readonly Func<IHubController, string, Task> _connectionHandler;
        private readonly Func<IHubController, string, Task> _notificationHandler;
        private readonly Func<IHubController, Task> _disconnectHandler;

        public BluetoothLowEnergyAdapter(Func<DiscoveredDevice, Task> discoveryHandler,
                                         Func<IHubController, string, Task> connectionHandler,
                                         Func<IHubController, string, Task> notificationHandler,
                                         Func<IHubController, Task> disconnectHandler)
        {
            _discoveryHandler = discoveryHandler;
            _connectionHandler = connectionHandler;
            _notificationHandler = notificationHandler;
            _disconnectHandler = disconnectHandler;
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
            IsScanning = true;
        }

        public void StopBleDeviceWatcher()
        {
            if (_watcher != null)
            {
                _watcher.Received -= ReceivedHandler;
                _watcher.Stop();
                IsScanning = false;
                _watcher = null;
            }
        }

        private async void ReceivedHandler(BluetoothLEAdvertisementWatcher _, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            using var device = BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress).AsTask().Result;

            if (!await (IsValidDeviceAsync(device)))
                return;

            var controller = new HubController
            {
                SelectedBleDeviceId = device.DeviceId
            };

            lock (_lock)
            {
                if (_controllers.Any(c => c.SelectedBleDeviceId == controller.SelectedBleDeviceId))
                    return;
                _controllers.Add(controller);
            }

            await _discoveryHandler(new DiscoveredDevice
            {
                Name = device.Name,
                BluetoothDeviceId = device.DeviceId
            });
            await ConnectAsync(controller, _connectionHandler);
        }

        private static async Task<bool> IsValidDeviceAsync(BluetoothLEDevice device)
        {
            return device != null &&
                    (await device.GetGattServicesAsync()).Services.Any(s => s.Uuid == new Guid(_legoHubService));
        }

        private async Task ConnectAsync(IHubController controller, Func<IHubController, string, Task> connectionHandler)
        {
            var bluetoothLEDevice = await GetDeviceAsync(controller.SelectedBleDeviceId);
            if (bluetoothLEDevice == null)
            {
                await connectionHandler(null, "Failed to connect");
                return;
            }

            var result = await bluetoothLEDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success)
            {
                await connectionHandler(null, "Device unreachable");
                return;
            }

            var service = result.Services.FirstOrDefault(s => s.Uuid == new Guid(_legoHubService));
            if (service == default)
            {
                await connectionHandler(null, "Wrong device type");
                return;
            }

            var characteristic = (await service.GetCharacteristicsForUuidAsync(new Guid(_legoHubCharacteristic))).Characteristics.Single();
            controller.HubCharacteristic = characteristic;

            // Ensure Hub type is detected
            controller.AddEventHandler(new SystemTypeUpdateHubTypeEventHandler(controller));
            controller.AddEventHandler(new RemoteButtonStateUpdateHubTypeEventHandler(controller));
            controller.AddEventHandler(new InternalMotorStateUpdateHubTypeEventHandler(controller));
            controller.AddEventHandler(new DisconnectEventHandler(controller, OnControllerDisconnect));

            await controller.ConnectAsync(_notificationHandler);
            await controller.ExecuteCommandAsync(new HubFirmwareCommand());

            // Avoid race condition where System Type has not yet returned
            var counter = 0;
            while (controller.Hub == null && counter < 20)
            {
                await Task.Delay(50);
                counter++;
            }

            await connectionHandler(controller, "");
        }

        private async Task OnControllerDisconnect(IHubController hubController)
        {
            lock (_lock)
            {
                _controllers.Remove(hubController);
            }
            await _disconnectHandler(hubController);
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
