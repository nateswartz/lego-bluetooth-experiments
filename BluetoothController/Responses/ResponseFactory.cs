using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Models;
using BluetoothController.Responses.Data;
using BluetoothController.Responses.State;
using System.Linq;

namespace BluetoothController.Responses
{
    public static class ResponseFactory
    {
        public static Response CreateResponse(string notification, HubController controller)
        {
            var messageType = GetMessageType(notification);

            switch (messageType)
            {
                case MessageTypes.HubProperty:
                    return HandleHubProperty(controller, notification);
                case MessageTypes.Error:
                    return new Error(notification);
                case MessageTypes.HubAttachedDetachedIO:
                    return HandleIOConnectionStateChange(controller, notification);
                case MessageTypes.PortValueSingle:
                    return HandlePortValueUpdate(controller, notification);
                case MessageTypes.PortInputFormatSingle:
                    return new PortNotificationState(notification);
            }
            return new Response(notification);
        }

        private static Response HandleHubProperty(HubController controller, string notification)
        {
            var deviceInfo = new HubInfo(notification);
            switch (deviceInfo.DeviceType)
            {
                case HubInfoType.HubName:
                    return new HubName(notification);
                case HubInfoType.ButtonState:
                    return new ButtonStateMessage(notification);
                case HubInfoType.FirmwareVersion:
                    return new FirmwareVersion(notification);
                case HubInfoType.SystemType:
                    return new SystemType(notification);
            }
            return deviceInfo;
        }

        private static Response HandleIOConnectionStateChange(HubController controller, string notification)
        {
            var portInfo = new PortState(notification);

            if (portInfo.Event == DeviceState.Detached)
            {
                return HandleIODetached(controller, portInfo);
            }
            return HandleIOAttached(controller, portInfo);
        }

        private static Response HandleIODetached(HubController controller, PortState portInfo)
        {
            var hub = controller.Hub;
            if (hub.GetPortsByDeviceType(IOType.TrainMotor).Any(p => p.PortID == portInfo.Port))
            {
                hub.GetPortByID(portInfo.Port).DeviceType = "";
                return new TrainMotorState(portInfo.Body);
            }
            if (hub.GetPortsByDeviceType(IOType.ExternalMotor).Any(p => p.PortID == portInfo.Port))
            {
                hub.GetPortByID(portInfo.Port).DeviceType = "";
                return new ExternalMotorState(portInfo.Body);
            }
            if (hub.GetPortsByDeviceType(IOType.ColorDistance).Any(p => p.PortID == portInfo.Port))
            {
                hub.GetPortByID(portInfo.Port).DeviceType = "";
                return new ColorDistanceState(portInfo.Body);
            }
            return portInfo;
        }

        private static Response HandleIOAttached(HubController controller, PortState portInfo)
        {
            if (controller.Hub == null)
                controller.Hub = new Hub();
            var hub = controller.Hub;

            if (hub.GetPortByID(portInfo.Port) == null)
            {
                hub.Ports.Add(new HubPort
                {
                    PortID = portInfo.Port,
                    DeviceType = portInfo.DeviceType
                });
            }
            else
            {
                hub.GetPortByID(portInfo.Port).DeviceType = portInfo.DeviceType;
            }

            switch (portInfo.DeviceType)
            {
                case IOType.LED:
                    return new LEDState(portInfo.Body);
                case IOType.ColorDistance:
                    return new ColorDistanceState(portInfo.Body);
                case IOType.ExternalMotor:
                    return new ExternalMotorState(portInfo.Body);
                case IOType.TrainMotor:
                    return new TrainMotorState(portInfo.Body);
                case IOType.TiltSensor:
                    return new TiltState(portInfo.Body);
                case IOType.RemoteButton:
                    return new RemoteButtonState(portInfo.Body);
                case IOType.VoltageSensor:
                    return new VoltageState(portInfo.Body);
                case IOType.InternalMotor:
                    return new InternalMotorState(portInfo.Body);
                case IOType.CurrentSensor:
                    return new CurrentState(portInfo.Body);
            }
            return portInfo;
        }

        private static Response HandlePortValueUpdate(HubController controller, string notification)
        {
            var sensorData = new SensorData(notification);
            var hub = controller.Hub;

            switch (hub.GetPortByID(sensorData.Port)?.DeviceType)
            {
                case IOType.ColorDistance:
                    return new ColorDistanceData(notification);
                case IOType.ExternalMotor:
                case IOType.TrainMotor:
                    return new ExternalMotorData(notification);
                case IOType.TiltSensor:
                    return new TiltData(notification);
                case IOType.RemoteButton:
                    return new RemoteButtonData(notification);
                case IOType.VoltageSensor:
                    return new VoltageData(notification);
            }

            return sensorData;
        }

        private static string GetMessageType(string notification)
        {
            return notification.Substring(4, 2);
        }
    }
}
