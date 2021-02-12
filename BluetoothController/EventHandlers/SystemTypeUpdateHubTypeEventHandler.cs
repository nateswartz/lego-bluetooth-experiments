using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Responses;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class SystemTypeUpdateHubTypeEventHandler : IEventHandler
    {
        private readonly HubController _controller;

        public Type HandledEvent { get; } = typeof(SystemType);

        public SystemTypeUpdateHubTypeEventHandler(HubController controller)
        {
            _controller = controller;
        }

        public async Task HandleEventAsync(Response response)
        {
            var data = (SystemType)response;
            SetupHub(data.HubType);
            await Task.CompletedTask;
        }

        private void SetupHub(HubType hubType)
        {
            switch (hubType)
            {
                case HubType.BoostMoveHub:
                    if (_controller.Hub is BoostMoveHub)
                        return;
                    var moveHub = new BoostMoveHub();
                    if (_controller.Hub != null)
                        moveHub.ChangeablePorts = ((HubWithChangeablePorts)_controller.Hub).ChangeablePorts;
                    _controller.Hub = moveHub;
                    break;
                case HubType.TwoPortHandset:
                    if (_controller.Hub == null)
                        _controller.Hub = new RemoteHub();
                    break;
                case HubType.TwoPortHub:
                    if (_controller.Hub is TwoPortHub)
                        return;
                    var twoPortHub = new TwoPortHub();
                    if (_controller.Hub != null)
                        twoPortHub.ChangeablePorts = ((HubWithChangeablePorts)_controller.Hub).ChangeablePorts;
                    _controller.Hub = twoPortHub;
                    break;
            }
        }
    }
}
