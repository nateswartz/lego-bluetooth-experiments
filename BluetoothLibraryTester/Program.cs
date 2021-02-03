using BluetoothController;
using BluetoothController.Commands.Boost;
using BluetoothController.Controllers;
using BluetoothController.EventHandlers;
using BluetoothController.Models;
using BluetoothController.Util;
using System;
using System.Threading.Tasks;

namespace BluetoothLibraryTester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var adapter = new BluetoothLowEnergyAdapter();
            Console.WriteLine("Searching for devices...");
            adapter.StartBleDeviceWatcher(HandleDiscover, HandleConnect);

            while (true)
            {
                await Task.Delay(1000);
            }
        }

        static async Task HandleDiscover(DiscoveredDevice device)
        {
            Console.WriteLine($"Discovered device: {device.Name}");
            await Task.CompletedTask;
        }

        static async Task HandleConnect(HubController controller, NotificationManager notificationManager, string errorMessage)
        {
            if (controller != null)
            {
                Console.WriteLine($"Connected device: {Enum.GetName(typeof(HubType), controller.HubType)}");
                Console.WriteLine($"Setting LED Yellow...");
                await controller.ExecuteCommandAsync(new LEDBoostCommand(LEDColors.Yellow));
                await Task.Delay(500);
                Console.WriteLine($"Registering for Button notifications...");
                await controller.ExecuteCommandAsync(new ButtonNotificationsCommand(true));
                await Task.Delay(500);
                Console.WriteLine("Registering Event handler to change LED on Button press...");
                notificationManager.AddEventHandler(new ButtonToLEDEventHandler(controller));
                while (true)
                {
                    await Task.Delay(1000);
                }
                await controller.ExecuteCommandAsync(new DisconnectCommand());
                await Task.CompletedTask;
            }
            else
            {
                Console.WriteLine($"Failed to connect: {errorMessage}");
            }
        }
    }
}
