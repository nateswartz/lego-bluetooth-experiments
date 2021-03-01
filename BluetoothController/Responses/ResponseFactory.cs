using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Models;
using BluetoothController.Responses.Device.Data;
using BluetoothController.Responses.Device.Info;
using BluetoothController.Responses.Device.State;
using BluetoothController.Responses.Hub;
using System.Linq;

namespace BluetoothController.Responses
{
    public static class ResponseFactory
    {
        public static Response CreateResponse(string notification, HubController controller)
        {
            var messageType = MessageTypes.GetByCode(notification.Substring(4, 2));

            if (messageType == MessageTypes.HubProperty)
                return HandleHubProperty(controller, notification);
            if (messageType == MessageTypes.HubAction)
                return new HubActionResponse(notification);
            if (messageType == MessageTypes.Error)
                return new Error(notification);
            if (messageType == MessageTypes.HubAttachedDetachedIO)
                return HandleIOConnectionStateChange(controller, notification);
            if (messageType == MessageTypes.PortInformation)
                return new PortInfo(notification);
            if (messageType == MessageTypes.PortModeInformation)
                return new PortModeInfo(notification);
            if (messageType == MessageTypes.PortValueSingle)
                return HandlePortValueUpdate(controller, notification);
            if (messageType == MessageTypes.PortInputFormatSingle)
                return HandleNotificationStateUpdate(controller, notification);
            if (messageType == MessageTypes.PortOutputFeedback)
                return new PortOutputFeedback(notification);

            return new Response(notification);
        }

        private static Response HandleNotificationStateUpdate(HubController controller, string notification)
        {
            var portState = new PortNotificationState(notification);
            controller.Hub.GetPortByID(portState.Port).NotificationMode = portState.Mode;
            return portState;
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

            if (portInfo.StateChangeEvent == DeviceState.Detached)
            {
                return HandleIODetached(controller, portInfo);
            }
            return HandleIOAttached(controller, portInfo);
        }

        private static Response HandleIODetached(HubController controller, PortState portInfo)
        {
            var hub = controller.Hub;
            if (hub.GetPortsByDeviceType(IOTypes.TrainMotor).Any(p => p.PortID == portInfo.Port))
            {
                hub.GetPortByID(portInfo.Port).DeviceType = IOTypes.None;
                return new TrainMotorState(portInfo.Body);
            }
            if (hub.GetPortsByDeviceType(IOTypes.ExternalMotor).Any(p => p.PortID == portInfo.Port))
            {
                hub.GetPortByID(portInfo.Port).DeviceType = IOTypes.None;
                return new ExternalMotorState(portInfo.Body);
            }
            if (hub.GetPortsByDeviceType(IOTypes.ColorDistance).Any(p => p.PortID == portInfo.Port))
            {
                hub.GetPortByID(portInfo.Port).DeviceType = IOTypes.None;
                return new ColorDistanceState(portInfo.Body);
            }
            return portInfo;
        }

        private static Response HandleIOAttached(HubController controller, PortState portInfo)
        {
            if (controller.Hub == null)
                controller.Hub = new LegoHub();
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

            if (portInfo.DeviceType == IOTypes.LED)
                return new LEDState(portInfo.Body);
            if (portInfo.DeviceType == IOTypes.ColorDistance)
                return new ColorDistanceState(portInfo.Body);
            if (portInfo.DeviceType == IOTypes.ExternalMotor)
                return new ExternalMotorState(portInfo.Body);
            if (portInfo.DeviceType == IOTypes.TrainMotor)
                return new TrainMotorState(portInfo.Body);
            if (portInfo.DeviceType == IOTypes.TiltSensor)
                return new TiltState(portInfo.Body);
            if (portInfo.DeviceType == IOTypes.RemoteButton)
                return new RemoteButtonState(portInfo.Body);
            if (portInfo.DeviceType == IOTypes.VoltageSensor)
                return new VoltageState(portInfo.Body);
            if (portInfo.DeviceType == IOTypes.InternalMotor)
                return new InternalMotorState(portInfo.Body);
            if (portInfo.DeviceType == IOTypes.CurrentSensor)
                return new CurrentState(portInfo.Body);

            return portInfo;
        }

        private static Response HandlePortValueUpdate(HubController controller, string notification)
        {
            var sensorData = new SensorData(notification);
            var hub = controller.Hub;
            var port = hub.GetPortByID(sensorData.Port);

            if (port?.DeviceType == IOTypes.ColorDistance)
                return new ColorDistanceData(notification, port.NotificationMode);
            if (port?.DeviceType == IOTypes.ExternalMotor)
                return new ExternalMotorData(notification, port.NotificationMode);
            if (port?.DeviceType == IOTypes.TrainMotor)
                return new TrainMotorData(notification);
            if (port?.DeviceType == IOTypes.TiltSensor)
                return new TiltData(notification);
            if (port?.DeviceType == IOTypes.RemoteButton)
                return new RemoteButtonData(notification);
            if (port?.DeviceType == IOTypes.VoltageSensor)
                return new VoltageData(notification);

            return sensorData;
        }
    }
}
