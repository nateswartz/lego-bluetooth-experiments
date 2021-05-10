using BluetoothController.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LegoBluetoothController.UI
{
    public class PortSliderCheckboxController : PortSliderController
    {
        private readonly CheckBox _checkbox;

        public PortSliderCheckboxController(Label label, Slider slider, CheckBox checkbox, IOType handledIOType, Type handledPortState)
            : base(label, slider, handledIOType, handledPortState)
        {
            _checkbox = checkbox;
        }

        public override void Hide()
        {
            base.Hide();
            _checkbox.Visibility = Visibility.Hidden;
            _checkbox.IsChecked = false;
        }

        public override void Show()
        {
            base.Show();
            _checkbox.Visibility = Visibility.Visible;
        }
    }
}
