using BluetoothController.Commands.Basic;
using BluetoothController.EventHandlers;
using BluetoothController.Hubs;
using BluetoothController.Models;
using BluetoothController.Responses;
using BluetoothController.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BluetoothController.Controllers
{
    public class HubController : IHubController
    {
        public bool IsConnected { get; private set; }

        public ILegoHub Hub { get; set; }

        public string SelectedBleDeviceId { get; set; }

        public bool SubscribedForNotifications { get; set; }

        public GattCharacteristic HubCharacteristic { get; set; }

        private Dictionary<string, List<IEventHandler>> _eventHandlers { get; set; }

        private readonly List<string> _notifications = new();

        private Func<IHubController, string, Task> _notificationHandler;

        public HubController()
        {
            _eventHandlers = new Dictionary<string, List<IEventHandler>>();
            Hub = new LegoHub();
        }

        public async Task<bool> ExecuteCommandAsync(ICommand command)
        {
            return await SetHexValueAsync(command.HexCommand);
        }

        private async Task<bool> SetHexValueAsync(string hex)
        {
            var bytes = DataConverter.HexStringToByteArray(hex);

            var writer = new DataWriter
            {
                ByteOrder = ByteOrder.LittleEndian
            };
            writer.WriteBytes(bytes);

            var writeSuccessful = await WriteBufferToMoveHubCharacteristicAsync(writer.DetachBuffer());
            return writeSuccessful;
        }

        public async Task ConnectAsync(Func<IHubController, string, Task> notificationHandler)
        {
            IsConnected = true;
            await ToggleSubscribedForNotificationsAsync(notificationHandler);
            await ExecuteCommandAsync(new HubTypeCommand());
        }

        public async Task DisconnectAsync()
        {
            IsConnected = false;
            await ExecuteCommandAsync(new DisconnectCommand());
            await ToggleSubscribedForNotificationsAsync(null);
        }

        public IEnumerable<string> GetPortIdsByDeviceType(IOType deviceType)
        {
            return Hub.GetPortsByDeviceType(deviceType).Select(h => h.PortID);
        }

        public override string ToString()
        {
            return $"{Hub.HubType} ({SelectedBleDeviceId})";
        }

        private async Task<bool> WriteBufferToMoveHubCharacteristicAsync(IBuffer buffer)
        {
            try
            {
                var result = await HubCharacteristic.WriteValueWithResultAsync(buffer);

                return result.Status == GattCommunicationStatus.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal async Task<string> ProcessNotification(string notification)
        {
            var response = ResponseFactory.CreateResponse(notification, this);

            await TriggerActionsFromNotification(response);

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

        private async Task<bool> ToggleSubscribedForNotificationsAsync(Func<IHubController, string, Task> notificationHandler)
        {
            _notificationHandler = notificationHandler;
            var enableNotifications = !SubscribedForNotifications;
            try
            {
                SubscribedForNotifications = enableNotifications;
                if (enableNotifications)
                    HubCharacteristic.ValueChanged += Characteristic_ValueChanged;
                else
                    HubCharacteristic.ValueChanged -= Characteristic_ValueChanged;

                var status = await
                    HubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                        enableNotifications ? GattClientCharacteristicConfigurationDescriptorValue.Notify
                               : GattClientCharacteristicConfigurationDescriptorValue.None);

                if (status == GattCommunicationStatus.Success)
                {
                    return true;
                }
                else
                {
                    if (enableNotifications)
                        HubCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                    else
                        HubCharacteristic.ValueChanged += Characteristic_ValueChanged;
                    SubscribedForNotifications = !enableNotifications;
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var notification = ReadNotificationFromBuffer(args.CharacteristicValue);
            var message = await ProcessNotification(notification);
            _notifications.Add(message);
            if (_notifications.Count > 10)
            {
                _notifications.RemoveAt(0);
            }
            await _notificationHandler(this, message);
        }

        private string ReadNotificationFromBuffer(IBuffer buffer)
        {
            var output = new byte[buffer.Length];
            var dataReader = DataReader.FromBuffer(buffer);
            dataReader.ReadBytes(output);
            return DataConverter.ByteArrayToString(output);
        }
    }
}
