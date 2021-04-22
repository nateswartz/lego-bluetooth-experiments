using BluetoothController.Responses;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public interface IEventHandler<T> where T : Response
    {
        /// <summary>
        /// Handle provided event of type T.
        /// </summary>
        /// <param name="response">Response of type T to process.</param>
        /// <returns>Whether or not to unregister this event handler after execution.</returns>
        Task<bool> HandleEventAsync(Response response);
    }
}
