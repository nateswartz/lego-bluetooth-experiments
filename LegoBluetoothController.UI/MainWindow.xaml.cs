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

        static List<IHubController> _controllers = new List<IHubController>();

        public MainWindow()
        {
            _adapter = new BluetoothLowEnergyAdapter(HandleDiscover, HandleConnect, HandleNotification);
            InitializeComponent();
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
                LogMessage($"{controller.HubType}: {message}");
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

                    LogMessage($"Connected device: {Enum.GetName(typeof(HubType), controller.HubType)}");
                }
                else
                {
                    LogMessage($"Failed to connect: {errorMessage}");
                }
            });
            await Task.CompletedTask;
        }

        private void LogMessage(string message)
        {
            LogMessages.Text += message + Environment.NewLine;
        }

        private async Task ChangeLedColorButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                foreach (var controller in _controllers)
                {
                    var color = LEDColors.All[new Random().Next(0, LEDColors.All.Count)];
                    await controller.ExecuteCommandAsync(new LEDCommand(controller, color));
                    LogMessage($"{controller.HubType}: Changing LED Color to {color.Name}");
                }
            });
        }

        private async Task ShutdownAllButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                foreach (var controller in _controllers)
                {
                    await controller.ExecuteCommandAsync(new ShutdownCommand());
                }
            });
        }
    }
}
