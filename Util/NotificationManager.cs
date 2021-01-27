using LegoBoostController.EventHandlers;
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
        private Dictionary<Type, List<IEventHandler>> _eventHandlers { get; set; }

        public NotificationManager(ResponseProcessor responseProcessor, StorageFolder storageFolder)
        {
            _responseProcessor = responseProcessor;
            _storageFolder = storageFolder;
            _eventHandlers = new Dictionary<Type, List<IEventHandler>>();
        }

        public async Task ProcessNotification(string notification)
        {
            var response = _responseProcessor.CreateResponse(notification);

            await TriggerActionsFromNotification(response);

            var message = DecodeNotification(notification);

            await StoreNotification(_storageFolder, message);
        }

        public string DecodeNotification(string notification)
        {
            var response = _responseProcessor.CreateResponse(notification);
            var message = response.ToString();
            return message;
        }

        public void AddEventHandler(IEventHandler eventHandler)
        {
            if (!_eventHandlers.ContainsKey(eventHandler.HandledEvent))
            {
                _eventHandlers[eventHandler.HandledEvent] = new List<IEventHandler>();
            }
            _eventHandlers[eventHandler.HandledEvent].Add(eventHandler);
        }

        public List<IEventHandler> GetEventHandlers(Type eventType)
        {
            return _eventHandlers[eventType] ?? new List<IEventHandler>();
        }

        public bool IsHandlerRegistered(Type eventType, Type eventHandlerType)
        {
            var hasHandlers = _eventHandlers[eventType] != null && _eventHandlers[eventType].Count > 0;
            if (!hasHandlers)
                return false;
            return _eventHandlers[eventType].Exists(x => x.GetType() == eventHandlerType);
        }

        public void RemoveEventHandler(IEventHandler eventHandler)
        {
            if (_eventHandlers.ContainsKey(eventHandler.HandledEvent))
            {
                _eventHandlers[eventHandler.HandledEvent].RemoveAll(x => x.GetType() == eventHandler.GetType());
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
            if (!_eventHandlers.ContainsKey(response.GetType()))
                return;
            var handlers = _eventHandlers[response.GetType()];
            foreach (var handler in handlers)
            {
                await handler.HandleEventAsync(response);
            }
        }
    }
}
