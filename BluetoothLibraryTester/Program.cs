using LegoBoostController;
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

        static async Task HandleConnect(string input)
        {
            Console.WriteLine($"Connected device: {input}");
            await Task.CompletedTask;
        }
    }
}
