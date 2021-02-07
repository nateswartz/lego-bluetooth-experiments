using BluetoothController.Commands.Boost;
using BluetoothController.EventHandlers;
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
    public enum HubType
    {
        BoostMoveHub = 1,
        TwoPortHub = 2,
        TwoPortHandset = 3,
        Unknown = 4
    }

    public class HubController
    {
        public HubType HubType { get; set; }

        public bool IsConnected { get; private set; }

        internal PortState PortState { get; set; } = new PortState();

        internal string SelectedBleDeviceId { get; set; }

        internal bool SubscribedForNotifications { get; set; }

        internal GattCharacteristic HubCharacteristic { get; set; }

        private Dictionary<string, List<IEventHandler>> _eventHandlers { get; set; }

        private List<string> _notifications = new List<string>();

        private Func<HubController, string, Task> _notificationHandler;

        public HubController()
        {
            _eventHandlers = new Dictionary<string, List<IEventHandler>>();
        }

        public string GetCurrentExternalMotorPort()
        {
            return PortState.CurrentExternalMotorPort;
        }

        public async Task<bool> ExecuteCommandAsync(IBoostCommand command)
        {
            return await SetHexValueAsync(command.HexCommand);
        }

        private async Task<bool> SetHexValueAsync(string hex)
        {
            if (hex.Contains(" "))
            {
                hex = hex.Replace(" ", "");
            }
            var bytes = Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();

            var writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteBytes(bytes);

            var writeSuccessful = await WriteBufferToMoveHubCharacteristicAsync(writer.DetachBuffer());
            return writeSuccessful;
        }

        public async Task ConnectAsync(Func<HubController, string, Task> notificationHandler)
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

        public override string ToString()
        {
            return $"{Enum.GetName(typeof(HubType), HubType)} ({SelectedBleDeviceId})";
        }

        private async Task<bool> WriteBufferToMoveHubCharacteristicAsync(IBuffer buffer)
        {
            try
            {
                var result = await HubCharacteristic.WriteValueWithResultAsync(buffer);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal async Task<string> ProcessNotification(string notification)
        {
            var response = ResponseProcessor.CreateResponse(notification, PortState);

            await TriggerActionsFromNotification(response);

            return DecodeNotification(notification);
        }

        private string DecodeNotification(string notification)
        {
            var response = ResponseProcessor.CreateResponse(notification, PortState);
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

        private async Task<bool> ToggleSubscribedForNotificationsAsync(Func<HubController, string, Task> notificationHandler)
        {
            _notificationHandler = notificationHandler;
            if (!SubscribedForNotifications)
            {
                // initialize status
                GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
                var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;

                try
                {
                    HubCharacteristic.ValueChanged += Characteristic_ValueChanged;
                    SubscribedForNotifications = true;
                    // BT_Code: Must write the CCCD in order for server to send indications.
                    // We receive them in the ValueChanged event handler.
                    status = await HubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

                    if (status == GattCommunicationStatus.Success)
                    {
                        //_rootPage.NotifyUser("Successfully subscribed for value changes", NotifyType.StatusMessage);
                        return true;
                    }
                    else
                    {
                        HubCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                        SubscribedForNotifications = false;
                        //_rootPage.NotifyUser($"Error registering for value changes: {status}", NotifyType.ErrorMessage);
                        return false;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support indicate, but it actually doesn't.
                    //_rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                    return false;
                }
            }
            else
            {
                try
                {
                    // BT_Code: Must write the CCCD in order for server to send notifications.
                    // We receive them in the ValueChanged event handler.
                    // Note that this sample configures either Indicate or Notify, but not both.
                    var result = await
                        HubCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.None);

                    if (result == GattCommunicationStatus.Success)
                    {
                        SubscribedForNotifications = false;
                        HubCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                        //_rootPage.NotifyUser("Successfully un-registered for notifications", NotifyType.StatusMessage);
                        return true;
                    }
                    else
                    {
                        //_rootPage.NotifyUser($"Error un-registering for notifications: {result}", NotifyType.ErrorMessage);
                        return false;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it supports notify, but it actually doesn't.
                    //_rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                    return false;
                }
            }
        }

        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var output = new byte[args.CharacteristicValue.Length];
            var dataReader = DataReader.FromBuffer(args.CharacteristicValue);
            dataReader.ReadBytes(output);
            var notification = DataConverter.ByteArrayToString(output);
            var message = await ProcessNotification(notification);
            _notifications.Add(message);
            if (_notifications.Count > 10)
            {
                _notifications.RemoveAt(0);
            }
            await _notificationHandler(this, message);
        }
    }
}
