using BluetoothController.Commands.Abstract;
using BluetoothController.Controllers;
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
            var hub = controller.Hub;
            string port = "00";
            switch (portType)
            {
                case NotificationDeviceType.Tilt:
                    if (hub.GetPortsByDeviceType((IOType.TiltSensor)).Any())
                        port = controller.Hub.GetPortsByDeviceType(IOType.TiltSensor).First().PortID;
                    else
                        return;
                    break;
                case NotificationDeviceType.Motor:
                    if (hub.GetPortsByDeviceType((IOType.ExternalMotor)).Any())
                        port = controller.Hub.GetPortsByDeviceType(IOType.ExternalMotor).First().PortID;
                    else
                        return;
                    break;
                case NotificationDeviceType.ColorDistanceSensor:
                    if (hub.GetPortsByDeviceType((IOType.ColorDistance)).Any())
                        port = hub.GetPortsByDeviceType(IOType.ColorDistance).First().PortID;
                    else
                        return;
                    break;
                case NotificationDeviceType.TrainMotor:
                    if (hub.GetPortsByDeviceType((IOType.TrainMotor)).Any())
                        port = hub.GetPortsByDeviceType(IOType.TrainMotor).First().PortID;
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


