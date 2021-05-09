﻿using BluetoothController.Models;
using System.Windows;
using System.Windows.Controls;

namespace LegoBluetoothController.UI
{
    public class PortSliderController : IPortController
    {
        private readonly Label _label;
        private readonly Slider _slider;

        public IOType HandledIOType { get; set; }

        public PortSliderController(Label label, Slider slider, IOType iOType)
        {
            _label = label;
            _slider = slider;
            HandledIOType = iOType;
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
