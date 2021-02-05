using BluetoothController;
using BluetoothController.Commands.Boost;
using BluetoothController.Controllers;
using BluetoothController.EventHandlers;
using BluetoothController.Models;
using System;
using System.Threading.Tasks;

namespace BluetoothLibraryTester
{
    class Program
    {
        static BluetoothLowEnergyAdapter _adapter;
        static async Task Main(string[] args)
        {
            _adapter = new BluetoothLowEnergyAdapter(HandleDiscover, HandleConnect, HandleNotification);
            Console.WriteLine("Searching for devices...");
            _adapter.StartBleDeviceWatcher();

            while (true)
            {
                await Task.Delay(1000);
            }
        }

        static async Task HandleNotification(HubController controller, string message)
        {
            Console.WriteLine($"{controller.HubType}: {message}");
            await Task.CompletedTask;
        }

        static async Task HandleDiscover(DiscoveredDevice device)
        {
            Console.WriteLine($"Discovered device: {device.Name}");
            await Task.CompletedTask;
        }

        static async Task HandleConnect(HubController controller, string errorMessage)
        {
            if (controller != null)
            {
                Console.WriteLine($"Connected device: {Enum.GetName(typeof(HubType), controller.HubType)}");
                await controller.ExecuteCommandAsync(new HubFirmwareCommand());
                await controller.ExecuteCommandAsync(new ToggleNotificationsCommand(controller, true, PortType.Motor, "01"));
                Console.WriteLine($"Setting LED Yellow...");
                await controller.ExecuteCommandAsync(new LEDBoostCommand(LEDColors.Yellow));
                await Task.Delay(500);
                Console.WriteLine($"Registering for Button notifications...");
                await controller.ExecuteCommandAsync(new ButtonNotificationsCommand(true));
                await Task.Delay(500);
                Console.WriteLine("Registering Event handler to change LED on Button press...");
                controller.AddEventHandler(new ButtonToLEDEventHandler(controller));
                await Task.Delay(6000);
                Console.WriteLine("Disconnecting soon...");
                await Task.Delay(2000);
                await controller.DisconnectAsync();
                _adapter.StopBleDeviceWatcher();
                Console.WriteLine("Disconnected");
                await Task.CompletedTask;
            }
            else
            {
                Console.WriteLine($"Failed to connect: {errorMessage}");
            }
        }
    }
}
