using BluetoothController.Models;

namespace LegoBluetoothController.UI
{
    public interface IPortController
    {
        public IOType HandledIOType { get; }
        void Hide();
        void Show();
    }
}