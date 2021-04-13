using BluetoothController;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses;
using BluetoothController.Responses.Device.State;
using System;
using System.Collections.Generic;
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
        private readonly TextBox _connectedDevices;
        private readonly List<IHubController> _controllers;

        public AdapterEventHandler(ComboBox hubSelect,
                                   TextBox logOutputTextBox,
                                   TextBox connectedHubsTextBox,
                                   TextBox connectedDevices,
                                   List<IHubController> controllers)
        {
            _hubSelect = hubSelect;
            _logOutputTextBox = logOutputTextBox;
            _connectedHubsTextBox = connectedHubsTextBox;
            _connectedDevices = connectedDevices;
            _controllers = controllers;
        }

        public async Task HandleNotificationAsync(IHubController controller, Response message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogMessage($"{controller.Hub.HubType}: {message}");
                if (message is PortState
                    && _hubSelect.SelectedItem is IHubController selectedController
                    && controller == selectedController)
                {
                    _connectedDevices.Text = GetConnectedDevicesText(controller);
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
                    _connectedHubsTextBox.Text += controller.Hub.HubType;

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
                UpdateConnectedHubsText();
                LogMessage($"Disconnected device: {controller.Hub.HubType}");
            });
            await Task.CompletedTask;
        }

        private void LogMessage(string message)
        {
            _logOutputTextBox.Text += message + Environment.NewLine;
            _logOutputTextBox.ScrollToEnd();
        }

        private void UpdateConnectedHubsText()
        {
            _connectedHubsTextBox.Text = "";
            foreach (var controller in _controllers)
            {
                _connectedHubsTextBox.Text += controller.Hub.HubType;
            }
        }
        private static string GetConnectedDevicesText(IHubController controller)
        {
            var text = "";
            foreach (var port in controller.Hub.Ports.Where(p => !string.IsNullOrWhiteSpace(p.DeviceType.Name)))
            {
                text += $"{port.DeviceType} ({port.PortID}){Environment.NewLine}";
            }
            return text;
        }
    }
}
