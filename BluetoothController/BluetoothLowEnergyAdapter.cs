using BluetoothController.Commands.Boost;
using BluetoothController.Controllers;
using BluetoothController.EventHandlers;
using BluetoothController.Models;
using System;
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

        private HubController _controller;
        private HubController _controller2;

        public bool Scanning { get; private set; } = false;


        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)

        private const string LegoHubService = "00001623-1212-EFDE-1623-785FEABCD123";
        private const string LegoHubCharacteristic = "00001624-1212-EFDE-1623-785FEABCD123";

        private Func<DiscoveredDevice, Task> _discoveryHandler;
        private Func<HubController, string, Task> _connectionHandler;
        private Func<HubController, string, Task> _notificationHandler;

        public BluetoothLowEnergyAdapter(Func<DiscoveredDevice, Task> discoveryHandler,
                                         Func<HubController, string, Task> connectionHandler,
                                         Func<HubController, string, Task> notificationHandler)
        {
            _controller = new HubController();
            _controller2 = new HubController();
            _discoveryHandler = discoveryHandler;
            _connectionHandler = connectionHandler;
            _notificationHandler = notificationHandler;
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

        // TODO: Verify this is working
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
                if (device == null)
                    return;

                if (string.IsNullOrEmpty(_controller.SelectedBleDeviceId) && device.Name == "LEGO Move Hub")
                {
                    _controller.SelectedBleDeviceId = device.DeviceId;

                    await _discoveryHandler(new DiscoveredDevice
                    {
                        Name = device.Name,
                        BluetoothDeviceId = device.DeviceId
                    });
                    await Connect(_controller, _connectionHandler);
                }

                if (string.IsNullOrEmpty(_controller2.SelectedBleDeviceId) && device.Name == "Two Port Hub")
                {
                    _controller2.SelectedBleDeviceId = device.DeviceId;

                    await _discoveryHandler(new DiscoveredDevice
                    {
                        Name = device.Name,
                        BluetoothDeviceId = device.DeviceId
                    });
                    await Connect(_controller2, _connectionHandler);
                }
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
                            controller.AddEventHandler(new SystemTypeUpdateHubTypeEventHandler(controller));
                            await controller.ConnectAsync(_notificationHandler);
                            await connectionHandler(controller, "");
                            await controller.ExecuteCommandAsync(new HubFirmwareCommand());
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
