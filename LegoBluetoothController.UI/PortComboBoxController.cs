using BluetoothController.Models;
using System.Windows;
using System.Windows.Controls;

namespace LegoBluetoothController.UI
{
    public class PortComboBoxController : IPortController
    {
        private readonly Label _label;
        private readonly ComboBox _comboBox;

        public IOType HandledIOType { get; private set; }

        public PortComboBoxController(Label label, ComboBox comboBox, IOType iOType)
        {
            _label = label;
            _comboBox = comboBox;
            HandledIOType = iOType;
        }

        public virtual void Hide()
        {
            _label.Visibility = Visibility.Hidden;
            _comboBox.Visibility = Visibility.Hidden;
            _comboBox.SelectedItem = -1;
        }

        public virtual void Show()
        {
            _label.Visibility = Visibility.Visible;
            _comboBox.Visibility = Visibility.Visible;
        }
    }
}
