using BluetoothController;
using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LegoBluetoothController.UI
{
    public partial class MainWindow : Window
    {

        static IBluetoothLowEnergyAdapter _adapter;

        static readonly List<IHubController> _controllers = new();

        public MainWindow()
        {
            InitializeComponent();
            var eventHandler = new AdapterEventHandler(HubSelect, LogMessages, ConnectedHubs, ConnectedDevices, _controllers);
            _adapter = new BluetoothLowEnergyAdapter(eventHandler);
            HubSelect.ItemsSource = _controllers;
            ColorSelect.ItemsSource = LEDColors.All;
        }

        private void DiscoverButton_Click(object sender, RoutedEventArgs e)
        {
            if (DiscoverButton.Content.ToString() == "Discover")
            {
                LogMessage("Searching for devices...");
                _adapter.StartBleDeviceWatcher();
                DiscoverButton.Content = "Stop Discovery";
            }
            else if (DiscoverButton.Content.ToString() == "Stop Discovery")
            {
                LogMessage("Ending search for devices...");
                _adapter.StopBleDeviceWatcher();
                DiscoverButton.Content = "Discover";
            }
        }

        private async void ChangeLedColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (HubSelect.SelectedItem is not IHubController controller)
                return;
            if (ColorSelect.SelectedItem is not LEDColor color)
                return;
            await controller.ExecuteCommandAsync(new LEDCommand(controller, color));
            LogMessage($"{controller.Hub.HubType}: Changing LED Color to {color.Name}");
        }

        private async void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            if (HubSelect.SelectedItem is not IHubController controller)
                return;
            await controller.ExecuteCommandAsync(new ShutdownCommand());

            UpdateConnectedHubsText();
        }

        private void ExecuteCommandButton_Click(object sender, RoutedEventArgs e)
        {
            if (HubSelect.SelectedItem is not IHubController controller)
                return;
            controller.ExecuteCommandAsync(new RawCommand(RawCommandText.Text));
        }

        private void HubSelect_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (HubSelect.SelectedItem is not IHubController controller)
                return;
            ConnectedDevices.Text = GetConnectedDevicesText(controller);
        }

        private void UpdateConnectedHubsText()
        {
            ConnectedHubs.Text = "";
            foreach (var controller in _controllers)
            {
                ConnectedHubs.Text += controller.Hub.HubType;
            }
        }

        private void LogMessage(string message)
        {
            LogMessages.Text += message + Environment.NewLine;
            LogMessages.ScrollToEnd();
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
