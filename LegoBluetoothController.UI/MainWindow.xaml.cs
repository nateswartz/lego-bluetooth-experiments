using BluetoothController;
using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using System;
using System.Collections.Generic;
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


        private readonly List<IPortController> _portControllers = new();

        private bool _forceClose = false;

        public MainWindow()
        {
            InitializeComponent();
            var ledBrightnessControl = new PortSliderController(LEDBrightnessLabel, LEDBrightnessSlider, IOTypes.LedLight);
            var trainMotorControl = new PortSliderCheckboxController(TrainMotorLabel, TrainMotorSlider, TrainMotorClockwiseCheckbox, IOTypes.TrainMotor);
            var boostMotorControl = new PortSliderCheckboxController(BoostMotorLabel, BoostMotorSlider, BoostMotorClockwiseCheckbox, IOTypes.BoostTachoMotor);
            var rgbLightControl = new PortComboBoxController(RgbLightColorLabel, RgbLightColorSelect, IOTypes.RgbLight);

            _portControllers.Add(ledBrightnessControl);
            _portControllers.Add(trainMotorControl);
            _portControllers.Add(boostMotorControl);
            _portControllers.Add(rgbLightControl);

            var eventHandler = new AdapterEventHandler(LogMessages, ConnectedHubs, _portControllers,
                                                       HubSelect, _controllers);
            _adapter = new BluetoothLowEnergyAdapter(eventHandler);
            HubSelect.ItemsSource = _controllers;
            RgbLightColorSelect.ItemsSource = RgbLightColors.All;

            foreach (var portController in _portControllers)
                portController.Hide();
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

        private async void RgbLightColorSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HubSelect.SelectedItem is not IHubController controller)
            {
                RgbLightColorSelect.SelectedItem = null;
                return;
            }
            if (RgbLightColorSelect.SelectedItem is not RgbLightColor color)
            {
                RgbLightColorSelect.SelectedItem = null;
                return;
            }
            await controller.ExecuteCommandAsync(new RgbLightCommand(controller, color));
            LogMessage($"{controller.Hub.HubType}: Changing RGB Color to {color.Name}");
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
            var externalLED = controller.GetPortIdsByDeviceType(IOTypes.LedLight).FirstOrDefault();
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

        private void BoostMotorClockwiseCheckbox_Click(object sender, RoutedEventArgs e)
        {
            SendMotorCommand();
        }

        private void BoostMotorSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SendMotorCommand();
        }

        private void SendMotorCommand()
        {
            if (HubSelect.SelectedItem is not IHubController controller)
                return;
            var boostMotor = controller.GetPortIdsByDeviceType(IOTypes.BoostTachoMotor).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(boostMotor))
                return;
            controller.ExecuteCommandAsync(new MotorCommand(boostMotor, Convert.ToInt32(BoostMotorSlider.Value), 10000, BoostMotorClockwiseCheckbox.IsChecked.Value));
        }

        private void HubSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HubSelect.SelectedItem is not IHubController controller)
                return;

            foreach (var portController in _portControllers)
            {
                if (!string.IsNullOrWhiteSpace(controller.GetPortIdsByDeviceType(portController.HandledIOType).FirstOrDefault()))
                {
                    portController.Show();
                }
                else
                {
                    portController.Hide();
                }
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
