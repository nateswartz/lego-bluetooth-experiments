using BluetoothController.Commands.Basic;
using BluetoothController.EventHandlers;
using BluetoothController.Hubs;
using BluetoothController.Models;
using BluetoothController.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BluetoothController.Controllers
{
    public interface IHubController
    {
        ILegoHub Hub { get; set; }
        GattCharacteristic HubCharacteristic { get; set; }
        bool IsConnected { get; }
        string SelectedBleDeviceId { get; set; }
        bool SubscribedForNotifications { get; set; }
        void AddEventHandler<T>(IEventHandler<T> eventHandler) where T : Response;
        Task ConnectAsync(Func<IHubController, string, Task> notificationHandler);
        Task DisconnectAsync();
        Task<bool> ExecuteCommandAsync(ICommand command);
        IEnumerable<IEventHandler<T>> GetEventHandlers<T>() where T : Response;
        IEnumerable<string> GetPortIdsByDeviceType(IOType deviceType);
        bool IsHandlerRegistered(Type eventType, Type eventHandlerType);
        void RemoveEventHandler<T>(IEventHandler<T> eventHandler) where T : Response;
        string ToString();
    }
}