using BluetoothController.Commands.Abstract;
using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Responses.State;
using System.Linq;

namespace BluetoothController.Commands.Basic
{
    public enum NotificationDeviceType
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

        public ToggleNotificationsCommand(HubController controller, bool enableNotifications, NotificationDeviceType portType, string sensorMode)
        {
            string port = "00";
            switch (portType)
            {
                case NotificationDeviceType.Tilt:
                    port = "3a";
                    break;
                case NotificationDeviceType.Motor:
                    if (controller.Hub is HubWithChangeablePorts motorHub)
                    {
                        port = motorHub.GetPortsByDeviceType(IOType.ExternalMotor).First().PortID;
                    }
                    else
                        return;
                    break;
                case NotificationDeviceType.ColorDistanceSensor:
                    if (controller.Hub is HubWithChangeablePorts colorHub)
                    {
                        port = colorHub.GetPortsByDeviceType(IOType.ColorDistance).First().PortID;
                    }
                    else
                        return;
                    break;
                case NotificationDeviceType.TrainMotor:
                    if (controller.Hub is HubWithChangeablePorts trainHub)
                    {
                        port = trainHub.GetPortsByDeviceType(IOType.TrainMotor).First().PortID;
                    }
                    else
                        return;
                    break;
                case NotificationDeviceType.RemoteButtonA:
                    port = "00";
                    break;
                case NotificationDeviceType.RemoteButtonB:
                    port = "01";
                    break;
            }

            var state = enableNotifications ? "01" : "00"; // 01 - On; 00 - Off
            var deltaInterval = "01000000"; // value must change by this interval to trigger a message, protects against jitter
            HexCommand = AddHeader($"{port}{sensorMode}{deltaInterval}{state}");
        }
    }
}


