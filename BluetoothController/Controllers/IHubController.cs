using BluetoothController.Commands.Basic;
using BluetoothController.EventHandlers;
using BluetoothController.Hubs;
using BluetoothController.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BluetoothController.Controllers
{
    public interface IHubController
    {
        LegoHub Hub { get; set; }
        HubType HubType { get; }
        bool IsConnected { get; }

        void AddEventHandler(IEventHandler eventHandler);
        Task ConnectAsync(Func<HubController, string, Task> notificationHandler);
        Task DisconnectAsync();
        Task<bool> ExecuteCommandAsync(ICommand command);
        List<IEventHandler> GetEventHandlers(Type eventType);
        IEnumerable<string> GetPortIdsByDeviceType(IOType deviceType);
        bool IsHandlerRegistered(Type eventType, Type eventHandlerType);
        void RemoveEventHandler(IEventHandler eventHandler);
        string ToString();
    }
}