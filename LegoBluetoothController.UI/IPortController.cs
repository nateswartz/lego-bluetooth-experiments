using BluetoothController.Models;
using System;

namespace LegoBluetoothController.UI
{
    public interface IPortController
    {
        public IOType HandledIOType { get; }
        public Type HandledPortState { get; }
        void Hide();
        void Show();
    }
}