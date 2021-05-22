using BluetoothController;
using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BluetoothLibraryTester
{
    class Program
    {
        static IBluetoothLowEnergyAdapter _adapter;

        static readonly List<IHubController> _controllers = new();

        static async Task Main(string[] _)
        {
            try
            {
                Console.CancelKeyPress += delegate
                {
                    Disconnect().GetAwaiter().GetResult();
                    Environment.Exit(0);
                };

                var eventHandler = new EventHandler(_controllers);
                _adapter = new BluetoothLowEnergyAdapter(eventHandler);
                Console.WriteLine("Searching for devices...");
                _adapter.StartBleDeviceWatcher();

                while (!_controllers.Any())
                {
                    await Task.Delay(100);
                }

                //await GetInfo();
                Console.WriteLine("Running test method...");
                await RunMotor(_controllers.First());
                //await GetPortInfoForIOType(_controllers.First(), IOTypes.SmallAngularMotor);
                //await RegisterForNotifications(_controllers.First());
                await Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Encountered exception: {e.Message}");
                await Disconnect();
            }
        }

        static async Task RunMotor(IHubController controller)
        {
            var port = controller.GetPortIdsByDeviceType(IOTypes.SmallAngularMotor).First();
            await controller.ExecuteCommandAsync(new MotorCommand(port, 100, 2000, true));
            await Task.Delay(3000);
        }

        static async Task RegisterForNotifications(IHubController controller)
        {
            await Task.Delay(3000);
            var port = controller.GetPortIdsByDeviceType(IOTypes.ColorSensor).First();
            await controller.ExecuteCommandAsync(new ToggleNotificationsCommand(port, true, "00"));
            await Task.Delay(30000);
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

        static async Task GetPortInfoForIOType(IHubController controller, IOType ioType)
        {
            var ports = controller.GetPortIdsByDeviceType(ioType);

            if (!ports.Any())
            {
                Console.WriteLine($"No IO Device Found");
                return;
            }

            var port = ports.First();

            await controller.ExecuteCommandAsync(new PortInfoCommand(port, InfoType.PossibleModeCombinations));
            await Task.Delay(1000);
            await controller.ExecuteCommandAsync(new PortInfoCommand(port, InfoType.ModeInfo));
            await Task.Delay(1000);
            await controller.ExecuteCommandAsync(new PortInfoModeCommand(port, "00", ModeInfoType.Name));
            await Task.Delay(1000);
            await controller.ExecuteCommandAsync(new PortInfoModeCommand(port, "00", ModeInfoType.Raw));
            await Task.Delay(1000);
            await controller.ExecuteCommandAsync(new PortInfoModeCommand(port, "00", ModeInfoType.Percent));
            await Task.Delay(1000);
            await controller.ExecuteCommandAsync(new PortInfoModeCommand(port, "00", ModeInfoType.Si));
            await Task.Delay(1000);
            await controller.ExecuteCommandAsync(new PortInfoModeCommand(port, "00", ModeInfoType.Symbol));
            await Task.Delay(1000);
            await controller.ExecuteCommandAsync(new PortInfoModeCommand(port, "00", ModeInfoType.Mapping));
            await Task.Delay(1000);
            await controller.ExecuteCommandAsync(new PortInfoModeCommand(port, "00", ModeInfoType.MotorBias));
            await Task.Delay(1000);
            await controller.ExecuteCommandAsync(new PortInfoModeCommand(port, "00", ModeInfoType.CapabilityBits));
            await Task.Delay(1000);
            await controller.ExecuteCommandAsync(new PortInfoModeCommand(port, "00", ModeInfoType.ValueFormat));
        }

        static async Task Disconnect()
        {
            Console.WriteLine("Disconnecting soon...");
            await Task.Delay(1000);
            foreach (var controller in _controllers)
                await controller.ExecuteCommandAsync(new ShutdownCommand());
            Console.WriteLine("Disconnected");
        }
    }
}
