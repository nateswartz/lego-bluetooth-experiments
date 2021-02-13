using BluetoothController.Commands.Abstract;
using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Responses.State;
using System.Linq;

namespace BluetoothController.Commands.Basic
{
    public enum PortType
    {
        Motor = 0,
        ColorDistanceSensor = 1,
        Tilt = 2,
        TrainMotor = 3,
        RemoteButtonA = 4,
        RemoteButtonB = 5
    }

    public class ToggleNotificationsCommand : PortInputFormatSetupSingleCommandType, IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        public ToggleNotificationsCommand(HubController controller, bool enableNotifications, PortType portType, string sensorMode)
        {
            string port = "00";
            switch (portType)
            {
                case PortType.Tilt:
                    port = "3a";
                    break;
                // TODO: Re-enable
                //case PortType.Motor:
                //    port = ((ModularHub)controller.Hub).CurrentExternalMotorPort;
                //    break;
                //case PortType.ColorDistanceSensor:
                //    port = ((ModularHub)controller.Hub).CurrentColorDistanceSensorPort;
                //    break;
                case PortType.TrainMotor:
                    if (controller.Hub is HubWithChangeablePorts dynamicHub)
                    {
                        port = dynamicHub.GetPortsByDeviceType(IOType.TrainMotor).First().PortID;
                    }
                    else
                        return;
                    break;
                case PortType.RemoteButtonA:
                    port = "00";
                    break;
                case PortType.RemoteButtonB:
                    port = "01";
                    break;
            }

            var state = enableNotifications ? "01" : "00"; // 01 - On; 00 - Off
            HexCommand = AddHeader($"{port}{sensorMode}01000000{state}");
        }
    }
}


