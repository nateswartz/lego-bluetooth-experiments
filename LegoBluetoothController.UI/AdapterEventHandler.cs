using BluetoothController;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Models.Enums;
using BluetoothController.Responses;
using BluetoothController.Responses.Device.State;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LegoBluetoothController.UI
{
    internal class AdapterEventHandler : IBluetoothLowEnergyAdapterEventHandler
    {
        private readonly TextBox _logOutputTextBox;
        private readonly TextBox _connectedHubsTextBox;
        private readonly Label _ledBrightnessLabel;
        private readonly Slider _ledBrightnessSlider;
        private readonly Label _trainMotorLabel;
        private readonly Slider _trainMotorSlider;
        private readonly ComboBox _hubSelect;
        private readonly ObservableCollection<IHubController> _controllers;

        public AdapterEventHandler(TextBox logOutputTextBox,
                                   TextBox connectedHubsTextBox,
                                   Label LEDBrightnessLabel,
                                   Slider LEDBrightnessSlider,
                                   Label TrainMotorLabel,
                                   Slider TrainMotorSlider,
                                   ComboBox HubSelect,
                                   ObservableCollection<IHubController> controllers)
        {
            _logOutputTextBox = logOutputTextBox;
            _connectedHubsTextBox = connectedHubsTextBox;
            _ledBrightnessLabel = LEDBrightnessLabel;
            _ledBrightnessSlider = LEDBrightnessSlider;
            _trainMotorLabel = TrainMotorLabel;
            _trainMotorSlider = TrainMotorSlider;
            _hubSelect = HubSelect;
            _controllers = controllers;
        }

        public async Task HandleNotificationAsync(IHubController controller, Response message)
        {
            if (Application.Current == null)
                return;
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogMessage($"{controller.Hub.HubType}: {message}");
                if (message is PortState)
                {
                    RefreshConnectedHubsText();
                    if (_hubSelect.SelectedItem is HubController selectedController &&
                        selectedController == controller)
                    {
                        if (message is ExternalLEDState ledState)
                        {
                            if (ledState.StateChangeEvent == DeviceState.Attached)
                            {
                                _ledBrightnessLabel.Visibility = Visibility.Visible;
                                _ledBrightnessSlider.Visibility = Visibility.Visible;
                            }
                            if (ledState.StateChangeEvent == DeviceState.Detached)
                            {
                                _ledBrightnessLabel.Visibility = Visibility.Hidden;
                                _ledBrightnessSlider.Visibility = Visibility.Hidden;
                                _ledBrightnessSlider.Value = 0;
                            }
                        }
                        if (message is TrainMotorState trainState)
                        {
                            if (trainState.StateChangeEvent == DeviceState.Attached)
                            {
                                _trainMotorLabel.Visibility = Visibility.Visible;
                                _trainMotorSlider.Visibility = Visibility.Visible;
                            }
                            if (trainState.StateChangeEvent == DeviceState.Detached)
                            {
                                _trainMotorLabel.Visibility = Visibility.Hidden;
                                _trainMotorSlider.Visibility = Visibility.Hidden;
                                _trainMotorSlider.Value = 0;
                            }
                        }
                    }
                }
            });
            await Task.CompletedTask;
        }

        public async Task HandleDiscoveryAsync(DiscoveredDevice device)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogMessage($"Discovered device: {device.Name}");
            });
            await Task.CompletedTask;
        }

        public async Task HandleConnectAsync(IHubController controller, string errorMessage)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (controller != null)
                {
                    _controllers.Add(controller);
                    RefreshConnectedHubsText();
                    LogMessage($"Connected device: {controller.Hub.HubType}");
                }
                else
                {
                    LogMessage($"Failed to connect: {errorMessage}");
                }
            });
            await Task.CompletedTask;
        }

        public async Task HandleDisconnectAsync(IHubController controller)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_hubSelect.SelectedItem is HubController selectedController &&
                    selectedController == controller)
                {
                    _ledBrightnessLabel.Visibility = Visibility.Hidden;
                    _ledBrightnessSlider.Visibility = Visibility.Hidden;
                    _ledBrightnessSlider.Value = 0;
                    _trainMotorLabel.Visibility = Visibility.Hidden;
                    _trainMotorSlider.Visibility = Visibility.Hidden;
                    _trainMotorSlider.Value = 0;
                }
                _controllers.Remove(controller);
                RefreshConnectedHubsText();
                LogMessage($"Disconnected device: {controller.Hub.HubType}");
            });
            await Task.CompletedTask;
        }

        private void LogMessage(string message)
        {
            _logOutputTextBox.Text += message + Environment.NewLine;
            _logOutputTextBox.ScrollToEnd();
        }

        private void RefreshConnectedHubsText()
        {
            var text = "";
            foreach (var controller in _controllers)
            {
                text += $"{controller.Hub.HubType} ({controller.SelectedBleDeviceId}){Environment.NewLine}";
                foreach (var port in controller.Hub.Ports.Where(p => !string.IsNullOrWhiteSpace(p.DeviceType.Name)))
                {
                    text += $"\t{port.DeviceType} ({port.PortID}){Environment.NewLine}";
                }
            }
            _connectedHubsTextBox.Text = text;
        }
    }
}
