using System.Windows;
using System.Windows.Controls;

namespace LegoBluetoothController.UI
{
    public class PortSliderController : IPortController
    {
        private readonly Label _label;
        private readonly Slider _slider;

        public PortSliderController(Label label, Slider slider)
        {
            _label = label;
            _slider = slider;
        }

        public void Hide()
        {
            _label.Visibility = Visibility.Hidden;
            _slider.Visibility = Visibility.Hidden;
            _slider.Value = 0;
        }

        public void Show()
        {
            _label.Visibility = Visibility.Visible;
            _slider.Visibility = Visibility.Visible;
        }
    }
}
