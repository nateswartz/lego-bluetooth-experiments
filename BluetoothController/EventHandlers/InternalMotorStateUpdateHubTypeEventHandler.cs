using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Responses;
using BluetoothController.Responses.Device.State;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    internal class InternalMotorStateUpdateHubTypeEventHandler : IEventHandler
    {
        private readonly IHubController _controller;

        public Type HandledEvent { get; } = typeof(InternalMotorState);

        public InternalMotorStateUpdateHubTypeEventHandler(IHubController controller)
        {
            _controller = controller;
        }

        public async Task HandleEventAsync(Response response)
        {
            _controller.Hub.HubType = HubType.BoostMoveHub;
            await Task.CompletedTask;
        }
    }
}
