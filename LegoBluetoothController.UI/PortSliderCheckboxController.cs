using BluetoothController.Models;
using System.Windows;
using System.Windows.Controls;

namespace LegoBluetoothController.UI
{
    public class PortSliderCheckboxController : PortSliderController
    {
        private readonly CheckBox _checkbox;

        public PortSliderCheckboxController(Label label, Slider slider, CheckBox checkbox, IoDeviceType handledDeviceType)
            : base(label, slider, handledDeviceType)
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
