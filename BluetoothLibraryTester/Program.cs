using BluetoothController;
using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BluetoothLibraryTester
{
    class Program
    {
        static IBluetoothLowEnergyAdapter _adapter;

        static List<IHubController> _controllers = new List<IHubController>();

        static async Task Main(string[] args)
        {
            try
            {
                Console.CancelKeyPress += delegate
                {
                    Disconnect().GetAwaiter().GetResult();
                    Environment.Exit(0);
                };

                _adapter = new BluetoothLowEnergyAdapter(HandleDiscover, HandleConnect, HandleNotification);
                Console.WriteLine("Searching for devices...");
                _adapter.StartBleDeviceWatcher();

                while (!_controllers.Any())
                {
                    await Task.Delay(100);
                }

                await GetInfo();
                Console.WriteLine("Running test method...");
                await RunCommands(_controllers.First());
                await Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Encountered exception: {e.Message}");
                await Disconnect();
            }
        }

        static async Task GetInfo()
        {
            foreach (var controller in _controllers)
            {
                await controller.ExecuteCommandAsync(new HubNameCommand());
                await controller.ExecuteCommandAsync(new HubFirmwareCommand());
                await controller.ExecuteCommandAsync(new HubTypeCommand());
            }
            await Task.Delay(1000);
        }

        static async Task RunCommands(IHubController controller)
        {
            var port = controller.GetPortIdsByDeviceType(IOTypes.VoltageSensor).First();

            await controller.ExecuteCommandAsync(new PortInfoCommand(port, InfoType.PossibleModeCombinations));
            await Task.Delay(1000);
            await controller.ExecuteCommandAsync(new PortInfoCommand(port, InfoType.ModeInfo));
            await Task.Delay(1000);
            await controller.ExecuteCommandAsync(new PortInfoModeCommand(port, "00", ModeInfoType.Mapping));
            await Task.Delay(1000);
            await controller.ExecuteCommandAsync(new PortInfoModeCommand(port, "01", ModeInfoType.Mapping));
        }

        static async Task Disconnect()
        {
            Console.WriteLine("Disconnecting soon...");
            await Task.Delay(1000);
            foreach (var controller in _controllers)
                await controller.ExecuteCommandAsync(new ShutdownCommand());
            Console.WriteLine("Disconnected");
        }

        static async Task HandleNotification(IHubController controller, string message)
        {
            Console.WriteLine($"{controller.HubType}: {message}");
            await Task.CompletedTask;
        }

        static async Task HandleDiscover(DiscoveredDevice device)
        {
            Console.WriteLine($"Discovered device: {device.Name}");
            await Task.CompletedTask;
        }

        static async Task HandleConnect(IHubController controller, string errorMessage)
        {
            if (controller != null)
            {
                _controllers.Add(controller);

                Console.WriteLine($"Connected device: {Enum.GetName(typeof(HubType), controller.HubType)}");

                await Task.CompletedTask;
            }
            else
            {
                Console.WriteLine($"Failed to connect: {errorMessage}");
            }
        }
    }
}
