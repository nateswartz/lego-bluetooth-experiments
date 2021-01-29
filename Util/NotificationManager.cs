using LegoBoostController.Controllers;
using LegoBoostController.EventHandlers;
using LegoBoostController.Models;
using LegoBoostController.Responses;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace LegoBoostController.Util
{
    public class NotificationManager
    {
        private SemaphoreSlim _logSemaphore = new SemaphoreSlim(1);
        private readonly ResponseProcessor _responseProcessor;
        private readonly StorageFolder _storageFolder;
        private const string _logFile = "move-hub-notifications.log";
        private Dictionary<string, List<IEventHandler>> _eventHandlers { get; set; }

        public NotificationManager(ResponseProcessor responseProcessor, StorageFolder storageFolder)
        {
            _responseProcessor = responseProcessor;
            _storageFolder = storageFolder;
            _eventHandlers = new Dictionary<string, List<IEventHandler>>();
        }

        public async Task ProcessNotification(string notification, HubController controller)
        {
            var response = _responseProcessor.CreateResponse(notification, controller.PortState);

            try
            {
                var hubTypeCommand = (SystemType)response;
                controller.HubType = hubTypeCommand.HubType;
            }
            catch (Exception)
            {
                // Command was not a Hub Type command
                // TODO: Find a better way to check this
            }

            await TriggerActionsFromNotification(response);

            var message = DecodeNotification(notification, controller.PortState);

            await StoreNotification(_storageFolder, message);
        }

        public string DecodeNotification(string notification, PortState portState)
        {
            var response = _responseProcessor.CreateResponse(notification, portState);
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

        private async Task StoreNotification(StorageFolder storageFolder, string notification)
        {
            var logFile = await storageFolder.CreateFileAsync(_logFile, CreationCollisionOption.OpenIfExists);
            await _logSemaphore.WaitAsync();
            try
            {
                await FileIO.AppendTextAsync(logFile, $"{DateTime.Now}: {notification}{Environment.NewLine}");
            }
            catch
            { }
            finally
            {
                _logSemaphore.Release();
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
