using BluetoothController.Hubs;
using BluetoothController.Models;

namespace BluetoothController.Responses
{
    public static class ResponseProcessor
    {
        const string TILT_SENSOR_PORT = "3a";
        const string VOLTAGE_SENSOR_PORT = "3c";

        public static Response CreateResponse(string notification, Hub hub)
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
                        if (hub is ModularHub modularHub)
                        {
                            if (modularHub.CurrentTrainMotorPort == portInfo.Port)
                                return new TrainMotorState(notification);
                        }
                        return portInfo;
                    }
                    switch (portInfo.DeviceType)
                    {
                        case IOType.LED:
                            return new LEDState(notification);
                        case IOType.ColorDistance:
                            if (hub == null)
                                hub = new ModularHub();
                            ((ModularHub)hub).CurrentColorDistanceSensorPort = portInfo.Port;
                            return new ColorDistanceState(notification);
                        case IOType.ExternalMotor:
                            if (hub == null)
                                hub = new ModularHub();
                            ((ModularHub)hub).CurrentExternalMotorPort = portInfo.Port;
                            return new ExternalMotorState(notification);
                        case IOType.InternalMotor:
                            return new InternalMotorState(notification);
                        case IOType.TrainMotor:
                            if (hub == null)
                                hub = new ModularHub();
                            ((ModularHub)hub).CurrentTrainMotorPort = portInfo.Port;
                            return new TrainMotorState(notification);
                        case IOType.RemoteButton:
                            return new RemoteButtonState(notification);
                    }
                    return portInfo;
                case MessageTypes.Error:
                    return new Error(notification);
                case MessageTypes.PortValueSingle:
                    var sensorData = new SensorData(notification);
                    if (sensorData.Port == ((ModularHub)hub).CurrentColorDistanceSensorPort)
                    {
                        return new ColorDistanceData(notification);
                    }
                    if (sensorData.Port == ((ModularHub)hub).CurrentExternalMotorPort ||
                        sensorData.Port == ((ModularHub)hub).CurrentTrainMotorPort)
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
                    if (sensorData.Port == TILT_SENSOR_PORT)
                    {
                        return new TiltData(notification);
                    }
                    if (sensorData.Port == VOLTAGE_SENSOR_PORT)
                    {
                        return new VoltageData(notification);
                    }
                    if (hub.GetType() == typeof(RemoteHub) && (sensorData.Port == "00" || sensorData.Port == "01"))
                    {
                        return new RemoteButtonData(notification);
                    }
                    return sensorData;
            }
            return response;
        }
    }
}
