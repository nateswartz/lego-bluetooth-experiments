using BluetoothController;
using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BluetoothLibraryTester
{
    class Program
    {
        static BluetoothLowEnergyAdapter _adapter;

        static HubController _remoteController;
        static HubController _hubController;
        static HubController _boostController;

        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += delegate
            {
                Disconnect().GetAwaiter().GetResult();
                Environment.Exit(0);
            };

            _adapter = new BluetoothLowEnergyAdapter(HandleDiscover, HandleConnect, HandleNotification);
            Console.WriteLine("Searching for devices...");
            _adapter.StartBleDeviceWatcher();

            while (_hubController == null)
            //while (_remoteController == null || _hubController == null)
            {
                await Task.Delay(100);
            }

            await GetNames();
            Console.WriteLine("Shutting down");
            await Task.Delay(1000);
            await _hubController.ExecuteCommandAsync(new ShutdownCommand());
            await Task.Delay(4000);
            //await RunMotorCommand(_boostController);

            await Disconnect();
        }

        static async Task GetNames()
        {
            if (_boostController != null)
                await _boostController.ExecuteCommandAsync(new HubNameCommand());
            if (_remoteController != null)
                await _remoteController.ExecuteCommandAsync(new HubNameCommand());
            if (_hubController != null)
                await _hubController.ExecuteCommandAsync(new HubNameCommand());
            await Task.Delay(2000);
        }

        static async Task RunMotorCommand(HubController controller)
        {
            await controller.ExecuteCommandAsync(new HubFirmwareCommand());
            await Task.Delay(1000);
            var motor = controller.GetPortIdsByDeviceType(IOType.ExternalMotor).Single();
            await controller.ExecuteCommandAsync(new ToggleNotificationsCommand(motor, true, "02"));
            await Task.Delay(1000);
            await controller.ExecuteCommandAsync(new MotorCommand(motor, 50, 1000, true));
            await Task.Delay(1000);
        }

        static async Task RunInternalMotorCommand()
        {
            await Task.Delay(1000);
            var port = _boostController.GetPortIdsByDeviceType(IOType.ColorDistance).Last();
            await _boostController.ExecuteCommandAsync(new ToggleNotificationsCommand(port, true, "08"));
            await Task.Delay(10000);
            //await _boostController.ExecuteCommandAsync(new MotorCommand(motor, 50, 2000, true));
            await Task.Delay(2000);

        }

        static async Task RunCommands()
        {
            await _remoteController.ExecuteCommandAsync(new HubFirmwareCommand());
            //await _remoteController.ExecuteCommandAsync(new RawCommand("0203"));
            //await controller.ExecuteCommandAsync(new ToggleNotificationsCommand(controller, true, PortType.Motor, "01"));
            Console.WriteLine($"Setting LED Pink...");
            await _remoteController.ExecuteCommandAsync(new LEDCommand(_remoteController, LEDColors.Pink));
            //await Task.Delay(500);
            //Console.WriteLine($"Registering for Button notifications...");
            //await controller.ExecuteCommandAsync(new ButtonNotificationsCommand(true));
            //_remoteController.AddEventHandler(new RemoteButtonToLEDEventHandler(_hubController));
            await Task.Delay(500);

            Console.WriteLine("Registering for remote button notifications");
            var remoteButtons = _remoteController.GetPortIdsByDeviceType(IOType.RemoteButton);
            foreach (var button in remoteButtons)
                await _remoteController.ExecuteCommandAsync(new ToggleNotificationsCommand(button, true, "03"));
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
            await Task.Delay(1000);
            if (_boostController != null)
                await _boostController.ExecuteCommandAsync(new ShutdownCommand());
            if (_remoteController != null)
                await _remoteController.ExecuteCommandAsync(new ShutdownCommand());
            if (_hubController != null)
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

                if (controller.HubType == HubType.BoostMoveHub)
                {
                    _boostController = controller;
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
