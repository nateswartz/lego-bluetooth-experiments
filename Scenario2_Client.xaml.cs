//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using SDKTemplate.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SDKTemplate
{
    // This scenario connects to the device selected in the "Discover
    // GATT Servers" scenario and communicates with it.
    // Note that this scenario is rather artificial because it communicates
    // with an unknown service with unknown characteristics.
    // In practice, your app will be interested in a specific service with
    // a specific characteristic.
    public sealed partial class Scenario2_Client : Page
    {
        private MainPage rootPage = MainPage.Current;

        private DeviceWatcher deviceWatcher;

        private BluetoothLEDevice bluetoothLeDevice = null;

        private string selectedBleDeviceId;

        private GattDeviceService moveHubService;
        private GattCharacteristic moveHubCharacteristic;

        private List<string> notifications = new List<string>();

        private string colorDistanceSensorPort;
        private string externalMotorPort;

        #region Error Codes
        readonly int E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED = unchecked((int)0x80650003);
        readonly int E_BLUETOOTH_ATT_INVALID_PDU = unchecked((int)0x80650004);
        readonly int E_ACCESSDENIED = unchecked((int)0x80070005);
        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)
        #endregion

        List<LEDColor> colors = LEDColors.All;
        List<Motor> motors = Motors.All;

        #region UI Code
        public Scenario2_Client()
        {
            InitializeComponent();
        }

        private async void LEDColorsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null) return;
            var color = (LEDColor)((ComboBox)sender).SelectedItem;
            rootPage.NotifyUser($"The selected item is {color}", NotifyType.StatusMessage);

            await SetLEDColor(color);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedBleDeviceId))
            {
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = false;
                ToggleButtons(false);
            }
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            var success = await DisconnectBluetoothLEDeviceAsync();
            if (!success)
            {
                rootPage.NotifyUser("Error: Unable to reset app state", NotifyType.ErrorMessage);
            }
        }

        private void ScanButton_Click()
        {
            if (deviceWatcher == null)
            {
                StartBleDeviceWatcher();
                ScanButton.Content = "Stop scanning";
                rootPage.NotifyUser($"Device watcher started.", NotifyType.StatusMessage);
            }
            else
            {
                StopBleDeviceWatcher();
                ScanButton.Content = "Start scanning";
                rootPage.NotifyUser($"Device watcher stopped.", NotifyType.StatusMessage);
            }
        }

        private void ToggleButtons(bool state)
        {
            CharacteristicWriteValue.IsEnabled = state;
            WriteHexButton.IsEnabled = state;

            LEDColorsCombo.IsEnabled = state;
            WriteHexButton.IsEnabled = state;
            MotorsCombo.IsEnabled = state;
            RunTimeText.IsEnabled = state;
            MotorPowerSlider.IsEnabled = state;
            RunMotorButton.IsEnabled = state;
            DirectionToggle.IsEnabled = state;

            EnableButtonNotificationsButton.IsEnabled = state;
            EnableColorDistanceNotificationsButton.IsEnabled = state;
            EnableExternalMotorNotificationsButton.IsEnabled = state;
            GetHubName.IsEnabled = state;
        }
        #endregion

        #region Device discovery

        /// <summary>
        /// Starts a device watcher that looks for all nearby Bluetooth devices (paired or unpaired). 
        /// Attaches event handlers to populate the device collection.
        /// </summary>
        private void StartBleDeviceWatcher()
        {
            // Additional properties we would like about the device.
            // Property strings are documented here https://msdn.microsoft.com/en-us/library/windows/desktop/ff521659(v=vs.85).aspx
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

            // BT_Code: Example showing paired and non-paired in a single query.
            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        aqsAllBluetoothLEDevices,
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start the watcher.
            deviceWatcher.Start();
        }

        /// <summary>
        /// Stops watching for all nearby Bluetooth devices.
        /// </summary>
        private void StopBleDeviceWatcher()
        {
            if (deviceWatcher != null)
            {
                // Unregister the event handlers.
                deviceWatcher.Added -= DeviceWatcher_Added;
                deviceWatcher.Updated -= DeviceWatcher_Updated;
                deviceWatcher.Removed -= DeviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watcher.
                deviceWatcher.Stop();
                deviceWatcher = null;
            }
        }

        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                Debug.WriteLine(String.Format("Added Called for ID: {0} Name: {1}", deviceInfo.Id, deviceInfo.Name));

                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    if (string.IsNullOrEmpty(selectedBleDeviceId) && deviceInfo.Name == "LEGO Move Hub")
                    {
                        string s = string.Join(";", deviceInfo.Properties.Select(x => x.Key + "=" + x.Value));
                        Debug.WriteLine(s);
                        selectedBleDeviceId = deviceInfo.Id;
                        ConnectButton.IsEnabled = true;
                        Debug.WriteLine(String.Format($"Found Move Hub: {deviceInfo.Id}"));
                        StopBleDeviceWatcher();
                        ScanButton.Content = "Start scanning";
                        rootPage.NotifyUser($"Device watcher stopped.", NotifyType.StatusMessage);
                        await Connect();
                    }
                }
            });
        }

        private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Updated {0}{1}", deviceInfoUpdate.Id, ""));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        // Do we need to do anything?
                    }
                }
            });
        }

        private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Removed {0}{1}", deviceInfoUpdate.Id, ""));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        // Do we need to do anything?
                    }
                }
            });
        }

        private async void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    // Do we need to do anything?
                }
            });
        }

        private async void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    rootPage.NotifyUser($"No longer watching for devices.",
                            sender.Status == DeviceWatcherStatus.Aborted ? NotifyType.ErrorMessage : NotifyType.StatusMessage);
                }
            });
        }
        #endregion

        #region Enumerating Services
        private async Task<bool> DisconnectBluetoothLEDeviceAsync()
        {
            if (subscribedForNotifications)
            {
                // Need to clear the CCCD from the remote device so we stop receiving notifications
                var result = await moveHubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                if (result != GattCommunicationStatus.Success)
                {
                    return false;
                }
                else
                {
                    moveHubCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                    subscribedForNotifications = false;
                }
            }
            bluetoothLeDevice?.Dispose();
            bluetoothLeDevice = null;
            moveHubService?.Dispose();
            moveHubService = null;
            return true;
        }

        private async void DisconnectButton_Click()
        {
            if (await DisconnectBluetoothLEDeviceAsync())
            {
                ConnectButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;
                ToggleButtons(false);
            }
        }

        private async void ConnectButton_Click()
        {
            await Connect();
        }

        private async Task<bool> Connect()
        {
            ConnectButton.IsEnabled = false;

            if (!await DisconnectBluetoothLEDeviceAsync())
            {
                rootPage.NotifyUser("Error: Unable to reset state, try again.", NotifyType.ErrorMessage);
                ConnectButton.IsEnabled = false;
                return false;
            }

            try
            {
                // BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
                bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(selectedBleDeviceId);

                if (bluetoothLeDevice == null)
                {
                    rootPage.NotifyUser("Failed to connect to device.", NotifyType.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
            {
                rootPage.NotifyUser("Bluetooth radio is not on.", NotifyType.ErrorMessage);
                return false;
            }

            if (bluetoothLeDevice != null)
            {
                // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
                // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
                // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.
                GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    rootPage.NotifyUser(String.Format("Found {0} services", services.Count), NotifyType.StatusMessage);
                    Debug.WriteLine(String.Format("Found {0} services", services.Count));

                    foreach (var service in services)
                    {
                        if (service.Uuid == new Guid("00001623-1212-efde-1623-785feabcd123"))
                        {
                            moveHubService = service;
                            var characteristics = await service.GetCharacteristicsForUuidAsync(new Guid("00001624-1212-efde-1623-785feabcd123"));
                            foreach (var characteristic in characteristics.Characteristics)
                            {
                                moveHubCharacteristic = characteristic;
                                ToggleButtons(true);
                                DisconnectButton.IsEnabled = true;
                                ConnectButton.IsEnabled = false;
                                await ToggleSubscribedForNotifications();
                                EnableCharacteristicPanels(characteristic.CharacteristicProperties);
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    rootPage.NotifyUser("Device unreachable", NotifyType.ErrorMessage);
                    return false;
                }
            }
            else
            {
                ConnectButton.IsEnabled = true;
                return false;
            }
        }
        #endregion

        private void AddValueChangedHandler()
        {
            if (!subscribedForNotifications)
            {
                moveHubCharacteristic.ValueChanged += Characteristic_ValueChanged;
                subscribedForNotifications = true;
            }
        }

        private void RemoveValueChangedHandler()
        {
            if (subscribedForNotifications)
            {
                moveHubCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                subscribedForNotifications = false;
            }
        }

        private void SetVisibility(UIElement element, bool visible)
        {
            element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void EnableCharacteristicPanels(GattCharacteristicProperties properties)
        {
            // BT_Code: Hide the controls which do not apply to this characteristic.
            SetVisibility(CharacteristicWritePanel,
                properties.HasFlag(GattCharacteristicProperties.Write) ||
                properties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse));
            CharacteristicWriteValue.Text = "";
        }

        private async void WriteHexButton_Click()
        {
            if (!String.IsNullOrEmpty(CharacteristicWriteValue.Text))
            {
                var bytes = Enumerable.Range(0, CharacteristicWriteValue.Text.Length)
                                      .Where(x => x % 2 == 0)
                                      .Select(x => Convert.ToByte(CharacteristicWriteValue.Text.Substring(x, 2), 16))
                                      .ToArray();

                var writer = new DataWriter();
                writer.ByteOrder = ByteOrder.LittleEndian;
                writer.WriteBytes(bytes);

                var writeSuccessful = await WriteBufferToMoveHubCharacteristicAsync(writer.DetachBuffer());
            }
            else
            {
                rootPage.NotifyUser("No data to write to device", NotifyType.ErrorMessage);
            }
        }

        private async Task<bool> SetLEDColor(LEDColor color)
        {
            var command = "08008132115100" + color.Code;
            return await SetHexValue(command);
        }

        private async void EnableButtonNotificationsButton_Click()
        {
            var command = "0500010202";
            await SetHexValue(command);
        }

        private async void EnableColorDistanceNotificationsButton_Click()
        {
            var port = colorDistanceSensorPort;
            var state = "01"; // 01 - On; 02 - Off;
            var sensorMode = "08"; 
            var command = $"0a0041{port}{sensorMode}01000000{state}";
            await SetHexValue(command);
        }

        private async void EnableExternalMotorNotificationsButton_Click()
        {
            var port = externalMotorPort;
            var state = "01"; // 01 - On; 02 - Off;
            var sensorMode = "02";
            var command = $"0a0041{port}{sensorMode}01000000{state}";
            await SetHexValue(command);
        }

        private async void GetHubNameButton_Click()
        {
            await SetHexValue("060001010200");
        }

        private async void RunMotorButton_Click()
        {
            var hasRunTime = int.TryParse(RunTimeText.Text, out int runTime);
            var clockwise = DirectionToggle.IsOn;
            if (MotorsCombo.SelectedItem != null && hasRunTime)
            {
                await RunMotor((Motor)MotorsCombo.SelectedItem, (int)MotorPowerSlider.Value, runTime, clockwise);
            }
            else
            {
                rootPage.NotifyUser("Motor option missing", NotifyType.ErrorMessage);
            }
        }

        private async Task<bool> RunMotor(Motor motor, int powerPercentage = 100, int timeInMS = 1000, bool clockwise = true)
        {
            string motorToRun = motor.Code;
            if (motor == Motors.External)
            {
                motorToRun = externalMotorPort;
            }

            // For time, LSB first
            var time = timeInMS.ToString("X").PadLeft(4, '0');
            time = $"{time[2]}{time[3]}{time[0]}{time[1]}";
            var power = "";
            if (clockwise)
            {
                power = powerPercentage.ToString("X");
            }
            else
            {
                power = (255 - powerPercentage).ToString("X");
            }
            power = power.PadLeft(2, '0');
            var command = $"0c0081{motorToRun}1109{time}{power}647f03";
            return await SetHexValue(command);
        }

        private async Task<bool> SetHexValue(string hex)
        {
            var bytes = Enumerable.Range(0, hex.Length)
                          .Where(x => x % 2 == 0)
                          .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                          .ToArray();

            var writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteBytes(bytes);

            var writeSuccessful = await WriteBufferToMoveHubCharacteristicAsync(writer.DetachBuffer());
            return writeSuccessful;
        }

        private async Task<bool> WriteBufferToMoveHubCharacteristicAsync(IBuffer buffer)
        {
            try
            {
                // BT_Code: Writes the value from the buffer to the characteristic.
                var result = await moveHubCharacteristic.WriteValueWithResultAsync(buffer);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    rootPage.NotifyUser("Successfully wrote value to device", NotifyType.StatusMessage);
                    return true;
                }
                else
                {
                    rootPage.NotifyUser($"Write failed: {result.Status}", NotifyType.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex) when (ex.HResult == E_BLUETOOTH_ATT_INVALID_PDU)
            {
                rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                return false;
            }
            catch (Exception ex) when (ex.HResult == E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED || ex.HResult == E_ACCESSDENIED)
            {
                // This usually happens when a device reports that it support writing, but it actually doesn't.
                rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                return false;
            }
        }

        private bool subscribedForNotifications = false;

        private async Task<bool> ToggleSubscribedForNotifications()
        {
            if (!subscribedForNotifications)
            {
                // initialize status
                GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
                var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;

                try
                {
                    AddValueChangedHandler();
                    // BT_Code: Must write the CCCD in order for server to send indications.
                    // We receive them in the ValueChanged event handler.
                    status = await moveHubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

                    if (status == GattCommunicationStatus.Success)
                    {
                        rootPage.NotifyUser("Successfully subscribed for value changes", NotifyType.StatusMessage);
                        return true;
                    }
                    else
                    {
                        RemoveValueChangedHandler();
                        rootPage.NotifyUser($"Error registering for value changes: {status}", NotifyType.ErrorMessage);
                        return false;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support indicate, but it actually doesn't.
                    rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
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
                            moveHubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                GattClientCharacteristicConfigurationDescriptorValue.None);
                    if (result == GattCommunicationStatus.Success)
                    {
                        subscribedForNotifications = false;
                        RemoveValueChangedHandler();
                        rootPage.NotifyUser("Successfully un-registered for notifications", NotifyType.StatusMessage);
                        return true;
                    }
                    else
                    {
                        rootPage.NotifyUser($"Error un-registering for notifications: {result}", NotifyType.ErrorMessage);
                        return false;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support notify, but it actually doesn't.
                    rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                    return false;
                }
            }
        }

        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] output = new byte[args.CharacteristicValue.Length];
            var dataReader = DataReader.FromBuffer(args.CharacteristicValue);
            dataReader.ReadBytes(output);
            var notification = ByteArrayToString(output);
            var message = DecodeNotification(notification);
            notifications.Add(message);
            if (notifications.Count > 10)
            {
                notifications.RemoveAt(0);
            }
            Debug.WriteLine(message);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => CharacteristicLatestValue.Text = string.Join(Environment.NewLine, notifications));
        }

        private string DecodeNotification(string notification)
        {
            var results = "";
            var messageType = notification.Substring(4, 2);
            var deviceType = "";
            switch (messageType)
            {
                case "01": // Device Info
                    deviceType = notification.Substring(6, 2);
                    switch (deviceType)
                    {
                        case "01":
                            results = "Hub Name: ";
                            var data = HexStringToByteArray(notification.Substring(10));
                            results += Encoding.ASCII.GetString(data);
                            break;
                        case "02":
                            results = "Button State: " + (notification.Substring(10, 2) == "00" ? "Released" : "Pressed");
                            break;
                    }
                    break;
                case "04": // Port Info
                    var port = notification.Substring(6, 2);
                    deviceType = notification.Substring(10, 2);
                    switch (deviceType)
                    {
                        case "17":
                            results = "LED Notification";
                            break;
                        case "25":
                            colorDistanceSensorPort = port;
                            results = $"Color Distance Sensor on port: " + (port == "01" ? "C" : "D");
                            break;
                        case "26":
                            externalMotorPort = port;
                            results = "External Motor on port: " + (port == "01" ? "C" : "D");
                            break;
                        case "27":
                            results = "Internal Motor notification";
                            break;
                    }
                    break;
            }
            return results == "" ? $"Notification value: {notification}" : results;
        }

        private static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
              .Where(x => x % 2 == 0)
              .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
              .ToArray();
        }
    }
}
