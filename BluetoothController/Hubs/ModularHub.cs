namespace BluetoothController.Hubs
{
    public class ModularHub : Hub
    {
        public string CurrentColorDistanceSensorPort { get; set; } = "";
        public string CurrentExternalMotorPort { get; set; } = "";
        public string CurrentTrainMotorPort { get; set; } = "";
    }
}
