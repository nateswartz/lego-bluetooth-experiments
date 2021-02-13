using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses;
using BluetoothController.Responses.Data;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class MotorToLEDEventHandler : IEventHandler
    {
        private readonly HubController _controller;

        public Type HandledEvent { get; } = typeof(SpeedData);

        public MotorToLEDEventHandler(HubController controller)
        {
            _controller = controller;
        }

        public async Task HandleEventAsync(Response response)
        {
            var data = (SpeedData)response;
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
