using BluetoothController.Responses;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public interface IEventHandler
    {
        Task HandleEventAsync(Response response);
        Type HandledEvent { get; }
    }
}
