using BluetoothController;
using BluetoothController.Controllers;
using BluetoothController.Models;
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
        private readonly ComboBox _hubSelect;
        private readonly TextBox _logOutputTextBox;
        private readonly TextBox _connectedHubsTextBox;
        private readonly ObservableCollection<IHubController> _controllers;

        public AdapterEventHandler(ComboBox hubSelect,
                                   TextBox logOutputTextBox,
                                   TextBox connectedHubsTextBox,
                                   ObservableCollection<IHubController> controllers)
        {
            _hubSelect = hubSelect;
            _logOutputTextBox = logOutputTextBox;
            _connectedHubsTextBox = connectedHubsTextBox;
            _controllers = controllers;
        }

        public async Task HandleNotificationAsync(IHubController controller, Response message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogMessage($"{controller.Hub.HubType}: {message}");
                if (message is PortState)
                {
                    RefreshConnectedHubsText();
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
