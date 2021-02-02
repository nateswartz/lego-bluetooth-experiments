using LegoBoostController.Commands.Boost;
using LegoBoostController.Controllers;
using LegoBoostController.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace LegoBoostController
{
    public class BluetoothAdapter
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

        public BluetoothAdapter()
        {
            _controller = new HubController();
            _controller2 = new HubController();
            _notificationManager = new NotificationManager(_controller);
            _notificationManager2 = new NotificationManager(_controller2);
        }

        public void StartBleDeviceWatcher(Func<string, Task> discoveryHandler, Func<HubController, Task> connectionHandler)
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

                    // TODO: Make controller and controller2 more generic, allow either device to connect
                    if (!_controller.IsConnected && device.Name == "LEGO Move Hub")
                    {
                        //string s = string.Join(";", deviceInfo.Properties.Select(x => x.Key + "=" + x.Value));
                        //Debug.WriteLine(s);
                        _controller.SelectedBleDeviceId = device.DeviceId;
                        //ConnectButton.IsEnabled = true;
                        //Debug.WriteLine(String.Format($"Found Move Hub: {deviceInfo.Id}"));
                        //_rootPage.NotifyUser($"Found Boost Move Hub.", NotifyType.StatusMessage);
                        await discoveryHandler("Boost Move Hub");
                        await Connect(_controller, connectionHandler);
                    }

                    if (!_controller2.IsConnected && device.Name == "Two Port Hub")
                    {
                        //string s = string.Join(";", deviceInfo.Properties.Select(x => x.Key + "=" + x.Value));
                        //Debug.WriteLine(s);
                        _controller2.SelectedBleDeviceId = device.DeviceId;
                        //ConnectButton.IsEnabled = true;
                        //Debug.WriteLine(String.Format($"Found Two Port Hub: {deviceInfo.Id}"));
                        //_rootPage.NotifyUser($"Found Two Port Hub.", NotifyType.StatusMessage);
                        await discoveryHandler("Two Port Hub");
                        await Connect(_controller2, connectionHandler);
                    }
                }
                //Debug.WriteLine(String.Format("Added Called for ID: {0} Name: {1}", deviceInfo.Id, deviceInfo.Name));
            }
        }

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

        private async Task<bool> Connect(HubController controller, Func<HubController, Task> connectionHandler)
        {
            //ConnectButton.IsEnabled = false;
            BluetoothLEDevice bluetoothLEDevice;

            try
            {
                bluetoothLEDevice = await BluetoothLEDevice.FromIdAsync(controller.SelectedBleDeviceId);
                if (bluetoothLEDevice == null)
                {
                    //_rootPage.NotifyUser("Failed to connect to device.", NotifyType.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
            {
                //_rootPage.NotifyUser("Bluetooth radio is not on.", NotifyType.ErrorMessage);
                return false;
            }

            if (bluetoothLEDevice != null)
            {
                GattDeviceServicesResult result = await bluetoothLEDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    //_rootPage.NotifyUser(String.Format("Found {0} services", services.Count), NotifyType.StatusMessage);
                    //Debug.WriteLine(String.Format("Found {0} services", services.Count));

                    foreach (var service in services)
                    {
                        if (service.Uuid == new Guid(LegoHubService))
                        {
                            var characteristics = await service.GetCharacteristicsForUuidAsync(new Guid(LegoHubCharacteristic));
                            foreach (var characteristic in characteristics.Characteristics)
                            {
                                controller.HubCharacteristic = characteristic;
                                //ToggleButtons(true);
                                //DisconnectButton.IsEnabled = true;
                                //ConnectButton.IsEnabled = false;
                                await ToggleSubscribedForNotifications(controller);
                                await controller.ConnectAsync();
                                //_hubs.Add(controller);
                                //if (controller.HubType == HubType.BoostMoveHub)
                                //{
                                //    ToggleControls.IsEnabled = true;
                                //}
                                //EnableCharacteristicPanels();
                                await connectionHandler(controller);
                                await controller.ExecuteCommandAsync(new HubFirmwareCommand());
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    //_rootPage.NotifyUser("Device unreachable", NotifyType.ErrorMessage);
                    return false;
                }
            }
            else
            {
                //ConnectButton.IsEnabled = true;
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
            HubController controller = null;
            NotificationManager notificationManager = null;
            if (sender == _controller.HubCharacteristic)
            {
                controller = _controller;
                notificationManager = _notificationManager;
            }
            else if (sender == _controller2.HubCharacteristic)
            {
                controller = _controller2;
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
