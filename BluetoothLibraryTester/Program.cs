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

        static HubController _controller;

        static async Task Main(string[] args)
        {
            _adapter = new BluetoothLowEnergyAdapter(HandleDiscover, HandleConnect, HandleNotification);
            Console.WriteLine("Searching for devices...");
            _adapter.StartBleDeviceWatcher();

            while (_controller == null)
            {
                await Task.Delay(100);
            }

            await RunCommands();

            await Disconnect();
        }

        static async Task RunCommands()
        {
            await _controller.ExecuteCommandAsync(new HubFirmwareCommand());
            //await controller.ExecuteCommandAsync(new ToggleNotificationsCommand(controller, true, PortType.Motor, "01"));
            Console.WriteLine($"Setting LED Yellow...");
            await _controller.ExecuteCommandAsync(new LEDBoostCommand(LEDColors.Yellow));
            await Task.Delay(500);
            //Console.WriteLine($"Registering for Button notifications...");
            //await controller.ExecuteCommandAsync(new ButtonNotificationsCommand(true));
            await Task.Delay(500);

            Console.WriteLine("Registering for motor notifications");
            await _controller.ExecuteCommandAsync(new ToggleNotificationsCommand(_controller, true, PortType.TrainMotor, "00"));
            await Task.Delay(500);

            Console.WriteLine("Running motor...");
            await _controller.ExecuteCommandAsync(new TrainMotorBoostCommand(_controller, 50, true));
            await Task.Delay(2000);
            Console.WriteLine("Running motor...");
            await _controller.ExecuteCommandAsync(new TrainMotorBoostCommand(_controller, 20, true));
            await Task.Delay(2000);
            Console.WriteLine("Running motor...");
            await _controller.ExecuteCommandAsync(new TrainMotorBoostCommand(_controller, 0, true));
            await Task.Delay(2000);
            //Console.WriteLine("Running motor...");
            //await controller.ExecuteCommandAsync(new MotorBoostCommand(Motors.A, 70, 2000, true, ""));
            //await Task.Delay(500);
            //Console.WriteLine("Running motor...");
            //await controller.ExecuteCommandAsync(new MotorBoostCommand(Motors.A, 90, 2000, false, ""));
            //await Task.Delay(500);

            Console.WriteLine("Registering Event handler to change LED on Button press...");
            _controller.AddEventHandler(new ButtonToLEDEventHandler(_controller));
            await Task.Delay(6000);

            await Task.CompletedTask;
        }

        static async Task Disconnect()
        {
            Console.WriteLine("Disconnecting soon...");
            await Task.Delay(2000);
            await _controller.DisconnectAsync();
            Console.WriteLine("Disconnected");
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
                _adapter.StopBleDeviceWatcher();

                _controller = controller;

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
