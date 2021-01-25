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
        public List<IEventHandler> EventHandlers { get; set; }

        public NotificationManager(ResponseProcessor responseProcessor, StorageFolder storageFolder)
        {
            _responseProcessor = responseProcessor;
            _storageFolder = storageFolder;
            EventHandlers = new List<IEventHandler>();
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
            foreach (var handler in EventHandlers)
            {
                await handler.HandleEventAsync(response);
            }
        }
    }
}
