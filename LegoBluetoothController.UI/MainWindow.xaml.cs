using BluetoothController;
using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LegoBluetoothController.UI
{
    public partial class MainWindow : Window
    {
        private readonly IBluetoothLowEnergyAdapter _adapter;
        private readonly ObservableCollection<IHubController> _controllers = new();

        private readonly IPortController _ledBrightnessControl;
        private readonly IPortController _trainMotorControl;

        private bool _forceClose = false;

        public MainWindow()
        {
            InitializeComponent();
            _ledBrightnessControl = new PortSliderController(LEDBrightnessLabel, LEDBrightnessSlider);
            _trainMotorControl = new PortSliderCheckboxController(TrainMotorLabel, TrainMotorSlider, TrainMotorClockwiseCheckbox);

            var eventHandler = new AdapterEventHandler(LogMessages, ConnectedHubs, _ledBrightnessControl,
                                                       _trainMotorControl,
                                                       HubSelect, _controllers);
            _adapter = new BluetoothLowEnergyAdapter(eventHandler);
            HubSelect.ItemsSource = _controllers;
            ColorSelect.ItemsSource = LEDColors.All;

            _ledBrightnessControl.Hide();
            _trainMotorControl.Hide();
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

        private async void ColorSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HubSelect.SelectedItem is not IHubController controller)
            {
                ColorSelect.SelectedItem = null;
                return;
            }
            if (ColorSelect.SelectedItem is not LEDColor color)
            {
                ColorSelect.SelectedItem = null;
                return;
            }
            await controller.ExecuteCommandAsync(new LEDCommand(controller, color));
            LogMessage($"{controller.Hub.HubType}: Changing LED Color to {color.Name}");
        }

        private async void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            if (HubSelect.SelectedItem is not IHubController controller)
                return;
            await controller.ExecuteCommandAsync(new ShutdownCommand());
        }

        private async void ExecuteCommandButton_Click(object sender, RoutedEventArgs e)
        {
            if (HubSelect.SelectedItem is not IHubController controller)
                return;
            await controller.ExecuteCommandAsync(new RawCommand(RawCommandText.Text));
        }

        private void LEDBrightnessSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (HubSelect.SelectedItem is not IHubController controller)
                return;
            var externalLED = controller.GetPortIdsByDeviceType(IOTypes.ExternalLED).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(externalLED))
                return;
            controller.ExecuteCommandAsync(new ExternalLEDCommand(externalLED, Convert.ToInt32(LEDBrightnessSlider.Value)));
        }

        private void TrainMotorSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SendTrainMotorCommand();
        }

        private void TrainMotorClockwiseCheckbox_Click(object sender, RoutedEventArgs e)
        {
            SendTrainMotorCommand();
        }

        private void SendTrainMotorCommand()
        {
            if (HubSelect.SelectedItem is not IHubController controller)
                return;
            var trainMotor = controller.GetPortIdsByDeviceType(IOTypes.TrainMotor).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(trainMotor))
                return;
            controller.ExecuteCommandAsync(new TrainMotorCommand(trainMotor, Convert.ToInt32(TrainMotorSlider.Value), TrainMotorClockwiseCheckbox.IsChecked.Value));
        }

        private void HubSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HubSelect.SelectedItem is not IHubController controller)
                return;

            if (!string.IsNullOrWhiteSpace(controller.GetPortIdsByDeviceType(IOTypes.ExternalLED).FirstOrDefault()))
            {
                _ledBrightnessControl.Show();
            }
            else
            {
                _ledBrightnessControl.Hide();
            }

            if (!string.IsNullOrWhiteSpace(controller.GetPortIdsByDeviceType(IOTypes.TrainMotor).FirstOrDefault()))
            {
                _trainMotorControl.Show();
            }
            else
            {
                _trainMotorControl.Hide();
            }
        }

        private void LogMessage(string message)
        {
            LogMessages.Text += message + Environment.NewLine;
            LogMessages.ScrollToEnd();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            if (_forceClose)
            {
                e.Cancel = false;
                return;
            }

            Dispatcher.InvokeAsync(ShutdownApplication, DispatcherPriority.Normal);
        }

        private async void ShutdownApplication()
        {
            foreach (var controller in _controllers)
            {
                await controller.ExecuteCommandAsync(new ShutdownCommand());
            }

            CloseForced();
        }

        private void CloseForced()
        {
            _forceClose = true;
            Close();
        }
    }
}
