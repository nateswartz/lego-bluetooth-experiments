using BluetoothController.Commands.Basic;
using BluetoothController.EventHandlers;
using BluetoothController.Hubs;
using BluetoothController.Models;
using BluetoothController.Responses;
using BluetoothController.Wrappers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BluetoothController.Controllers
{
    public interface IHubController
    {
        ILegoHub Hub { get; set; }
        string SelectedBleDeviceId { get; }
        void AddEventHandler<T>(IEventHandler<T> eventHandler) where T : Response;
        Task InitializeAsync(Func<IHubController, Response, Task> notificationHandler, IGattCharacteristicWrapper gattCharacteristicWrapper);
        Task<bool> ExecuteCommandAsync(ICommand command);
        IEnumerable<string> GetPortIdsByDeviceType(IoDeviceType deviceType);
        void RemoveEventHandler<T>(IEventHandler<T> eventHandler) where T : Response;
        string ToString();
    }
}