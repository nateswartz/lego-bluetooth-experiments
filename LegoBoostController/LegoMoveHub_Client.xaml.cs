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
            _bluetoothAdapter = new BluetoothLowEnergyAdapter(OnDeviceDiscoveredAsync, OnDeviceConnectedAsync, OnNotificationAsync);
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
                _bluetoothAdapter.StartBleDeviceWatcher();
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

        private async Task OnNotificationAsync(string message)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                _notifications.Add(message);
                if (_notifications.Count > 10)
                {
                    _notifications.RemoveAt(0);
                }
                Debug.WriteLine(message);
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => CharacteristicLatestValue.Text = string.Join(Environment.NewLine, _notifications));
            });
        }

        private async Task DisconnectButton_Click()
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

        private async void ConnectButton_Click()
        {
            _rootPage.NotifyUser("Button not currently connected", NotifyType.ErrorMessage);
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
    }
}
