using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses;
using BluetoothController.Responses.Device.State;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers.Internal
{
    internal class InternalMotorStateUpdateHubTypeEventHandler : EventHandlerBase, IEventHandler
    {
        public Type HandledEvent { get; } = typeof(InternalMotorState);

        public InternalMotorStateUpdateHubTypeEventHandler(IHubController controller) : base(controller) { }

        public async Task HandleEventAsync(Response response)
        {
            _controller.Hub.HubType = HubType.BoostMoveHub;
            await Task.CompletedTask;
        }
    }
}
