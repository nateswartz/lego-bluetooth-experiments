using BluetoothController.Models;
using BluetoothController.Responses.Device.State;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LegoBluetoothController.UI
{
    public class PortSliderController : IPortController
    {
        private readonly Label _label;
        private readonly Slider _slider;

        public IOType HandledIOType { get; private set; }

        public Type HandledPortState { get; private set; }

        public PortSliderController(Label label, Slider slider, IOType iOType, Type handledPortState)
        {
            if (!handledPortState.IsSubclassOf(typeof(PortState)))
            {
                throw new ArgumentException("handledPortState must be a derivative of PortState");
            }
            _label = label;
            _slider = slider;
            HandledIOType = iOType;
            HandledPortState = handledPortState;
        }

        public virtual void Hide()
        {
            _label.Visibility = Visibility.Hidden;
            _slider.Visibility = Visibility.Hidden;
            _slider.Value = 0;
        }

        public virtual void Show()
        {
            _label.Visibility = Visibility.Visible;
            _slider.Visibility = Visibility.Visible;
        }
    }
}
