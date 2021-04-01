using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses.Device.Data;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class MotorToLEDEventHandler : EventHandlerBase, IEventHandler<ExternalMotorData>
    {
        public MotorToLEDEventHandler(IHubController controller) : base(controller) { }

        public async Task HandleEventAsync(ExternalMotorData response)
        {
            var color = LEDColors.Red;
            if (response.Speed > 30)
            {
                color = LEDColors.Green;
            }
            else if (response.Speed > 1)
            {
                color = LEDColors.Purple;
            }
            var command = new LEDCommand(_controller, color);
            await _controller.ExecuteCommandAsync(command);
        }
    }
}
