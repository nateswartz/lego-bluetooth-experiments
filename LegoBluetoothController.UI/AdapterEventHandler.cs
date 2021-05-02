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
        private readonly ObservableCollection<IHubController> _controllers;

        public AdapterEventHandler(TextBox logOutputTextBox,
                                   TextBox connectedHubsTextBox,
                                   Label LEDBrightnessLabel,
                                   Slider LEDBrightnessSlider,
                                   ObservableCollection<IHubController> controllers)
        {
            _logOutputTextBox = logOutputTextBox;
            _connectedHubsTextBox = connectedHubsTextBox;
            _ledBrightnessLabel = LEDBrightnessLabel;
            _ledBrightnessSlider = LEDBrightnessSlider;
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
                    if (message is ExternalLEDState state)
                    {
                        if (state.StateChangeEvent == DeviceState.Attached)
                        {
                            _ledBrightnessLabel.Visibility = Visibility.Visible;
                            _ledBrightnessSlider.Visibility = Visibility.Visible;
                        }
                        if (state.StateChangeEvent == DeviceState.Detached)
                        {
                            _ledBrightnessLabel.Visibility = Visibility.Hidden;
                            _ledBrightnessSlider.Visibility = Visibility.Hidden;
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
                _controllers.Remove(controller);
                RefreshConnectedHubsText();
                _ledBrightnessLabel.Visibility = Visibility.Hidden;
                _ledBrightnessSlider.Visibility = Visibility.Hidden;
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
