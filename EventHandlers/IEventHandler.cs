using LegoBoostController.Responses;
using System;
using System.Threading.Tasks;

namespace LegoBoostController.EventHandlers
{
    public interface IEventHandler
    {
        Task HandleEventAsync(Response response);
        Type HandledEvent { get; }
    }
}
