using System;

namespace BluetoothController.Responses.Data
{
    // TODO: Base dataType off of the notification mode that was registered for the port
    public class ExternalMotorData : SensorData
    {
        private string _dataType;

        public int Speed { get; set; }

        public ExternalMotorData(string body, string mode) : base(body)
        {
            _dataType = Length;
            if (mode == "01")
            {
                Speed = Convert.ToInt32(body.Substring(8, 2), 16);
            }

            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            if (_dataType == "08")
                return $"External Motor Angle Data: {Body}";
            if (_dataType == "05")
                return $"External Motor ({Port}) Speed Data: {(Speed == 255 ? 0 : Speed)}";
            return $"External Motor Sensor Data: {Body}";
        }
    }
}