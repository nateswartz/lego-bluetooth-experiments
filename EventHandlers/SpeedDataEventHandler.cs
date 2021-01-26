using LegoBoostController.Commands.Boost;
using LegoBoostController.Controllers;
using LegoBoostController.Models;
using LegoBoostController.Responses;
using System;
using System.Threading.Tasks;

namespace LegoBoostController.EventHandlers
{
    public class SpeedDataEventHandler : IEventHandler
    {
        private readonly BoostController _controller;

        public Type HandledEvent { get; } = typeof(SpeedData);

        public SpeedDataEventHandler(BoostController controller)
        {
            _controller = controller;
        }

        public async Task HandleEventAsync(Response response)
        {
            if (response.GetType() == typeof(SpeedData))
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
}
