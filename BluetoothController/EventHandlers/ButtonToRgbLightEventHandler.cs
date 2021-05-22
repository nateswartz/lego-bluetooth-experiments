using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Models.Enums;
using BluetoothController.Responses;
using BluetoothController.Responses.Hub;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class ButtonToRgbLightEventHandler : EventHandlerBase, IEventHandler<ButtonStateMessage>
    {
        public ButtonToRgbLightEventHandler(IHubController controller) : base(controller) { }

        public async Task<bool> HandleEventAsync(Response response)
        {
            var data = (ButtonStateMessage)response;
            var color = RgbLightColors.Red;
            if (data.State == ButtonState.Pressed)
            {
                color = RgbLightColors.Yellow;
            }
            else if (data.State == ButtonState.Released)
            {
                color = RgbLightColors.Purple;
            }
            var command = new RgbLightCommand(_controller, color);
            await _controller.ExecuteCommandAsync(command);
            return false;
        }
    }
}
