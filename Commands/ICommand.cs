using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDKTemplate.Commands
{
    public interface ICommand
    {
        IEnumerable<string> Keywords { get; }
        Task RunAsync(BoostController controller, string commandText);
    }
}


