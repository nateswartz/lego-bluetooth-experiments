using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses;
using BluetoothController.Responses.Device.Data;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class MotorToRgbLightEventHandler : EventHandlerBase, IEventHandler<BoostMotorData>
    {
        public MotorToRgbLightEventHandler(IHubController controller) : base(controller) { }

        public async Task<bool> HandleEventAsync(Response response)
        {
            var data = (BoostMotorData)response;
            var color = RgbLightColors.Red;
            if (data.Speed > 30)
            {
                color = RgbLightColors.Green;
            }
            else if (data.Speed > 1)
            {
                color = RgbLightColors.Purple;
            }
            var command = new RgbLightCommand(_controller, color);
            await _controller.ExecuteCommandAsync(command);
            return false;
        }
    }
}
