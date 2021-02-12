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
                    var boostHub = new BoostMoveHub();
                    if (_controller.Hub != null && _controller.Hub.GetType() == typeof(BoostMoveHub))
                    {
                        boostHub.CurrentExternalMotorPort = ((BoostMoveHub)_controller.Hub).CurrentExternalMotorPort;
                        boostHub.CurrentColorDistanceSensorPort = ((BoostMoveHub)_controller.Hub).CurrentColorDistanceSensorPort;
                        boostHub.CurrentTrainMotorPort = ((BoostMoveHub)_controller.Hub).CurrentTrainMotorPort;
                    }
                    _controller.Hub = boostHub;
                    break;
                case HubType.TwoPortHandset:
                    _controller.Hub = new RemoteHub();
                    break;
                case HubType.TwoPortHub:
                    var twoPortHub = new TwoPortHub();
                    if (_controller.Hub != null && _controller.Hub.GetType() == typeof(TwoPortHub))
                    {
                        twoPortHub.CurrentExternalMotorPort = ((TwoPortHub)_controller.Hub).CurrentExternalMotorPort;
                        twoPortHub.CurrentColorDistanceSensorPort = ((TwoPortHub)_controller.Hub).CurrentColorDistanceSensorPort;
                        twoPortHub.CurrentTrainMotorPort = ((TwoPortHub)_controller.Hub).CurrentTrainMotorPort;
                    }
                    _controller.Hub = twoPortHub;
                    break;
            }
        }
    }
}
