﻿using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses;
using BluetoothController.Responses.Device.Data;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class MotorToLEDEventHandler : IEventHandler
    {
        private readonly HubController _controller;

        public Type HandledEvent { get; } = typeof(ExternalMotorData);

        public MotorToLEDEventHandler(HubController controller)
        {
            _controller = controller;
        }

        public async Task HandleEventAsync(Response response)
        {
            var data = (ExternalMotorData)response;
            var color = LEDColors.Red;
            if (data.Speed > 30)
            {
                color = LEDColors.Green;
            }
            else if (data.Speed > 1)
            {
                color = LEDColors.Purple;
            }
            var command = new LEDCommand(_controller, color);
            await _controller.ExecuteCommandAsync(command);
        }
    }
}
