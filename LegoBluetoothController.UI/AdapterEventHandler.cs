using BluetoothController;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Models.Enums;
using BluetoothController.Responses;
using BluetoothController.Responses.Device.State;
using System;
using System.Collections.Generic;
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
        private readonly List<IPortController> _portControllers;
        private readonly ComboBox _hubSelect;
        private readonly ObservableCollection<IHubController> _controllers;

        public AdapterEventHandler(TextBox logOutputTextBox,
                                   TextBox connectedHubsTextBox,
                                   List<IPortController> portControllers,
                                   ComboBox HubSelect,
                                   ObservableCollection<IHubController> controllers)
        {
            _logOutputTextBox = logOutputTextBox;
            _connectedHubsTextBox = connectedHubsTextBox;
            _portControllers = portControllers;
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
                if (message is PortState portState)
                {
                    RefreshConnectedHubsText();
                    if (_hubSelect.SelectedItem is HubController selectedController &&
                        selectedController == controller)
                    {
                        foreach (var portController in _portControllers)
                        {
                            if (portState.DeviceType == portController.HandledIOType)
                            {
                                if (portState.StateChangeEvent == DeviceState.Attached)
                                {
                                    portController.Show();
                                }
                                if (portState.StateChangeEvent == DeviceState.Detached)
                                {
                                    portController.Hide();
                                }
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
                    foreach (var portController in _portControllers)
                        portController.Hide();
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
