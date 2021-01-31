using LegoBoostController.Commands.Boost;
using LegoBoostController.Controllers;
using LegoBoostController.Models;
using LegoBoostController.Responses;
using System;
using System.Threading.Tasks;

namespace LegoBoostController.EventHandlers
{
    public class ButtonToLEDEventHandler : IEventHandler
    {
        private readonly HubController _controller;

        public Type HandledEvent { get; } = typeof(ButtonStateMessage);

        public ButtonToLEDEventHandler(HubController controller)
        {
            _controller = controller;
        }

        public async Task HandleEventAsync(Response response)
        {
            var data = (ButtonStateMessage)response;
            LEDColor color = LEDColors.Red;
            if (data.State == ButtonState.Pressed)
            {
                color = LEDColors.Yellow;
            }
            else if (data.State == ButtonState.Released)
            {
                color = LEDColors.Purple;
            }
            var command = new LEDBoostCommand(color);
            await _controller.ExecuteCommandAsync(command);
        }
    }
}
