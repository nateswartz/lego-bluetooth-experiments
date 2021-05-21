﻿using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses;
using BluetoothController.Responses.Device.Data;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class MotorToLEDEventHandler : EventHandlerBase, IEventHandler<BoostMotorData>
    {
        public MotorToLEDEventHandler(IHubController controller) : base(controller) { }

        public async Task<bool> HandleEventAsync(Response response)
        {
            var data = (BoostMotorData)response;
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
            return false;
        }
    }
}
