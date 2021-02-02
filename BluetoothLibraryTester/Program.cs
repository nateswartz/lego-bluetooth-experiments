using LegoBoostController;
using LegoBoostController.Commands.Boost;
using LegoBoostController.Controllers;
using LegoBoostController.Models;
using System;
using System.Threading.Tasks;

namespace BluetoothLibraryTester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var adapter = new BluetoothAdapter();
            adapter.StartBleDeviceWatcher(HandleDiscover, HandleConnect);

            while (true)
            {
                await Task.Delay(1000);
                Console.WriteLine("Waiting...");
            }
        }

        static async Task HandleDiscover(string input)
        {
            Console.WriteLine($"Discovered device: {input}");
            await Task.CompletedTask;
        }

        static async Task HandleConnect(HubController controller)
        {
            Console.WriteLine($"Connected device: {Enum.GetName(typeof(HubType), controller.HubType)}");
            await controller.ExecuteCommandAsync(new LEDBoostCommand(LEDColors.Yellow));
            await Task.Delay(2000);
            await controller.ExecuteCommandAsync(new DisconnectCommand());
            await Task.CompletedTask;
        }
    }
}
