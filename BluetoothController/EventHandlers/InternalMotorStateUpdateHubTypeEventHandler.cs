using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Responses;
using BluetoothController.Responses.State;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class InternalMotorStateUpdateHubTypeEventHandler : IEventHandler
    {
        private readonly HubController _controller;

        public Type HandledEvent { get; } = typeof(InternalMotorState);

        public InternalMotorStateUpdateHubTypeEventHandler(HubController controller)
        {
            _controller = controller;
        }

        public async Task HandleEventAsync(Response response)
        {
            if (_controller.Hub is BoostMoveHub)
                return;
            var moveHub = new BoostMoveHub();
            if (_controller.Hub != null)
                moveHub.Ports = _controller.Hub.Ports;
            _controller.Hub = moveHub;
            await Task.CompletedTask;
        }
    }
}
