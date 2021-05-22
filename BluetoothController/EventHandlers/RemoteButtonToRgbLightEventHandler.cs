using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses;
using BluetoothController.Responses.Device.Data;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class RemoteButtonToRgbLightEventHandler : EventHandlerBase, IEventHandler<RemoteButtonData>
    {
        public RemoteButtonToRgbLightEventHandler(IHubController controller) : base(controller) { }

        public async Task<bool> HandleEventAsync(Response response)
        {
            var data = (RemoteButtonData)response;
            var color = RgbLightColors.Red;
            if (data.PlusPressed)
            {
                color = RgbLightColors.Yellow;
            }
            else if (data.RedPressed)
            {
                color = RgbLightColors.Red;
            }
            else if (data.MinusPressed)
            {
                color = RgbLightColors.Pink;
            }
            if (!data.PlusPressed && !data.MinusPressed && !data.RedPressed)
            {
                color = RgbLightColors.None;
            }
            var command = new RgbLightCommand(_controller, color);
            await _controller.ExecuteCommandAsync(command);
            return false;
        }
    }
}
