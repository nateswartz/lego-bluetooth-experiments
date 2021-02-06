using BluetoothController.Controllers;

namespace BluetoothController.Commands.Boost
{
    public class TrainMotorBoostCommand : IBoostCommand
    {
        // TODO: Better parameterize
        //08-00-81-00-11-51-00-02
        public string HexCommand { get; set; }

        public TrainMotorBoostCommand(HubController controller)
        {
            string motorToRun = controller.PortState.CurrentTrainMotorPort;

            HexCommand = $"080081{motorToRun}11510009";
        }
    }
}


