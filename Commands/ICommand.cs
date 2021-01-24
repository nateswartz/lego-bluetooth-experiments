using LegoBoostController.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegoBoostController.Commands
{
    public interface ICommand
    {
        IEnumerable<string> Keywords { get; }
        string Description { get; }
        Task RunAsync(BoostController controller, string commandText);
    }
}


