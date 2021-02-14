using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Models;
using BluetoothController.Responses.Data;
using BluetoothController.Responses.State;
using System.Linq;

namespace BluetoothController.Responses
{
    public static class ResponseProcessor
    {
        public static Response CreateResponse(string notification, HubController controller)
        {
            var response = new Response(notification);

            // Simple cases - hub agnostic
            switch (response.MessageType)
            {
                case MessageTypes.HubProperty:
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
                case MessageTypes.Error:
                    return new Error(notification);
            }

            // Complex cases - hub dependent
            switch (response.MessageType)
            {
                case MessageTypes.HubAttachedIO:
                    return HandleIOAttached(controller, notification);
                case MessageTypes.PortValueSingle:
                    return HandlePortValueUpdate(controller, notification);
            }
            return response;
        }

        private static Response HandleIOAttached(HubController controller, string notification)
        {
            var portInfo = new PortState(notification);

            if (portInfo.Event == DeviceState.Detached)
            {
                if (controller.Hub is HubWithChangeablePorts hub)
                {
                    if (hub.GetPortsByDeviceType(IOType.TrainMotor).Any(p => p.PortID == portInfo.Port))
                    {
                        hub.GetPortByID(portInfo.Port).DeviceType = "";
                        return new TrainMotorState(notification);
                    }
                    if (hub.GetPortsByDeviceType(IOType.ExternalMotor).Any(p => p.PortID == portInfo.Port))
                    {
                        hub.GetPortByID(portInfo.Port).DeviceType = "";
                        return new ExternalMotorState(notification);
                    }
                    if (hub.GetPortsByDeviceType(IOType.ColorDistance).Any(p => p.PortID == portInfo.Port))
                    {
                        hub.GetPortByID(portInfo.Port).DeviceType = "";
                        return new ColorDistanceState(notification);
                    }
                }
                return portInfo;
            }

            switch (portInfo.DeviceType)
            {
                case IOType.LED:
                    return new LEDState(notification);
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
                        return new ColorDistanceState(notification);
                    if (portInfo.DeviceType == IOType.ExternalMotor)
                        return new ExternalMotorState(notification);
                    if (portInfo.DeviceType == IOType.TrainMotor)
                        return new TrainMotorState(notification);
                    break;
                case IOType.InternalMotor:
                    return new InternalMotorState(notification);
                case IOType.RemoteButton:
                    return new RemoteButtonState(notification);
            }
            switch (portInfo.Port)
            {
                case PortType.VoltageSensor:
                    return new VoltageState(notification);
                case PortType.TiltSensor:
                    return new TiltState(notification);
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
    }
}
