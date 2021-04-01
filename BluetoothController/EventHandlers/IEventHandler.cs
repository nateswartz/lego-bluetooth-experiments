using BluetoothController.Responses;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public interface IEventHandler<T> where T : Response
    {
        Task HandleEventAsync(T response);
    }
}
