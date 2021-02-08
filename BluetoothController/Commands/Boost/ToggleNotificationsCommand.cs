using BluetoothController.Controllers;
using BluetoothController.Util;

namespace BluetoothController.Commands.Boost
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
    //0A-00-41-00-03-01-00-00-00-01
    public class ToggleNotificationsCommand : IBoostCommand
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
                case PortType.Motor:
                    port = controller.PortState.CurrentExternalMotorPort;
                    break;
                case PortType.ColorDistanceSensor:
                    port = controller.PortState.CurrentColorDistanceSensorPort;
                    break;
                case PortType.TrainMotor:
                    port = controller.PortState.CurrentTrainMotorPort;
                    break;
                case PortType.RemoteButtonA:
                    port = "00";
                    break;
                case PortType.RemoteButtonB:
                    port = "01";
                    break;
            }

            var state = enableNotifications ? "01" : "00"; // 01 - On; 00 - Off
            HexCommand = CommandHelper.AddHeader($"41{port}{sensorMode}01000000{state}");
        }
    }
}


