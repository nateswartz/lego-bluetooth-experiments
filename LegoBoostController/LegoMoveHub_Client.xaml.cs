using BluetoothController;
using BluetoothController.Commands.Boost;
using BluetoothController.Controllers;
using BluetoothController.EventHandlers;
using BluetoothController.Models;
using BluetoothController.Responses;
using BluetoothController.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LegoBoostController
{
    public sealed partial class LegoMoveHub_Client : Page
    {
        private MainPage _rootPage = MainPage.Current;

        private BluetoothLowEnergyAdapter _bluetoothAdapter;

        private List<string> _notifications = new List<string>();

        private HubController _controller;
        private NotificationManager _notificationManager;

        private HubController _controller2;
        private NotificationManager _notificationManager2;

        private TextCommandsController _textCommandsController;

        List<LEDColor> _colors = LEDColors.All;
        List<Motor> _motors = Motors.All;
        List<Robot> _robots = Enum.GetValues(typeof(Robot)).Cast<Robot>().ToList();
        ObservableCollection<HubController> _hubs = new ObservableCollection<HubController>();

        #region UI Code
        public LegoMoveHub_Client()
        {
            _bluetoothAdapter = new BluetoothLowEnergyAdapter();
            // TODO: This currently only supports the Move Hub (controller1)
            _textCommandsController = new TextCommandsController(_controller);
            InitializeComponent();
            SampleCommands.Text = "Sample Commands:";
        }

        private async Task LEDColorsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null) return;
            var color = (LEDColor)((ComboBox)sender).SelectedItem;
            _rootPage.NotifyUser($"The selected item is {color}", NotifyType.StatusMessage);

            var selectedHub = HubSelectCombo.SelectedItem as HubController;

            await selectedHub.ExecuteCommandAsync(new LEDBoostCommand(color));
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
            if (string.IsNullOrEmpty(_controller?.SelectedBleDeviceId))
            {
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = false;
                ToggleButtons(false);
            }
        }

        private void ScanButton_Click()
        {
            if (!_bluetoothAdapter.Scanning)
            {
                _bluetoothAdapter.StartBleDeviceWatcher(OnDeviceDiscoveredAsync, OnDeviceConnectedAsync);
                ScanButton.Content = "Stop scanning";
                _rootPage.NotifyUser($"Device watcher started.", NotifyType.StatusMessage);
            }
            else
            {
                _bluetoothAdapter.StopBleDeviceWatcher();
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

        private async Task OnDeviceDiscoveredAsync(DiscoveredDevice device)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                ConnectButton.IsEnabled = true;
                Debug.WriteLine(String.Format($"Found {device.Name}: {device.BluetoothDeviceId}"));
                _rootPage.NotifyUser($"Found {device.Name}.", NotifyType.StatusMessage);
                await Task.CompletedTask;
            });
        }

        private async Task<bool> DisableNotifications()
        {
            foreach (var controller in _hubs)
            {
                if (controller.SubscribedForNotifications)
                {
                    // Need to clear the CCCD from the remote device so we stop receiving notifications
                    var result = await controller.HubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                    if (result != GattCommunicationStatus.Success)
                    {
                        return false;
                    }
                    else
                    {
                        controller.HubCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                        controller.SubscribedForNotifications = false;
                    }
                }
            }
            return true;
        }

        private async Task DisconnectButton_Click()
        {
            if (await DisableNotifications())
            {
                ConnectButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;
                ToggleButtons(false);
                if (_controller?.IsConnected == true)
                {
                    await _controller.ExecuteCommandAsync(new DisconnectCommand());
                    _controller.Disconnect();
                }
                if (_controller2?.IsConnected == true)
                {
                    await _controller2.ExecuteCommandAsync(new DisconnectCommand());
                    _controller2.Disconnect();
                }
            }
        }

        private async void ConnectButton_Click()
        {
            //await Connect(_controller, _notificationManager);
            await Task.CompletedTask;
        }

        private async Task OnDeviceConnectedAsync(HubController controller, NotificationManager notificationManager, string errorMessage)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (controller != null)
                {
                    ToggleButtons(true);
                    DisconnectButton.IsEnabled = true;
                    ConnectButton.IsEnabled = false;
                    _hubs.Add(controller);
                    if (controller.HubType == HubType.BoostMoveHub)
                    {
                        ToggleControls.IsEnabled = true;
                        _controller = controller;
                        _notificationManager = notificationManager;
                    }
                    else
                    {
                        _controller2 = controller;
                        _notificationManager2 = notificationManager;
                    }
                    EnableCharacteristicPanels();
                }
                else if (!string.IsNullOrEmpty(errorMessage))
                {
                    _rootPage.NotifyUser(errorMessage, NotifyType.ErrorMessage);
                }
                await Task.CompletedTask;
            });
        }

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

        private void EnableCharacteristicPanels()
        {
            CharacteristicWritePanel.Visibility = Visibility.Visible;
        }

        private async void WriteHexButton_Click()
        {
            var selectedHub = HubSelectCombo.SelectedItem as HubController;
            var text = CharacteristicWriteValue.Text;
            if (!string.IsNullOrEmpty(text))
            {
                await selectedHub.SetHexValueAsync(text);
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
            await _controller.ExecuteCommandAsync(new ButtonNotificationsCommand(notificationsEnabled));
        }

        private async void ToggleColorDistanceNotificationsButton_Click()
        {
            await ToggleNotification(ToggleColorDistanceNotificationsButton, "Color/Distance", _controller.PortState.CurrentColorDistanceSensorPort, "08");
        }

        private async void ToggleExternalMotorNotificationsButton_Click()
        {
            // 01 - Speed; 02 - Angle
            var notificationType = ExternalMotorNotificationTypeToggle.IsOn ? "02" : "01";
            await ToggleNotification(ToggleExternalMotorNotificationsButton, "External Motor", _controller.PortState.CurrentExternalMotorPort, notificationType);
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
            return await _controller.ExecuteCommandAsync(new ToggleNotificationsCommand(notificationsEnabled, port, sensorMode));
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
            await ToggleNotification(ToggleExternalMotorNotificationsButton, "External Motor", _controller.PortState.CurrentExternalMotorPort, "01");

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
                SyncLEDButtonButton.Content = "Un-sync LED with Button";
                _notificationManager.AddEventHandler(new ButtonToLEDEventHandler(_controller2));

            }
            else
            {
                SyncLEDButtonButton.Content = "Sync LED with Button";
                _notificationManager.RemoveEventHandler(new ButtonToLEDEventHandler(_controller2));
            }
        }

        private async void RunMotorButton_Click()
        {
            var hasRunTime = int.TryParse(RunTimeText.Text, out int runTime);
            var clockwise = DirectionToggle.IsOn;
            if (MotorsCombo.SelectedItem != null && hasRunTime)
            {
                var command = new MotorBoostCommand((Motor)MotorsCombo.SelectedItem, (int)MotorPowerSlider.Value, runTime, clockwise, _controller.GetCurrentExternalMotorPort());
                await _controller.ExecuteCommandAsync(command);
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
                        _rootPage.NotifyUser("Successfully subscribed for value changes", NotifyType.StatusMessage);
                        return true;
                    }
                    else
                    {
                        RemoveValueChangedHandler(controller);
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
                    var result = await
                        controller.HubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.None);

                    if (result == GattCommunicationStatus.Success)
                    {
                        controller.SubscribedForNotifications = false;

                        RemoveValueChangedHandler(controller);
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
                    // This usually happens when a device reports that it supports notify, but it actually doesn't.
                    _rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                    return false;
                }
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
            Debug.WriteLine(message);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => CharacteristicLatestValue.Text = string.Join(Environment.NewLine, _notifications));
        }
    }
}
