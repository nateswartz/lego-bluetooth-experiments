using LegoBoostController.Commands;
using LegoBoostController.Controllers;
using LegoBoostController.Models;
using LegoBoostController.Responses;
using System;
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

        public NotificationManager(ResponseProcessor responseProcessor, StorageFolder storageFolder)
        {
            _responseProcessor = responseProcessor;
            _storageFolder = storageFolder;
        }

        public async Task ProcessNotification(string notification,
                                              bool syncMotorAndLED,
                                              BoostController controller)
        {

            var response = _responseProcessor.CreateResponse(notification);

            await TriggerActionsFromNotification(syncMotorAndLED, response, controller);

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

        private async Task TriggerActionsFromNotification(bool syncMotorAndLED, Response response, BoostController controller)
        {
            if (syncMotorAndLED && response.GetType() == typeof(SpeedData))
            {
                var data = (SpeedData)response;
                var color = LEDColors.Red;
                if (data.Speed > 30)
                {
                    color = LEDColors.Green;
                }
                else if (data.Speed > 1)
                {
                    color = LEDColors.Purple;
                }
                var command = new LEDBoostCommand(color);
                await controller.SetHexValueAsync(command.HexCommand);
            }
        }
    }
}
