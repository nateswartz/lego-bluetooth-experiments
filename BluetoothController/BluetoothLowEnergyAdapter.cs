using BluetoothController.Commands.Boost;
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


        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)

        private const string LegoHubService = "00001623-1212-EFDE-1623-785FEABCD123";
        private const string LegoHubCharacteristic = "00001624-1212-EFDE-1623-785FEABCD123";

        public BluetoothLowEnergyAdapter()
        {
            _controller = new HubController();
            _controller2 = new HubController();
            _notificationManager = new NotificationManager(_controller);
            _notificationManager2 = new NotificationManager(_controller2);
        }

        public void StartBleDeviceWatcher(Func<string, Task> discoveryHandler, Func<HubController, NotificationManager, string, Task> connectionHandler)
        {
            _watcher = new BluetoothLEAdvertisementWatcher();
            _watcher.ScanningMode = BluetoothLEScanningMode.Active;

            _watcher.Received += ReceivedHandler;

            _watcher.Start();

            async void ReceivedHandler(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
            {
                using (var device = BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress).AsTask().Result)
                {
                    if (device == null)
                        return;

                    if (!_controller.IsConnected && device.Name == "LEGO Move Hub")
                    {
                        _controller.SelectedBleDeviceId = device.DeviceId;
                        // TODO: Move to Discovery Handler in UWP App ******
                        //ConnectButton.IsEnabled = true;
                        //Debug.WriteLine(String.Format($"Found Move Hub: {deviceInfo.Id}"));
                        //_rootPage.NotifyUser($"Found Boost Move Hub.", NotifyType.StatusMessage);
                        //string s = string.Join(";", deviceInfo.Properties.Select(x => x.Key + "=" + x.Value));
                        //Debug.WriteLine(s);
                        // ************************************************
                        await discoveryHandler("Boost Move Hub");
                        await Connect(_controller, _notificationManager, connectionHandler);
                    }

                    if (!_controller2.IsConnected && device.Name == "Two Port Hub")
                    {
                        _controller2.SelectedBleDeviceId = device.DeviceId;
                        // TODO: Move to Discovery Handler in UWP App ******
                        //string s = string.Join(";", deviceInfo.Properties.Select(x => x.Key + "=" + x.Value));
                        //Debug.WriteLine(s);
                        //ConnectButton.IsEnabled = true;
                        //Debug.WriteLine(String.Format($"Found Two Port Hub: {deviceInfo.Id}"));
                        //_rootPage.NotifyUser($"Found Two Port Hub.", NotifyType.StatusMessage);
                        // ************************************************
                        await discoveryHandler("Two Port Hub");
                        await Connect(_controller2, _notificationManager2, connectionHandler);
                    }
                }
                //Debug.WriteLine(String.Format("Added Called for ID: {0} Name: {1}", deviceInfo.Id, deviceInfo.Name));
            }
        }

        // TODO: Add this back in if needed
        //public void StopBleDeviceWatcher()
        //{
        //    if (_deviceWatcher != null)
        //    {
        //        // Unregister the event handlers.
        //        _deviceWatcher.Added -= DeviceWatcher_Added;

        //        // Stop the watcher.
        //        _deviceWatcher.Stop();
        //        _deviceWatcher = null;
        //    }
        //}

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
                            // TODO: Move to Connection Handler in UWP App ******
                            //ToggleButtons(true);
                            //DisconnectButton.IsEnabled = true;
                            //ConnectButton.IsEnabled = false;
                            //_hubs.Add(controller);
                            //if (controller.HubType == HubType.BoostMoveHub)
                            //{
                            //    ToggleControls.IsEnabled = true;
                            //}
                            //EnableCharacteristicPanels();
                            // ************************************************
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
            //Debug.WriteLine(message);
            //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            //    () => CharacteristicLatestValue.Text = string.Join(Environment.NewLine, _notifications));
        }
    }
}
