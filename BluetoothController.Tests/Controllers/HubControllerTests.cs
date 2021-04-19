using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Wrappers;
using Moq;
using Xunit;

namespace BluetoothController.Tests.Controllers
{
    public class HubControllerTests
    {
        private HubController _hubController;
        private readonly Mock<IGattCharacteristicWrapper> _mockGattCharacteristicWrapper;
        private readonly Mock<ILegoHub> _mockLegoHub;

        public HubControllerTests()
        {
            _mockGattCharacteristicWrapper = new Mock<IGattCharacteristicWrapper>();
            _mockLegoHub = new Mock<ILegoHub>();
            _hubController = new HubController(_mockLegoHub.Object, "");
        }

        [Fact]
        public void ToString_CleansBleDeviceId()
        {
            _mockLegoHub.SetupGet(h => h.HubType).Returns(Models.Enums.HubType.BoostMoveHub);
            _hubController = new HubController(_mockLegoHub.Object, "BluetoothLE#BluetoothLE34:34:23");

            var result = _hubController.ToString();

            Assert.Equal("BoostMoveHub (34:34:23)", result);
        }
    }
}
