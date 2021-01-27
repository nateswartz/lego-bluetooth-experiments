using LegoBoostController.Commands.Boost;
using LegoBoostController.Controllers;
using LegoBoostController.Models;
using LegoBoostController.Responses;
using System;
using System.Threading.Tasks;

namespace LegoBoostController.EventHandlers
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
            var command = new LEDBoostCommand(color);
            await _controller.SetHexValueAsync(command.HexCommand);
        }
    }
}
