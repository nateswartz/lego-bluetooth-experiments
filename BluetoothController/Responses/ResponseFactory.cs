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
            if (controller.Hub is HubWithChangeablePorts hub)
            {
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
            }
            return portInfo;
        }

        private static Response HandleIOAttached(HubController controller, PortState portInfo)
        {
            switch (portInfo.DeviceType)
            {
                case IOType.LED:
                    return new LEDState(portInfo.Body);
                case IOType.ColorDistance:
                case IOType.ExternalMotor:
                case IOType.TrainMotor:
                    if (controller.Hub == null)
                        controller.Hub = new HubWithChangeablePorts();
                    var dynamicHub = ((HubWithChangeablePorts)controller.Hub);

                    if (dynamicHub.GetPortByID(portInfo.Port) == null)
                    {
                        dynamicHub.ChangeablePorts.Add(new HubPort
                        {
                            PortID = portInfo.Port,
                            DeviceType = portInfo.DeviceType
                        });
                    }
                    else
                    {
                        dynamicHub.GetPortByID(portInfo.Port).DeviceType = portInfo.DeviceType;
                    }
                    if (portInfo.DeviceType == IOType.ColorDistance)
                        return new ColorDistanceState(portInfo.Body);
                    if (portInfo.DeviceType == IOType.ExternalMotor)
                        return new ExternalMotorState(portInfo.Body);
                    if (portInfo.DeviceType == IOType.TrainMotor)
                        return new TrainMotorState(portInfo.Body);
                    break;
                case IOType.InternalMotor:
                    return new InternalMotorState(portInfo.Body);
                case IOType.RemoteButton:
                    return new RemoteButtonState(portInfo.Body);
                case IOType.VoltageSensor:
                    return new VoltageState(portInfo.Body);
                case IOType.TiltSensor:
                    return new TiltState(portInfo.Body);
            }
            return portInfo;
        }

        private static Response HandlePortValueUpdate(HubController controller, string notification)
        {
            var sensorData = new SensorData(notification);
            if (controller.Hub is HubWithChangeablePorts hub)
            {
                if (hub.GetPortByID(sensorData.Port)?.DeviceType == IOType.ColorDistance)
                {
                    return new ColorDistanceData(notification);
                }
                if (hub.GetPortByID(sensorData.Port)?.DeviceType == IOType.ExternalMotor
                    || hub.GetPortByID(sensorData.Port)?.DeviceType == IOType.TrainMotor)
                {
                    var externalMotorData = new ExternalMotorData(notification);
                    switch (externalMotorData.DataType)
                    {
                        case MotorDataType.Angle:
                            return new AngleData(notification);
                        case MotorDataType.Speed:
                            return new SpeedData(notification);
                    }
                    return new ExternalMotorData(notification);
                }
            }
            if (sensorData.Port == IOType.TiltSensor)
            {
                return new TiltData(notification);
            }
            if (sensorData.Port == IOType.VoltageSensor)
            {
                return new VoltageData(notification);
            }
            if (controller.Hub.GetType() == typeof(RemoteHub) && (sensorData.Port == "00" || sensorData.Port == "01"))
            {
                return new RemoteButtonData(notification);
            }
            return sensorData;
        }

        private static string GetMessageType(string notification)
        {
            return notification.Substring(4, 2);
        }
    }
}
