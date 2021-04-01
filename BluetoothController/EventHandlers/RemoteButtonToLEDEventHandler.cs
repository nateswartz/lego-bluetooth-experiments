using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses.Device.Data;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class RemoteButtonToLEDEventHandler : EventHandlerBase, IEventHandler<RemoteButtonData>
    {
        public RemoteButtonToLEDEventHandler(IHubController controller) : base(controller) { }

        public async Task HandleEventAsync(RemoteButtonData response)
        {
            LEDColor color = LEDColors.Red;
            if (response.PlusPressed)
            {
                color = LEDColors.Yellow;
            }
            else if (response.RedPressed)
            {
                color = LEDColors.Red;
            }
            else if (response.MinusPressed)
            {
                color = LEDColors.Pink;
            }
            if (!response.PlusPressed && !response.MinusPressed && !response.RedPressed)
            {
                color = LEDColors.None;
            }
            var command = new LEDCommand(_controller, color);
            await _controller.ExecuteCommandAsync(command);
        }
    }
}
