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

using LegoBoostController.Commands.Boost;
using LegoBoostController.Controllers;
using LegoBoostController.EventHandlers;
using LegoBoostController.Models;
using LegoBoostController.Responses;
using LegoBoostController.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace LegoBoostController
{
    // This scenario connects to the device selected in the "Discover
    // GATT Servers" scenario and communicates with it.
    // Note that this scenario is rather artificial because it communicates
    // with an unknown service with unknown characteristics.
    // In practice, your app will be interested in a specific service with
    // a specific characteristic.
    public sealed partial class LegoMoveHub_Client : Page
    {
        private MainPage _rootPage = MainPage.Current;

        private DeviceWatcher _deviceWatcher;

        private List<string> _notifications = new List<string>();

        private const string LegoHubService = "00001623-1212-EFDE-1623-785FEABCD123";
        private const string LegoHubCharacteristic = "00001624-1212-EFDE-1623-785FEABCD123";

        private ResponseProcessor _responseProcessor;

        private HubController _controller;
        private string _selectedBleDeviceId;
        private GattDeviceService _moveHubService;
        private BluetoothLEDevice _bluetoothLeDevice = null;
        private PortState _portState;
        private bool _subscribedForNotifications = false;

        private HubController _controller2;
        private string _selectedBleDeviceId2;
        private GattDeviceService _twoPortHubService;
        private BluetoothLEDevice _bluetoothLeDevice2 = null;
        private PortState _portState2;
        private bool _subscribedForNotifications2 = false;

        private NotificationManager _notificationManager;
        private TextCommandsController _textCommandsController;

        #region Error Codes
        readonly int E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED = unchecked((int)0x80650003);
        readonly int E_BLUETOOTH_ATT_INVALID_PDU = unchecked((int)0x80650004);
        readonly int E_ACCESSDENIED = unchecked((int)0x80070005);
        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)
        #endregion

        List<LEDColor> _colors = LEDColors.All;
        List<Motor> _motors = Motors.All;
        List<Robot> _robots = Enum.GetValues(typeof(Robot)).Cast<Robot>().ToList();

        #region UI Code
        public LegoMoveHub_Client()
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            _portState = new PortState();
            _portState2 = new PortState();
            _responseProcessor = new ResponseProcessor(_portState);
            _controller = new HubController(_portState);
            _controller2 = new HubController(_portState2);
            _notificationManager = new NotificationManager(_responseProcessor, storageFolder);
            _textCommandsController = new TextCommandsController(_controller, storageFolder);
            InitializeComponent();
            SampleCommands.Text = "Sample Commands:";
        }

        private async Task LEDColorsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null) return;
            var color = (LEDColor)((ComboBox)sender).SelectedItem;
            _rootPage.NotifyUser($"The selected item is {color}", NotifyType.StatusMessage);

            var command = new LEDBoostCommand(color);
            await _controller.SetHexValueAsync(command.HexCommand);
        }

        private void RobotSelectionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null) return;
            var robot = (Robot)((ComboBox)sender).SelectedItem;
            _rootPage.NotifyUser($"The selected robot is {Enum.GetName(typeof(Robot), robot)}", NotifyType.StatusMessage);

            _textCommandsController.SelectedRobot = robot;
            SampleCommands.Text = _textCommandsController.SampleCommandsText;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedBleDeviceId))
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
                _rootPage.NotifyUser("Error: Unable to reset app state", NotifyType.ErrorMessage);
            }
        }

        private void ScanButton_Click()
        {
            if (_deviceWatcher == null)
            {
                StartBleDeviceWatcher();
                ScanButton.Content = "Stop scanning";
                _rootPage.NotifyUser($"Device watcher started.", NotifyType.StatusMessage);
            }
            else
            {
                StopBleDeviceWatcher();
                ScanButton.Content = "Start scanning";
                _rootPage.NotifyUser($"Device watcher stopped.", NotifyType.StatusMessage);
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

            ToggleButtonNotificationsButton.IsEnabled = state;
            ToggleColorDistanceNotificationsButton.IsEnabled = state;
            ToggleExternalMotorNotificationsButton.IsEnabled = state;
            ToggleTiltSensorNotificationsButton.IsEnabled = state;
            GetHubName.IsEnabled = state;
            GetHubFirmware.IsEnabled = state;
            ToggleHubBatteryStatus.IsEnabled = state;
            SyncLEDMotorButton.IsEnabled = state;
            SyncLEDButtonButton.IsEnabled = state;
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

            _deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        aqsAllBluetoothLEDevices,
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            _deviceWatcher.Added += DeviceWatcher_Added;
            _deviceWatcher.Updated += DeviceWatcher_Updated;
            _deviceWatcher.Removed += DeviceWatcher_Removed;
            _deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            _deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start the watcher.
            _deviceWatcher.Start();
        }

        /// <summary>
        /// Stops watching for all nearby Bluetooth devices.
        /// </summary>
        private void StopBleDeviceWatcher()
        {
            if (_deviceWatcher != null)
            {
                // Unregister the event handlers.
                _deviceWatcher.Added -= DeviceWatcher_Added;
                _deviceWatcher.Updated -= DeviceWatcher_Updated;
                _deviceWatcher.Removed -= DeviceWatcher_Removed;
                _deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                _deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watcher.
                _deviceWatcher.Stop();
                _deviceWatcher = null;
            }
        }

        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                Debug.WriteLine(String.Format("Added Called for ID: {0} Name: {1}", deviceInfo.Id, deviceInfo.Name));

                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == _deviceWatcher)
                {
                    if (string.IsNullOrEmpty(_selectedBleDeviceId) && deviceInfo.Name == "LEGO Move Hub")
                    {
                        string s = string.Join(";", deviceInfo.Properties.Select(x => x.Key + "=" + x.Value));
                        Debug.WriteLine(s);
                        _selectedBleDeviceId = deviceInfo.Id;
                        ConnectButton.IsEnabled = true;
                        Debug.WriteLine(String.Format($"Found Move Hub: {deviceInfo.Id}"));
                        _rootPage.NotifyUser($"Found Boost Move Hub.", NotifyType.StatusMessage);
                        await Connect();
                    }

                    if (string.IsNullOrEmpty(_selectedBleDeviceId2) && deviceInfo.Name == "HUB NO.4")
                    {
                        string s = string.Join(";", deviceInfo.Properties.Select(x => x.Key + "=" + x.Value));
                        Debug.WriteLine(s);
                        _selectedBleDeviceId2 = deviceInfo.Id;
                        ConnectButton.IsEnabled = true;
                        Debug.WriteLine(String.Format($"Found Two Port Hub: {deviceInfo.Id}"));
                        _rootPage.NotifyUser($"Found Two Port Hub.", NotifyType.StatusMessage);
                        await Connect(true);
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
                    if (sender == _deviceWatcher)
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
                    if (sender == _deviceWatcher)
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
                if (sender == _deviceWatcher)
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
                if (sender == _deviceWatcher)
                {
                    _rootPage.NotifyUser($"No longer watching for devices.",
                            sender.Status == DeviceWatcherStatus.Aborted ? NotifyType.ErrorMessage : NotifyType.StatusMessage);
                }
            });
        }
        #endregion

        #region Enumerating Services
        private async Task<bool> DisconnectBluetoothLEDeviceAsync()
        {
            if (_subscribedForNotifications)
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
                    _subscribedForNotifications = false;
                }
            }
            _bluetoothLeDevice?.Dispose();
            _bluetoothLeDevice = null;
            _moveHubService?.Dispose();
            _moveHubService = null;
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

        private async Task<bool> Connect(bool secondHub = false)
        {
            ConnectButton.IsEnabled = false;

            //if (!await DisconnectBluetoothLEDeviceAsync())
            //{
            //    _rootPage.NotifyUser("Error: Unable to reset state, try again.", NotifyType.ErrorMessage);
            //    ConnectButton.IsEnabled = false;
            //    return false;
            //}

            try
            {
                if (secondHub)
                {
                    _bluetoothLeDevice2 = await BluetoothLEDevice.FromIdAsync(_selectedBleDeviceId2);
                    if (_bluetoothLeDevice2 == null)
                    {
                        _rootPage.NotifyUser("Failed to connect to device.", NotifyType.ErrorMessage);
                        return false;
                    }
                }
                else
                {
                    _bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(_selectedBleDeviceId);
                    if (_bluetoothLeDevice == null)
                    {
                        _rootPage.NotifyUser("Failed to connect to device.", NotifyType.ErrorMessage);
                        return false;
                    }
                }
            }
            catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
            {
                _rootPage.NotifyUser("Bluetooth radio is not on.", NotifyType.ErrorMessage);
                return false;
            }

            if (!secondHub && _bluetoothLeDevice != null)
            {
                GattDeviceServicesResult result = await _bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    _rootPage.NotifyUser(String.Format("Found {0} services", services.Count), NotifyType.StatusMessage);
                    Debug.WriteLine(String.Format("Found {0} services", services.Count));

                    foreach (var service in services)
                    {
                        if (service.Uuid == new Guid(LegoHubService))
                        {
                            _moveHubService = service;
                            var characteristics = await service.GetCharacteristicsForUuidAsync(new Guid(LegoHubCharacteristic));
                            foreach (var characteristic in characteristics.Characteristics)
                            {
                                _controller.MoveHubCharacteristic = characteristic;
                                ToggleButtons(true);
                                DisconnectButton.IsEnabled = true;
                                ConnectButton.IsEnabled = false;
                                await ToggleSubscribedForNotifications();
                                EnableCharacteristicPanels(characteristic.CharacteristicProperties);
                                await _controller.ExecuteCommandAsync(new HubFirmwareCommand());
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    _rootPage.NotifyUser("Device unreachable", NotifyType.ErrorMessage);
                    return false;
                }
            }
            else if (secondHub && _bluetoothLeDevice2 != null)
            {
                GattDeviceServicesResult result = await _bluetoothLeDevice2.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    _rootPage.NotifyUser(String.Format("Found {0} services", services.Count), NotifyType.StatusMessage);
                    Debug.WriteLine(String.Format("Found {0} services", services.Count));

                    foreach (var service in services)
                    {
                        if (service.Uuid == new Guid(LegoHubService))
                        {
                            _twoPortHubService = service;
                            var characteristics = await service.GetCharacteristicsForUuidAsync(new Guid(LegoHubCharacteristic));
                            foreach (var characteristic in characteristics.Characteristics)
                            {
                                _controller2.MoveHubCharacteristic = characteristic;
                                //ToggleButtons(true);
                                //DisconnectButton.IsEnabled = true;
                                //ConnectButton.IsEnabled = false;
                                await ToggleSubscribedForNotifications(true);
                                //EnableCharacteristicPanels(characteristic.CharacteristicProperties);
                                await _controller2.ExecuteCommandAsync(new HubFirmwareCommand());
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    _rootPage.NotifyUser("Device unreachable", NotifyType.ErrorMessage);
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

        private void ToggleControls_Click()
        {
            var showAdvanced = ToggleControls.IsOn;
            if (showAdvanced)
            {
                NotificationControlsPanel.Visibility = Visibility.Collapsed;
                FreeformCommandsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                NotificationControlsPanel.Visibility = Visibility.Visible;
                FreeformCommandsPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void AddValueChangedHandler(bool isSecondHub = false)
        {
            if (!isSecondHub)
            {
                if (!_subscribedForNotifications)
                {
                    _controller.MoveHubCharacteristic.ValueChanged += Characteristic_ValueChanged;
                    _subscribedForNotifications = true;
                }
            }
            else
            {
                if (!_subscribedForNotifications2)
                {
                    _controller2.MoveHubCharacteristic.ValueChanged += Characteristic_ValueChanged;
                    _subscribedForNotifications2 = true;
                }
            }

        }

        private void RemoveValueChangedHandler(bool isSecondHub = false)
        {
            if (!isSecondHub)
            {
                if (_subscribedForNotifications)
                {
                    _controller.MoveHubCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                    _subscribedForNotifications = false;
                }
            }
            else
            {
                if (_subscribedForNotifications2)
                {
                    _controller2.MoveHubCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                    _subscribedForNotifications2 = false;
                }
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
            if (!string.IsNullOrEmpty(text))
            {
                await _controller.SetHexValueAsync(text);
            }
            else
            {
                _rootPage.NotifyUser("No data to write to device", NotifyType.ErrorMessage);
            }
        }

        private async void ToggleButtonNotificationsButton_Click()
        {
            bool notificationsEnabled;
            var button = ToggleButtonNotificationsButton;
            if (button.Content.ToString() == $"Enable Button Notifications")
            {
                notificationsEnabled = false;
                button.Content = $"Disable Button Notifications";
            }
            else
            {
                notificationsEnabled = true;
                button.Content = $"Enable Button Notifications";
            }
            var command = new ButtonNotificationsCommand(notificationsEnabled);
            await _controller.ExecuteCommandAsync(command);
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
            bool notificationsEnabled;
            if (button.Content.ToString() == $"Enable {sensorType} Notifications")
            {
                notificationsEnabled = false;
                button.Content = $"Disable {sensorType} Notifications";
            }
            else
            {
                notificationsEnabled = true;
                button.Content = $"Enable {sensorType} Notifications";
            }
            var command = new ToggleNotificationsCommand(notificationsEnabled, port, sensorMode);
            return await _controller.ExecuteCommandAsync(command);
        }

        private async void GetHubNameButton_Click()
        {
            await _controller.ExecuteCommandAsync(new HubNameCommand());
        }

        private async void GetHubFirmwareButton_Click()
        {
            await _controller.ExecuteCommandAsync(new HubFirmwareCommand());
        }

        private async void GetHub2FirmwareButton_Click()
        {
            await _controller2.ExecuteCommandAsync(new HubFirmwareCommand());
        }

        private async void ToggleHubBatteryStatusButton_Click()
        {
            bool notificationsEnabled;
            var button = ToggleHubBatteryStatus;
            if (button.Content.ToString() == $"Enable Battery Notifications")
            {
                notificationsEnabled = false;
                button.Content = $"Disable Battery Notifications";
            }
            else
            {
                notificationsEnabled = true;
                button.Content = $"Enable Battery Notifications";
            }
            await _controller.ExecuteCommandAsync(new HubVoltageNotificationsCommand(notificationsEnabled));
        }

        private async void SyncLEDMotorButton_Click()
        {
            await ToggleNotification(ToggleExternalMotorNotificationsButton, "External Motor", _portState.CurrentExternalMotorPort, "01");

            if (!_notificationManager.IsHandlerRegistered(typeof(ExternalMotorState), typeof(MotorToLEDEventHandler)))
            {
                SyncLEDMotorButton.Content = "Un-sync LED with Motor";
                _notificationManager.AddEventHandler(new MotorToLEDEventHandler(_controller));
            }
            else
            {
                SyncLEDMotorButton.Content = "Sync LED with Motor";
                _notificationManager.RemoveEventHandler(new MotorToLEDEventHandler(_controller));
            }
        }

        private void SyncLEDButtonButton_Click()
        {
            if (!_notificationManager.IsHandlerRegistered(typeof(ButtonStateMessage), typeof(ButtonToLEDEventHandler)))
            {
                SyncLEDButtonButton.Content = "Un-sync LED with Motor";
                _notificationManager.AddEventHandler(new ButtonToLEDEventHandler(_controller));

            }
            else
            {
                SyncLEDButtonButton.Content = "Sync LED with Motor";
                _notificationManager.RemoveEventHandler(new ButtonToLEDEventHandler(_controller));
            }
        }

        private async void RunMotorButton_Click()
        {
            var hasRunTime = int.TryParse(RunTimeText.Text, out int runTime);
            var clockwise = DirectionToggle.IsOn;
            if (MotorsCombo.SelectedItem != null && hasRunTime)
            {
                var command = new MotorBoostCommand((Motor)MotorsCombo.SelectedItem, (int)MotorPowerSlider.Value, runTime, clockwise, _controller.GetCurrentExternalMotorPort());
                await _controller.SetHexValueAsync(command.HexCommand);
            }
            else
            {
                _rootPage.NotifyUser("Motor option missing", NotifyType.ErrorMessage);
            }
        }

        private async Task RunCommandsButton_Click()
        {
            await _textCommandsController.RunCommandsAsync(CommandsText.Text);
        }

        private async void SaveCommandsButton_Click()
        {
            await _textCommandsController.SaveCommandsAsync(CommandsText.Text);
        }

        private async void LoadCommandsButton_Click()
        {
            var savedCommands = await _textCommandsController.LoadCommandsAsync();
            if (string.IsNullOrEmpty(savedCommands))
            {
                _rootPage.NotifyUser("Failed to load commands", NotifyType.ErrorMessage);
            }
            else
            {
                CommandsText.Text = savedCommands;
            }
        }

        private async Task<bool> ToggleSubscribedForNotifications(bool isSecondHub = false)
        {
            if ((!isSecondHub && !_subscribedForNotifications) || (isSecondHub && !_subscribedForNotifications2))
            {
                // initialize status
                GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
                var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;

                try
                {
                    AddValueChangedHandler(isSecondHub);
                    // BT_Code: Must write the CCCD in order for server to send indications.
                    // We receive them in the ValueChanged event handler.
                    if (!isSecondHub)
                        status = await _controller.MoveHubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);
                    else
                        status = await _controller2.MoveHubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

                    if (status == GattCommunicationStatus.Success)
                    {
                        _rootPage.NotifyUser("Successfully subscribed for value changes", NotifyType.StatusMessage);
                        return true;
                    }
                    else
                    {
                        RemoveValueChangedHandler();
                        _rootPage.NotifyUser($"Error registering for value changes: {status}", NotifyType.ErrorMessage);
                        return false;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support indicate, but it actually doesn't.
                    _rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
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
                    GattCommunicationStatus result;
                    if (!isSecondHub)
                    {
                        result = await
                            _controller.MoveHubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                GattClientCharacteristicConfigurationDescriptorValue.None);
                    }
                    else
                    {
                        result = await
                            _controller2.MoveHubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                GattClientCharacteristicConfigurationDescriptorValue.None);
                    }

                    if (result == GattCommunicationStatus.Success)
                    {
                        if (!isSecondHub)
                            _subscribedForNotifications = false;
                        else
                            _subscribedForNotifications2 = false;
                        RemoveValueChangedHandler();
                        _rootPage.NotifyUser("Successfully un-registered for notifications", NotifyType.StatusMessage);
                        return true;
                    }
                    else
                    {
                        _rootPage.NotifyUser($"Error un-registering for notifications: {result}", NotifyType.ErrorMessage);
                        return false;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support notify, but it actually doesn't.
                    _rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                    return false;
                }
            }
        }

        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var output = new byte[args.CharacteristicValue.Length];
            var dataReader = DataReader.FromBuffer(args.CharacteristicValue);
            dataReader.ReadBytes(output);
            var notification = DataConverter.ByteArrayToString(output);
            await _notificationManager.ProcessNotification(notification);
            var message = _notificationManager.DecodeNotification(notification);
            _notifications.Add(message);
            if (_notifications.Count > 10)
            {
                _notifications.RemoveAt(0);
            }
            Debug.WriteLine(message);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => CharacteristicLatestValue.Text = string.Join(Environment.NewLine, _notifications));
        }
    }
}
