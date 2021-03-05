using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.EventHandlers;
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
    public class BluetoothLowEnergyAdapter
    {
        private BluetoothLEAdvertisementWatcher _watcher;

        private List<HubController> _controllers;

        public bool Scanning { get; private set; } = false;


        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)

        private const string LegoHubService = "00001623-1212-EFDE-1623-785FEABCD123";
        private const string LegoHubCharacteristic = "00001624-1212-EFDE-1623-785FEABCD123";

        private object _lock = new object();

        private Func<DiscoveredDevice, Task> _discoveryHandler;
        private Func<HubController, string, Task> _connectionHandler;
        private Func<HubController, string, Task> _notificationHandler;

        public BluetoothLowEnergyAdapter(Func<DiscoveredDevice, Task> discoveryHandler,
                                         Func<HubController, string, Task> connectionHandler,
                                         Func<HubController, string, Task> notificationHandler)
        {
            _discoveryHandler = discoveryHandler;
            _connectionHandler = connectionHandler;
            _notificationHandler = notificationHandler;
            _controllers = new List<HubController>();
        }

        public void StartBleDeviceWatcher()
        {
            _watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            _watcher.Received += ReceivedHandler;

            _watcher.Start();
            Scanning = true;
        }

        public void StopBleDeviceWatcher()
        {
            if (_watcher != null)
            {
                _watcher.Received -= ReceivedHandler;
                _watcher.Stop();
                Scanning = false;
                _watcher = null;
            }
        }

        private async void ReceivedHandler(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            using (var device = BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress).AsTask().Result)
            {
                HubController controller;

                if (device == null)
                    return;

                if (!(await device.GetGattServicesAsync()).Services.Any(s => s.Uuid == new Guid(LegoHubService)))
                    return;

                controller = new HubController
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
                await Connect(controller, _connectionHandler);
            }
        }

        private async Task<bool> Connect(HubController controller, Func<HubController, string, Task> connectionHandler)
        {
            BluetoothLEDevice bluetoothLEDevice;

            try
            {
                bluetoothLEDevice = await BluetoothLEDevice.FromIdAsync(controller.SelectedBleDeviceId);
                if (bluetoothLEDevice == null)
                {
                    await connectionHandler(null, "Failed to connect to device.");
                    return false;
                }
            }
            catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
            {
                await connectionHandler(null, "Bluetooth radio is not on.");
                return false;
            }

            if (bluetoothLEDevice != null)
            {
                GattDeviceServicesResult result = await bluetoothLEDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    var service = result.Services.FirstOrDefault(s => s.Uuid == new Guid(LegoHubService));

                    if (service != default)
                    {
                        var characteristics = await service.GetCharacteristicsForUuidAsync(new Guid(LegoHubCharacteristic));
                        foreach (var characteristic in characteristics.Characteristics)
                        {
                            controller.HubCharacteristic = characteristic;

                            // Ensure Hub type is detected
                            controller.AddEventHandler(new SystemTypeUpdateHubTypeEventHandler(controller));
                            controller.AddEventHandler(new RemoteButtonStateUpdateHubTypeEventHandler(controller));
                            controller.AddEventHandler(new InternalMotorStateUpdateHubTypeEventHandler(controller));

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
                    }
                    return true;
                }
                else
                {
                    await connectionHandler(null, "Device unreachable");
                    return false;
                }
            }
            else
            {
                await connectionHandler(null, "Failed to connect.");
                return false;
            }
        }


    }
}
