using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses;
using BluetoothController.Responses.Hub;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class ButtonToLEDEventHandler : EventHandlerBase, IEventHandler
    {
        public Type HandledEvent { get; } = typeof(ButtonStateMessage);

        public ButtonToLEDEventHandler(IHubController controller) : base(controller) { }

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
            var command = new LEDCommand(_controller, color);
            await _controller.ExecuteCommandAsync(command);
        }
    }
}
