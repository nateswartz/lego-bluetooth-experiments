using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class RemoteButtonToLEDEventHandler : IEventHandler
    {
        private readonly HubController _controller;

        public Type HandledEvent { get; } = typeof(RemoteButtonData);

        public RemoteButtonToLEDEventHandler(HubController controller)
        {
            _controller = controller;
        }

        public async Task HandleEventAsync(Response response)
        {
            var data = (RemoteButtonData)response;
            LEDColor color = LEDColors.Red;
            if (data.PlusPressed)
            {
                color = LEDColors.Yellow;
            }
            else if (data.RedPressed)
            {
                color = LEDColors.Red;
            }
            else if (data.MinusPressed)
            {
                color = LEDColors.Pink;
            }
            if (!data.PlusPressed && !data.MinusPressed && !data.RedPressed)
            {
                color = LEDColors.None;
            }
            var command = new LEDCommand(_controller, color);
            await _controller.ExecuteCommandAsync(command);
        }
    }
}
