using BluetoothController.Commands.Boost;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BluetoothController
{
    public class BluetoothLowEnergyAdapter
    {
        private BluetoothLEAdvertisementWatcher _watcher;

        private HubController _controller;
        private HubController _controller2;

        private NotificationManager _notificationManager;
        private NotificationManager _notificationManager2;
        private List<string> _notifications = new List<string>();

        public bool Scanning { get; private set; } = false;


        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)

        private const string LegoHubService = "00001623-1212-EFDE-1623-785FEABCD123";
        private const string LegoHubCharacteristic = "00001624-1212-EFDE-1623-785FEABCD123";

        private Func<DiscoveredDevice, Task> _discoveryHandler;
        private Func<HubController, NotificationManager, string, Task> _connectionHandler;
        private Func<string, Task> _notificationHandler;

        public BluetoothLowEnergyAdapter(Func<DiscoveredDevice, Task> discoveryHandler,
                                         Func<HubController, NotificationManager, string, Task> connectionHandler,
                                         Func<string, Task> notificationHandler)
        {
            _controller = new HubController();
            _controller2 = new HubController();
            _notificationManager = new NotificationManager(_controller);
            _notificationManager2 = new NotificationManager(_controller2);
            _discoveryHandler = discoveryHandler;
            _connectionHandler = connectionHandler;
            _notificationHandler = notificationHandler;
        }

        public void StartBleDeviceWatcher()
        {
            _watcher = new BluetoothLEAdvertisementWatcher();
            _watcher.ScanningMode = BluetoothLEScanningMode.Active;

            _watcher.Received += ReceivedHandler;

            _watcher.Start();
            Scanning = true;

            async void ReceivedHandler(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
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
                        await Connect(_controller, _notificationManager, _connectionHandler);
                    }

                    if (string.IsNullOrEmpty(_controller2.SelectedBleDeviceId) && device.Name == "Two Port Hub")
                    {
                        _controller2.SelectedBleDeviceId = device.DeviceId;

                        await _discoveryHandler(new DiscoveredDevice
                        {
                            Name = device.Name,
                            BluetoothDeviceId = device.DeviceId
                        });
                        await Connect(_controller2, _notificationManager2, _connectionHandler);
                    }
                }
            }
        }

        public void StopBleDeviceWatcher()
        {
            if (_watcher != null)
            {
                // Stop the watcher.
                _watcher.Stop();
                Scanning = false;
                _watcher = null;
            }
        }

        private async Task<bool> Connect(HubController controller, NotificationManager notificationManager, Func<HubController, NotificationManager, string, Task> connectionHandler)
        {
            BluetoothLEDevice bluetoothLEDevice;

            try
            {
                bluetoothLEDevice = await BluetoothLEDevice.FromIdAsync(controller.SelectedBleDeviceId);
                if (bluetoothLEDevice == null)
                {
                    await connectionHandler(null, null, "Failed to connect to device.");
                    return false;
                }
            }
            catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
            {
                await connectionHandler(null, null, "Bluetooth radio is not on.");
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
                            await ToggleSubscribedForNotifications(controller);
                            await controller.ConnectAsync();
                            await connectionHandler(controller, notificationManager, "");
                            await controller.ExecuteCommandAsync(new HubFirmwareCommand());
                        }
                    }
                    return true;
                }
                else
                {
                    await connectionHandler(null, null, "Device unreachable");
                    return false;
                }
            }
            else
            {
                await connectionHandler(null, null, "Failed to connect.");
                return false;
            }
        }

        private async Task<bool> ToggleSubscribedForNotifications(HubController controller)
        {
            if (!controller.SubscribedForNotifications)
            {
                // initialize status
                GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
                var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;

                try
                {
                    AddValueChangedHandler(controller);
                    // BT_Code: Must write the CCCD in order for server to send indications.
                    // We receive them in the ValueChanged event handler.
                    status = await controller.HubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

                    if (status == GattCommunicationStatus.Success)
                    {
                        //_rootPage.NotifyUser("Successfully subscribed for value changes", NotifyType.StatusMessage);
                        return true;
                    }
                    else
                    {
                        RemoveValueChangedHandler(controller);
                        //_rootPage.NotifyUser($"Error registering for value changes: {status}", NotifyType.ErrorMessage);
                        return false;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support indicate, but it actually doesn't.
                    //_rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                    return false;
                }
            }
            else
            {
                try
                {
                    // BT_Code: Must write the CCCD in order for server to send notifications.
                    // We receive them in the ValueChanged event handler.
                    // Note that this sample configures either Indicate or Notify, but not both.
                    var result = await
                        controller.HubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.None);

                    if (result == GattCommunicationStatus.Success)
                    {
                        controller.SubscribedForNotifications = false;

                        RemoveValueChangedHandler(controller);
                        //_rootPage.NotifyUser("Successfully un-registered for notifications", NotifyType.StatusMessage);
                        return true;
                    }
                    else
                    {
                        //_rootPage.NotifyUser($"Error un-registering for notifications: {result}", NotifyType.ErrorMessage);
                        return false;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it supports notify, but it actually doesn't.
                    //_rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                    return false;
                }
            }
        }

        private void AddValueChangedHandler(HubController controller)
        {
            if (!controller.SubscribedForNotifications)
            {
                controller.HubCharacteristic.ValueChanged += Characteristic_ValueChanged;
                controller.SubscribedForNotifications = true;
            }
        }

        private void RemoveValueChangedHandler(HubController controller)
        {
            if (controller.SubscribedForNotifications)
            {
                controller.HubCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                controller.SubscribedForNotifications = false;
            }
        }

        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            NotificationManager notificationManager;
            if (sender == _controller.HubCharacteristic)
            {
                notificationManager = _notificationManager;
            }
            else if (sender == _controller2.HubCharacteristic)
            {
                notificationManager = _notificationManager2;
            }
            else
            {
                return;
            }

            var output = new byte[args.CharacteristicValue.Length];
            var dataReader = DataReader.FromBuffer(args.CharacteristicValue);
            dataReader.ReadBytes(output);
            var notification = DataConverter.ByteArrayToString(output);
            await notificationManager.ProcessNotification(notification);
            var message = notificationManager.DecodeNotification(notification);
            _notifications.Add(message);
            if (_notifications.Count > 10)
            {
                _notifications.RemoveAt(0);
            }
            await _notificationHandler(message);
        }
    }
}
