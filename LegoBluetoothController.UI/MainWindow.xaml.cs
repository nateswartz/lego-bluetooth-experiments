using BluetoothController;
using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace LegoBluetoothController.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        static IBluetoothLowEnergyAdapter _adapter;

        static readonly List<IHubController> _controllers = new();

        public MainWindow()
        {
            InitializeComponent();
            _adapter = new BluetoothLowEnergyAdapter(HandleDiscover, HandleConnect, HandleNotification, HandleDisconnect);
            HubSelect.ItemsSource = _controllers;
        }

        private void DiscoverButton_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Searching for devices...");
            _adapter.StartBleDeviceWatcher();
        }

        private async Task HandleNotification(IHubController controller, string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogMessage($"{controller.Hub.HubType}: {message}");
            });
            await Task.CompletedTask;
        }

        private async Task HandleDiscover(DiscoveredDevice device)
        {
            Dispatcher.Invoke(() =>
            {
                LogMessage($"Discovered device: {device.Name}");
            });
            await Task.CompletedTask;
        }

        private async Task HandleConnect(IHubController controller, string errorMessage)
        {
            Dispatcher.Invoke(() =>
            {
                if (controller != null)
                {
                    _controllers.Add(controller);
                    ConnectedHubs.Text += controller.Hub.HubType;

                    LogMessage($"Connected device: {controller.Hub.HubType}");
                }
                else
                {
                    LogMessage($"Failed to connect: {errorMessage}");
                }
            });
            await Task.CompletedTask;
        }

        private async Task HandleDisconnect(IHubController controller)
        {
            Dispatcher.Invoke(() =>
            {
                _controllers.Remove(controller);
                ConnectedHubs.Text = "";
                foreach (var existingController in _controllers)
                {
                    ConnectedHubs.Text += existingController.Hub.HubType;
                }

                LogMessage($"Disconnected device: {controller.Hub.HubType}");
            });
            await Task.CompletedTask;
        }

        private void LogMessage(string message)
        {
            LogMessages.Text += message + Environment.NewLine;
            LogMessages.ScrollToEnd();
        }

        private async void ChangeLedColorButton_Click(object sender, RoutedEventArgs e)
        {
            var controller = HubSelect.SelectedItem as IHubController;
            var color = LEDColors.All[new Random().Next(0, LEDColors.All.Count)];
            await controller.ExecuteCommandAsync(new LEDCommand(controller, color));
            LogMessage($"{controller.Hub.HubType}: Changing LED Color to {color.Name}");
        }

        private async void ShutdownAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var controller in _controllers)
            {
                await controller.ExecuteCommandAsync(new ShutdownCommand());
            }
            ConnectedHubs.Text = "";
        }

        private void ExecuteCommandButton_Click(object sender, RoutedEventArgs e)
        {
            var controller = HubSelect.SelectedItem as IHubController;
            controller.ExecuteCommandAsync(new RawCommand(RawCommandText.Text));
        }
    }
}
