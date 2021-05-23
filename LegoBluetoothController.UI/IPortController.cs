using BluetoothController.Models;

namespace LegoBluetoothController.UI
{
    public interface IPortController
    {
        public IoDeviceType HandledDeviceType { get; }
        void Hide();
        void Show();
    }
}