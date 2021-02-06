using BluetoothController.Controllers;

namespace BluetoothController.Commands.Boost
{
    public enum PortType
    {
        Motor = 0,
        ColorDistanceSensor = 1,
        Tilt = 2,
        TrainMotor = 3
    }

    public class ToggleNotificationsCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        // TODO: Better parameterize this
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
                // TODO: Get TrainMotor case working
                case PortType.TrainMotor:
                    port = controller.PortState.CurrentTrainMotorPort;
                    break;
            }

            var messageLength = "0a";
            var state = enableNotifications ? "01" : "00"; // 01 - On; 00 - Off
            HexCommand = $"{messageLength}0041{port}{sensorMode}01000000{state}";
        }
    }
}


