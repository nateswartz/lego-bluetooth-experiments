using BluetoothController.Models;

namespace LegoBluetoothController.UI
{
    public interface IPortController
    {
        public IOType HandledIOType { get; set; }

        void Hide();
        void Show();
    }
}