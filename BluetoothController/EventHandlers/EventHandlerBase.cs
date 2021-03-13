using BluetoothController.Controllers;

namespace BluetoothController.EventHandlers
{
    public class EventHandlerBase
    {
        protected readonly IHubController _controller;

        public EventHandlerBase(IHubController controller)
        {
            _controller = controller;
        }
    }
}