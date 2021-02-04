using BluetoothController.Commands.Boost;
using BluetoothController.EventHandlers;
using BluetoothController.Models;
using BluetoothController.Responses;
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
        TwoPortHub = 2
    }

    public class HubController
    {
        public GattCharacteristic HubCharacteristic { get; set; }

        public PortState PortState { get; set; } = new PortState();

        public string SelectedBleDeviceId { get; set; }

        public HubType HubType { get; set; }

        public bool IsConnected { get; private set; }

        public bool SubscribedForNotifications { get; set; }

        private Dictionary<string, List<IEventHandler>> _eventHandlers { get; set; }

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

        public async Task<bool> SetHexValueAsync(string hex)
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

        public async Task ConnectAsync()
        {
            IsConnected = true;
            await ExecuteCommandAsync(new HubTypeCommand());
        }

        public void Disconnect()
        {
            IsConnected = false;
        }

        public override string ToString()
        {
            return $"{Enum.GetName(typeof(HubType), HubType)} ({SelectedBleDeviceId})";
        }

        private async Task<bool> WriteBufferToMoveHubCharacteristicAsync(IBuffer buffer)
        {
            try
            {
                // BT_Code: Writes the value from the buffer to the characteristic.
                var result = await HubCharacteristic.WriteValueWithResultAsync(buffer);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    //rootPage.NotifyUser("Successfully wrote value to device", NotifyType.StatusMessage);
                    return true;
                }
                else
                {
                    //rootPage.NotifyUser($"Write failed: {result.Status}", NotifyType.ErrorMessage);
                    return false;
                }
            }
            //catch (Exception ex) when (ex.HResult == E_BLUETOOTH_ATT_INVALID_PDU)
            //{
            //    //rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
            //    return false;
            //}
            catch (Exception)// when (ex.HResult == E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED || ex.HResult == E_ACCESSDENIED)
            {
                // This usually happens when a device reports that it support writing, but it actually doesn't.
                //rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                return false;
            }
        }

        internal async Task<string> ProcessNotification(string notification)
        {
            var response = ResponseProcessor.CreateResponse(notification, PortState);

            try
            {
                var hubTypeCommand = (SystemType)response;
                HubType = hubTypeCommand.HubType;
            }
            catch (Exception)
            {
                // Command was not a Hub Type command
                // TODO: Find a better way to check this
            }

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
    }
}
