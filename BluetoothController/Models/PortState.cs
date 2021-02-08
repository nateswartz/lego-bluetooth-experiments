namespace BluetoothController.Models
{
    public class PortState
    {
        public string CurrentColorDistanceSensorPort { get; set; } = "";
        public string CurrentExternalMotorPort { get; set; } = "";
        public string CurrentTrainMotorPort { get; set; } = "";
        public bool IsTwoHubRemote { get; set; } = false;
    }
}
