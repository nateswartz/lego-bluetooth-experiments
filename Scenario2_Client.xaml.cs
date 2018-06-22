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

using SDKTemplate.Commands;
using SDKTemplate.Models;
using SDKTemplate.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage;
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

        private List<string> notifications = new List<string>();

        private StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

        private PortState _portState;
        private ResponseProcessor _responseProcessor;
        private BoostController _controller;

        private List<ICommand> _commands = 
            new List<ICommand> {
                new MoveCommand(),
                new SpinCommand() };

        private bool syncMotorAndLED = false;

        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

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
            _portState = new PortState();
            _responseProcessor = new ResponseProcessor(_portState);
            _controller = new BoostController(_portState);
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
            ExternalMotorNotificationTypeToggle.IsEnabled = state;

            EnableButtonNotificationsButton.IsEnabled = state;
            ToggleColorDistanceNotificationsButton.IsEnabled = state;
            ToggleExternalMotorNotificationsButton.IsEnabled = state;
            ToggleTiltSensorNotificationsButton.IsEnabled = state;
            GetHubName.IsEnabled = state;
            GetHubFirmware.IsEnabled = state;
            SyncLEDMotorButton.IsEnabled = state;
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
                var result = await _controller.MoveHubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                if (result != GattCommunicationStatus.Success)
                {
                    return false;
                }
                else
                {
                    _controller.MoveHubCharacteristic.ValueChanged -= Characteristic_ValueChanged;
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
                                _controller.MoveHubCharacteristic = characteristic;
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
                _controller.MoveHubCharacteristic.ValueChanged += Characteristic_ValueChanged;
                subscribedForNotifications = true;
            }
        }

        private void RemoveValueChangedHandler()
        {
            if (subscribedForNotifications)
            {
                _controller.MoveHubCharacteristic.ValueChanged -= Characteristic_ValueChanged;
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
            var text = CharacteristicWriteValue.Text;
            if (!String.IsNullOrEmpty(text))
            {
                if (text.Contains(" "))
                {
                    text = text.Replace(" ", "");
                }
                var bytes = Enumerable.Range(0, text.Length)
                                      .Where(x => x % 2 == 0)
                                      .Select(x => Convert.ToByte(text.Substring(x, 2), 16))
                                      .ToArray();

                var writer = new DataWriter();
                writer.ByteOrder = ByteOrder.LittleEndian;
                writer.WriteBytes(bytes);

                var writeSuccessful = _controller.WriteBufferToMoveHubCharacteristicAsync(writer.DetachBuffer());
            }
            else
            {
                rootPage.NotifyUser("No data to write to device", NotifyType.ErrorMessage);
            }
        }

        private async Task<bool> SetLEDColor(LEDColor color)
        {
            var command = "08008132115100" + color.Code;
            return await _controller.SetHexValue(command);
        }

        private async void EnableButtonNotificationsButton_Click()
        {
            var command = "0500010202";
            await _controller.SetHexValue(command);
        }

        private async void ToggleColorDistanceNotificationsButton_Click()
        {
            await ToggleNotification(ToggleColorDistanceNotificationsButton, "Color/Distance", _portState.CurrentColorDistanceSensorPort, "08");
        }

        private async void ToggleExternalMotorNotificationsButton_Click()
        {
            // 01 - Speed; 02 - Angle
            var notificationType = ExternalMotorNotificationTypeToggle.IsOn ? "02" : "01";
            await ToggleNotification(ToggleExternalMotorNotificationsButton, "External Motor", _portState.CurrentExternalMotorPort, notificationType);
        }

        private async void ToggleTiltSensorNotificationsButton_Click()
        {
            await ToggleNotification(ToggleTiltSensorNotificationsButton, "Tilt Sensor", "3a", "04");
        }

        private async Task<bool> ToggleNotification(Button button, string sensorType, string port, string sensorMode)
        {
            string state;
            if (button.Content.ToString() == $"Enable {sensorType} Notifications")
            {
                state = "01"; // 01 - On; 02 - Off;
                button.Content = $"Disable {sensorType} Notifications";
            }
            else
            {
                state = "00"; // 01 - On; 02 - Off;
                button.Content = $"Enable {sensorType} Notifications";
            }
            var command = $"0a0041{port}{sensorMode}01000000{state}";
            return await _controller.SetHexValue(command);
        }

        private async void GetHubNameButton_Click()
        {
            var messageType = "01"; // Device info
            var infoType = "01"; // Name
            var action = "05"; // One-time request
            await _controller.SetHexValue($"0600{messageType}{infoType}{action}00");
        }

        private async void GetHubFirmwareButton_Click()
        {
            var messageType = "01"; // Device info
            var infoType = "03"; // Firmware
            var action = "05"; // One-time request
            await _controller.SetHexValue($"0600{messageType}{infoType}{action}00");
        }

        private async void SyncLEDMotorButton_Click()
        {
            await ToggleNotification(ToggleExternalMotorNotificationsButton, "External Motor", _portState.CurrentExternalMotorPort, "01");
            syncMotorAndLED = !syncMotorAndLED;
            if (syncMotorAndLED)
            {
                SyncLEDMotorButton.Content = "Un-sync LED with Motor";
            }
            else
            {
                SyncLEDMotorButton.Content = "Sync LED with Motor";
            }
        }

        private async void RunMotorButton_Click()
        {
            var hasRunTime = int.TryParse(RunTimeText.Text, out int runTime);
            var clockwise = DirectionToggle.IsOn;
            if (MotorsCombo.SelectedItem != null && hasRunTime)
            {
                await _controller.RunMotor((Motor)MotorsCombo.SelectedItem, (int)MotorPowerSlider.Value, runTime, clockwise);
            }
            else
            {
                rootPage.NotifyUser("Motor option missing", NotifyType.ErrorMessage);
            }
        }

        private async void RunCommandsButton_Click()
        {
            if (!String.IsNullOrEmpty(CommandsText.Text))
            {
                var statements = CommandsText.Text.Split(';');
                foreach (var statement in statements)
                {
                    var commandToRun = Regex.Replace(statement.ToLower(), @"\s+", "");
                    var keyword = commandToRun.Split('(')[0];
                    foreach (var command in _commands)
                    {
                        if (command.Keywords.Any(k => k == keyword))
                        {
                            await command.RunAsync(_controller, commandToRun);
                        }
                    }
                    if (commandToRun.StartsWith("raise"))
                    {
                        Match m = Regex.Match(commandToRun, @"\((\d+)\)");
                        if (m.Groups.Count == 2)
                        {
                            var speed = Convert.ToInt32(m.Groups[1].Value);
                            var time = 21500 / speed;
                            await _controller.RunMotor(Motors.External, speed, time, true);
                        }
                    }
                    else if (commandToRun.StartsWith("lower"))
                    {
                        Match m = Regex.Match(commandToRun, @"\((\d+)\)");
                        if (m.Groups.Count == 2)
                        {
                            var speed = Convert.ToInt32(m.Groups[1].Value);
                            var time = 19500 / speed;
                            await _controller.RunMotor(Motors.External, speed, time, false);
                        }              
                    }
                    else if (commandToRun.StartsWith("led"))
                    {
                        Match m = Regex.Match(commandToRun, @"\((\w+)\)");
                        if (m.Groups.Count == 2)
                        {
                            var color = m.Groups[1].Value;

                            await SetLEDColor(LEDColors.GetByName(color));
                        }
                    }
                    await Task.Delay(500);
                }
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
                    status = await _controller.MoveHubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

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
                            _controller.MoveHubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
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
            var notification = DataConverter.ByteArrayToString(output);
            var message = await DecodeNotification(notification);
            notifications.Add(message);
            if (notifications.Count > 10)
            {
                notifications.RemoveAt(0);
            }
            Debug.WriteLine(message);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => CharacteristicLatestValue.Text = string.Join(Environment.NewLine, notifications));
        }

        private async Task<string> DecodeNotification(string notification)
        {
            StorageFile logFile = await storageFolder.CreateFileAsync("move-hub-notifications.log", CreationCollisionOption.OpenIfExists);

            var response = _responseProcessor.CreateResponse(notification);

            // TODO: Move this somewhere more appropriate
            if (syncMotorAndLED && response.GetType() == typeof(SpeedData))
            {
                var data = (SpeedData)response;
                var color = LEDColors.Red;
                if (data.Speed > 30)
                {
                    color = LEDColors.Green;
                }
                else if (data.Speed > 1)
                {
                    color = LEDColors.Purple;
                }
                SetLEDColor(color);
            }

            var message = response.ToString();

            await semaphore.WaitAsync();
            try
            {
                await FileIO.AppendTextAsync(logFile, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch
            {}
            finally
            {
                semaphore.Release();
            }

            return message;
        }
    }
}
