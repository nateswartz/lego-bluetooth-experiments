using BluetoothController.EventHandlers;
using BluetoothController.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BluetoothController.Util
{
    public class NotificationManager
    {
        private HubController _controller;
        private Dictionary<string, List<IEventHandler>> _eventHandlers { get; set; }

        public NotificationManager(HubController controller)
        {
            _controller = controller;
            _eventHandlers = new Dictionary<string, List<IEventHandler>>();
        }

        public async Task ProcessNotification(string notification)
        {
            Console.WriteLine(notification);
            var response = ResponseProcessor.CreateResponse(notification, _controller.PortState);

            try
            {
                var hubTypeCommand = (SystemType)response;
                _controller.HubType = hubTypeCommand.HubType;
            }
            catch (Exception)
            {
                // Command was not a Hub Type command
                // TODO: Find a better way to check this
            }

            await TriggerActionsFromNotification(response);

            var message = DecodeNotification(notification);
        }

        public string DecodeNotification(string notification)
        {
            var response = ResponseProcessor.CreateResponse(notification, _controller.PortState);
            var message = response.ToString();
            return message;
        }

        public void AddEventHandler(IEventHandler eventHandler)
        {
            if (!_eventHandlers.ContainsKey(eventHandler.HandledEvent.Name))
            {
                _eventHandlers[eventHandler.HandledEvent.Name] = new List<IEventHandler>();
            }
            _eventHandlers[eventHandler.HandledEvent.Name].Add(eventHandler);
        }

        public List<IEventHandler> GetEventHandlers(Type eventType)
        {
            return _eventHandlers[eventType.Name] ?? new List<IEventHandler>();
        }

        public bool IsHandlerRegistered(Type eventType, Type eventHandlerType)
        {
            var hasHandlers = _eventHandlers.ContainsKey(eventType.Name) && _eventHandlers[eventType.Name] != null && _eventHandlers[eventType.Name].Count > 0;
            if (!hasHandlers)
                return false;
            return _eventHandlers[eventType.Name].Exists(x => x.GetType() == eventHandlerType);
        }

        public void RemoveEventHandler(IEventHandler eventHandler)
        {
            if (_eventHandlers.ContainsKey(eventHandler.HandledEvent.Name))
            {
                _eventHandlers[eventHandler.HandledEvent.Name].RemoveAll(x => x.GetType() == eventHandler.GetType());
            }
        }

        private async Task TriggerActionsFromNotification(Response response)
        {
            if (!_eventHandlers.ContainsKey(response.NotificationType))
                return;
            var handlers = _eventHandlers[response.NotificationType];
            foreach (var handler in handlers)
            {
                await handler.HandleEventAsync(response);
            }
        }
    }
}
