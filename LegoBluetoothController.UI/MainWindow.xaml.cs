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
            var ledBrightnessControl = new PortSliderController(LEDBrightnessLabel, LEDBrightnessSlider, IoDeviceTypes.LedLight);
            var trainMotorControl = new PortSliderCheckboxController(TrainMotorLabel, TrainMotorSlider, TrainMotorClockwiseCheckbox, IoDeviceTypes.TrainMotor);
            var boostMotorControl = new PortSliderCheckboxController(BoostMotorLabel, BoostMotorSlider, BoostMotorClockwiseCheckbox, IoDeviceTypes.BoostTachoMotor);
            var technicMotorControl = new PortSliderCheckboxController(TechnicMotorLabel, TechnicMotorSlider, TechnicMotorClockwiseCheckbox, IoDeviceTypes.SmallAngularMotor);
            var rgbLightControl = new PortComboBoxController(RgbLightColorLabel, RgbLightColorSelect, IoDeviceTypes.RgbLight);

            _portControllers.Add(ledBrightnessControl);
            _portControllers.Add(trainMotorControl);
            _portControllers.Add(boostMotorControl);
            _portControllers.Add(rgbLightControl);
            _portControllers.Add(technicMotorControl);

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

        private void LEDBrightnessSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (HubSelect.SelectedItem is not IHubController controller)
                return;
            var externalLED = controller.GetPortIdsByDeviceType(IoDeviceTypes.LedLight).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(externalLED))
                return;
            controller.ExecuteCommandAsync(new ExternalLedCommand(externalLED, Convert.ToInt32(LEDBrightnessSlider.Value)));
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
            var trainMotor = controller.GetPortIdsByDeviceType(IoDeviceTypes.TrainMotor).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(trainMotor))
                return;
            controller.ExecuteCommandAsync(new TrainMotorCommand(trainMotor, Convert.ToInt32(TrainMotorSlider.Value), TrainMotorClockwiseCheckbox.IsChecked.Value));
        }

        private void BoostMotorClockwiseCheckbox_Click(object sender, RoutedEventArgs e)
        {
            SendBoostMotorCommand();
        }

        private void BoostMotorSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SendBoostMotorCommand();
        }

        private void SendBoostMotorCommand()
        {
            SendMotorCommand(IoDeviceTypes.BoostTachoMotor, Convert.ToInt32(BoostMotorSlider.Value), BoostMotorClockwiseCheckbox.IsChecked.Value);
        }

        private void TechnicMotorClockwiseCheckbox_Click(object sender, RoutedEventArgs e)
        {
            SendSmallAngularMotorCommand();
        }

        private void TechnicMotorSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SendSmallAngularMotorCommand();
        }

        private void SendSmallAngularMotorCommand()
        {
            SendMotorCommand(IoDeviceTypes.SmallAngularMotor, Convert.ToInt32(TechnicMotorSlider.Value), TechnicMotorClockwiseCheckbox.IsChecked.Value);
        }

        private void SendMotorCommand(IoDeviceType motorType, int power, bool clockwise)
        {
            if (HubSelect.SelectedItem is not IHubController controller)
                return;
            var motor = controller.GetPortIdsByDeviceType(motorType).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(motor))
                return;
            controller.ExecuteCommandAsync(new MotorCommand(motor, power, 10000, clockwise));
        }

        private void HubSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HubSelect.SelectedItem is not IHubController controller)
                return;

            foreach (var portController in _portControllers)
            {
                if (!string.IsNullOrWhiteSpace(controller.GetPortIdsByDeviceType(portController.HandledDeviceType).FirstOrDefault()))
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
