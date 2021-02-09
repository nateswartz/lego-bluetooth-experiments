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

        static HubController _remoteController;
        static HubController _hubController;

        static async Task Main(string[] args)
        {
            _adapter = new BluetoothLowEnergyAdapter(HandleDiscover, HandleConnect, HandleNotification);
            Console.WriteLine("Searching for devices...");
            _adapter.StartBleDeviceWatcher();

            while (_remoteController == null || _hubController == null)
            {
                await Task.Delay(100);
            }

            await GetNames();
            await RunCommands();

            await Disconnect();
        }

        static async Task GetNames()
        {
            await _remoteController.ExecuteCommandAsync(new HubNameCommand());
            await _hubController.ExecuteCommandAsync(new HubNameCommand());

            await Task.Delay(4000);
        }

        static async Task RunCommands()
        {
            await _remoteController.ExecuteCommandAsync(new HubFirmwareCommand());
            await _remoteController.ExecuteCommandAsync(new RawCommand("0203"));
            //await controller.ExecuteCommandAsync(new ToggleNotificationsCommand(controller, true, PortType.Motor, "01"));
            //Console.WriteLine($"Setting LED Pink...");
            //await _controller.ExecuteCommandAsync(new LEDBoostCommand(_controller, LEDColors.Pink));
            //await Task.Delay(500);
            //Console.WriteLine($"Registering for Button notifications...");
            //await controller.ExecuteCommandAsync(new ButtonNotificationsCommand(true));
            _remoteController.AddEventHandler(new RemoteButtonToLEDEventHandler(_hubController));
            await Task.Delay(500);

            Console.WriteLine("Registering for remote button notifications");
            await _remoteController.ExecuteCommandAsync(new ToggleNotificationsCommand(_remoteController, true, PortType.RemoteButtonA, "03"));
            await _remoteController.ExecuteCommandAsync(new ToggleNotificationsCommand(_remoteController, true, PortType.RemoteButtonB, "03"));
            await Task.Delay(500);

            //Console.WriteLine("Running motor...");
            //await _controller.ExecuteCommandAsync(new TrainMotorBoostCommand(_controller, 50, true));
            //await Task.Delay(2000);
            //Console.WriteLine("Running motor...");
            //await _controller.ExecuteCommandAsync(new TrainMotorBoostCommand(_controller, 20, true));
            //await Task.Delay(2000);
            //Console.WriteLine("Running motor...");
            //await _controller.ExecuteCommandAsync(new TrainMotorBoostCommand(_controller, 0, true));
            //await Task.Delay(2000);
            //Console.WriteLine("Running motor...");
            //await controller.ExecuteCommandAsync(new MotorBoostCommand(Motors.A, 70, 2000, true, ""));
            //await Task.Delay(500);
            //Console.WriteLine("Running motor...");
            //await controller.ExecuteCommandAsync(new MotorBoostCommand(Motors.A, 90, 2000, false, ""));
            //await Task.Delay(500);

            //Console.WriteLine("Registering Event handler to change LED on Button press...");
            //_controller.AddEventHandler(new ButtonToLEDEventHandler(_controller));
            await Task.Delay(10000);

            await Task.CompletedTask;
        }

        static async Task Disconnect()
        {
            Console.WriteLine("Disconnecting soon...");
            await Task.Delay(2000);
            await _remoteController.ExecuteCommandAsync(new ShutdownCommand());
            await _hubController.ExecuteCommandAsync(new ShutdownCommand());
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
                //_adapter.StopBleDeviceWatcher();

                if (controller.HubType == HubType.TwoPortHandset)
                {
                    _remoteController = controller;
                }

                if (controller.HubType == HubType.TwoPortHub)
                {
                    _hubController = controller;
                }

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
