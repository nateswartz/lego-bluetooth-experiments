using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses.Hub;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class ButtonToLEDEventHandler : EventHandlerBase, IEventHandler<ButtonStateMessage>
    {
        public ButtonToLEDEventHandler(IHubController controller) : base(controller) { }

        public async Task HandleEventAsync(ButtonStateMessage response)
        {
            LEDColor color = LEDColors.Red;
            if (response.State == ButtonState.Pressed)
            {
                color = LEDColors.Yellow;
            }
            else if (response.State == ButtonState.Released)
            {
                color = LEDColors.Purple;
            }
            var command = new LEDCommand(_controller, color);
            await _controller.ExecuteCommandAsync(command);
        }
    }
}
