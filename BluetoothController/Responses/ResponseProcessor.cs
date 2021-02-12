using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Models;
using System.Linq;

namespace BluetoothController.Responses
{
    public static class ResponseProcessor
    {
        const string TILT_SENSOR_PORT = "3a";
        const string VOLTAGE_SENSOR_PORT = "3c";

        public static Response CreateResponse(string notification, HubController controller)
        {
            var response = new Response(notification);
            switch (response.MessageType)
            {
                case MessageTypes.HubProperty:
                    var deviceInfo = new DeviceInfo(notification);
                    switch (deviceInfo.DeviceType)
                    {
                        case DeviceType.HubName:
                            return new HubName(notification);
                        case DeviceType.ButtonState:
                            return new ButtonStateMessage(notification);
                        case DeviceType.FirmwareVersion:
                            return new FirmwareVersion(notification);
                        case DeviceType.SystemType:
                            return new SystemType(notification);
                    }
                    return deviceInfo;
                case MessageTypes.HubAttachedIO:
                    var portInfo = new PortInfo(notification);
                    if (portInfo.Event == DeviceState.Detached)
                    {
                        if (controller.Hub is HubWithChangeablePorts hub)
                        {
                            if (hub.GetPortsByDeviceType(IOType.TrainMotor).Any(p => p.PortID == portInfo.Port))
                            {
                                hub.GetPortByID(portInfo.Port).DeviceType = "";
                                return new TrainMotorState(notification);
                            }
                            if (hub.GetPortsByDeviceType(IOType.ColorDistance).Any(p => p.PortID == portInfo.Port))
                            {
                                hub.GetPortByID(portInfo.Port).DeviceType = "";
                                return new ColorDistanceState(notification);
                            }
                        }
                        return portInfo;
                    }
                    if (controller.Hub == null)
                        controller.Hub = new HubWithChangeablePorts();
                    var dynamicHub = ((HubWithChangeablePorts)controller.Hub);
                    switch (portInfo.DeviceType)
                    {
                        //case IOType.LED:
                        //    return new LEDState(notification);
                        case IOType.ColorDistance:
                            if (dynamicHub.GetPortByID(portInfo.Port) == null)
                            {
                                dynamicHub.ChangeablePorts.Add(new HubPort
                                {
                                    PortID = portInfo.Port,
                                    DeviceType = IOType.ColorDistance
                                });
                            }
                            else
                            {
                                dynamicHub.GetPortByID(portInfo.Port).DeviceType = IOType.ColorDistance;
                            }
                            return new ColorDistanceState(notification);
                        //case IOType.ExternalMotor:
                        //    if (hub == null)
                        //        hub = new ModularHub();
                        //    ((ModularHub)hub).CurrentExternalMotorPort = portInfo.Port;
                        //    return new ExternalMotorState(notification);
                        //case IOType.InternalMotor:
                        //    return new InternalMotorState(notification);
                        case IOType.TrainMotor:
                            if (dynamicHub.GetPortByID(portInfo.Port) == null)
                            {
                                dynamicHub.ChangeablePorts.Add(new HubPort
                                {
                                    PortID = portInfo.Port,
                                    DeviceType = IOType.TrainMotor
                                });
                            }
                            else
                            {
                                dynamicHub.GetPortByID(portInfo.Port).DeviceType = IOType.TrainMotor;
                            }
                            return new TrainMotorState(notification);
                        case IOType.RemoteButton:
                            return new RemoteButtonState(notification);
                    }
                    return portInfo;
                case MessageTypes.Error:
                    return new Error(notification);
                case MessageTypes.PortValueSingle:
                    var sensorData = new SensorData(notification);
                    if (((HubWithChangeablePorts)controller.Hub).GetPortByID(sensorData.Port).DeviceType == IOType.ColorDistance)
                    {
                        return new ColorDistanceData(notification);
                    }
                    //if (sensorData.Port == ((ModularHub)hub).CurrentExternalMotorPort ||
                    //    sensorData.Port == ((ModularHub)hub).CurrentTrainMotorPort)
                    //{
                    //    var externalMotorData = new ExternalMotorData(notification);
                    //    switch (externalMotorData.DataType)
                    //    {
                    //        case MotorDataType.Angle:
                    //            return new AngleData(notification);
                    //        case MotorDataType.Speed:
                    //            return new SpeedData(notification);
                    //    }
                    //    return new ExternalMotorData(notification);
                    //}
                    if (sensorData.Port == TILT_SENSOR_PORT)
                    {
                        return new TiltData(notification);
                    }
                    if (sensorData.Port == VOLTAGE_SENSOR_PORT)
                    {
                        return new VoltageData(notification);
                    }
                    if (controller.Hub.GetType() == typeof(RemoteHub) && (sensorData.Port == "00" || sensorData.Port == "01"))
                    {
                        return new RemoteButtonData(notification);
                    }
                    return sensorData;
            }
            return response;
        }
    }
}
