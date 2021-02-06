using BluetoothController.Models;

namespace BluetoothController.Responses
{
    public static class ResponseProcessor
    {
        const string TILT_SENSOR_PORT = "3a";
        const string VOLTAGE_SENSOR_PORT = "3c";

        public static Response CreateResponse(string notification, PortState portState)
        {
            var response = new Response(notification);
            switch (response.MessageType)
            {
                case MessageType.DeviceInfo:
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
                case MessageType.PortInfo:
                    var portInfo = new PortInfo(notification);
                    switch (portInfo.DeviceType)
                    {
                        case IOType.LEDState:
                            return new LEDState(notification);
                        case IOType.ColorDistanceState:
                            portState.CurrentColorDistanceSensorPort = portInfo.Port;
                            return new ColorDistanceState(notification);
                        case IOType.ExternalMotorState:
                            portState.CurrentExternalMotorPort = portInfo.Port;
                            return new ExternalMotorState(notification);
                        case IOType.InternalMotorState:
                            return new InternalMotorState(notification);
                        case IOType.TrainMotor:
                            portState.CurrentTrainMotorPort = portInfo.Port;
                            return new TrainMotorState(notification);
                    }
                    return portInfo;
                case MessageType.Error:
                    return new Error(notification);
                case MessageType.SensorData:
                    var sensorData = new SensorData(notification);
                    if (sensorData.Port == portState.CurrentColorDistanceSensorPort)
                    {
                        return new ColorDistanceData(notification);
                    }
                    if (sensorData.Port == portState.CurrentExternalMotorPort ||
                        sensorData.Port == portState.CurrentTrainMotorPort)
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
                    return sensorData;
            }
            return response;
        }
    }
}
