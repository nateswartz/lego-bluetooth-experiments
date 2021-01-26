using LegoBoostController.Models;

namespace LegoBoostController.Responses
{
    public class ResponseProcessor
    {
        PortState _state;

        public ResponseProcessor(PortState portState)
        {
            _state = portState;
        }
        const string TILT_SENSOR_PORT = "3a";

        public Response CreateResponse(string notification)
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
                    }
                    return deviceInfo;
                case MessageType.PortInfo:
                    var portInfo = new PortInfo(notification);
                    switch (portInfo.DeviceType)
                    {
                        case DeviceType.LEDState:
                            return new LEDState(notification);
                        case DeviceType.ColorDistanceState:
                            _state.CurrentColorDistanceSensorPort = portInfo.Port;
                            return new ColorDistanceState(notification);
                        case DeviceType.ExternalMotorState:
                            _state.CurrentExternalMotorPort = portInfo.Port;
                            return new ExternalMotorState(notification);
                        case DeviceType.InternalMotorState:
                            return new InternalMotorState(notification);
                    }
                    return portInfo;
                case MessageType.Error:
                    return new Error(notification);
                case MessageType.SensorData:
                    var sensorData = new SensorData(notification);
                    if (sensorData.Port == _state.CurrentColorDistanceSensorPort)
                    {
                        return new ColorDistanceData(notification);
                    }
                    if (sensorData.Port == _state.CurrentExternalMotorPort)
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
                    return sensorData;
            }
            return response;
        }
    }
}
