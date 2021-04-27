using BluetoothController.Commands.Basic;
using BluetoothController.EventHandlers;
using BluetoothController.Hubs;
using BluetoothController.Models;
using BluetoothController.Responses;
using BluetoothController.Util;
using BluetoothController.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace BluetoothController.Controllers
{
    public class HubController : IHubController
    {
        public ILegoHub Hub { get; set; }

        public string SelectedBleDeviceId { get; }

        private IGattCharacteristicWrapper _hubCharacteristic;

        private Dictionary<string, List<object>> _eventHandlers { get; set; }

        private Func<IHubController, Response, Task> _notificationHandler;

        public HubController(ILegoHub legoHub, string selectedBleDeviceId)
        {
            _eventHandlers = new Dictionary<string, List<object>>();
            Hub = legoHub;
            SelectedBleDeviceId = selectedBleDeviceId;
        }

        public async Task<bool> ExecuteCommandAsync(ICommand command)
        {
            return await SetHexValueAsync(command.HexCommand);
        }

        public async Task InitializeAsync(Func<IHubController, Response, Task> notificationHandler, IGattCharacteristicWrapper gattCharacteristicWrapper)
        {
            _hubCharacteristic = gattCharacteristicWrapper;
            await ToggleSubscribedForNotificationsAsync(notificationHandler);
            await ExecuteCommandAsync(new HubTypeCommand());
            await ExecuteCommandAsync(new HubFirmwareCommand());
        }

        public IEnumerable<string> GetPortIdsByDeviceType(IOType deviceType)
        {
            return Hub.GetPortsByDeviceType(deviceType).Select(h => h.PortID);
        }

        public override string ToString()
        {
            return $"{Hub.HubType} ({SelectedBleDeviceId.Replace("BluetoothLE#BluetoothLE", "")})";
        }

        public void AddEventHandler<T>(IEventHandler<T> eventHandler) where T : Response
        {
            if (!_eventHandlers.ContainsKey(typeof(T).Name))
            {
                _eventHandlers[typeof(T).Name] = new List<object>();
            }
            _eventHandlers[typeof(T).Name].Add(eventHandler);
        }

        public void RemoveEventHandler<T>(IEventHandler<T> eventHandler) where T : Response
        {
            if (_eventHandlers.ContainsKey(typeof(T).Name))
            {
                _eventHandlers[typeof(T).Name].RemoveAll(x => x.GetType() == eventHandler.GetType());
            }
        }

        private async Task<Response> ProcessNotification(string notification)
        {
            var response = ResponseFactory.CreateResponse(notification, this);

            await TriggerActionsFromNotification(response);

            return response;
        }

        private async Task<bool> SetHexValueAsync(string hex)
        {
            var bytes = DataConverter.HexStringToByteArray(hex);

            var writer = new DataWriter
            {
                ByteOrder = ByteOrder.LittleEndian
            };
            writer.WriteBytes(bytes);

            var writeSuccessful = await _hubCharacteristic.WriteValueAsync(writer.DetachBuffer());
            return writeSuccessful;
        }

        private async Task TriggerActionsFromNotification(Response response)
        {
            if (!_eventHandlers.ContainsKey(response.NotificationType))
                return;
            var handlers = _eventHandlers[response.NotificationType];
            var handlersToRemove = new List<dynamic>();
            foreach (var handler in handlers)
            {
                dynamic dynamicHandler = handler;
                var shouldRemoveHandler = (bool)(await dynamicHandler.HandleEventAsync(response));
                if (shouldRemoveHandler)
                {
                    handlersToRemove.Add(dynamicHandler);
                }
            }
            foreach (var handlerToRemove in handlersToRemove)
            {
                handlers.Remove(handlerToRemove);
            }
        }

        private async Task<bool> ToggleSubscribedForNotificationsAsync(Func<IHubController, Response, Task> notificationHandler)
        {
            _notificationHandler = notificationHandler;
            try
            {
                _hubCharacteristic.AddValueChangedCallback(HubNotificationCallback);

                var writeSuccessful = await _hubCharacteristic.EnableNotificationsAsync();

                if (writeSuccessful)
                {
                    return true;
                }
                else
                {
                    _hubCharacteristic.RemoveValueChangedCallback();
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task HubNotificationCallback(IBuffer messageData)
        {
            var notification = ReadNotificationFromBuffer(messageData);
            var message = await ProcessNotification(notification);
            await _notificationHandler(this, message);
        }

        private static string ReadNotificationFromBuffer(IBuffer buffer)
        {
            var output = new byte[buffer.Length];
            var dataReader = DataReader.FromBuffer(buffer);
            dataReader.ReadBytes(output);
            return DataConverter.ByteArrayToString(output);
        }
    }
}
