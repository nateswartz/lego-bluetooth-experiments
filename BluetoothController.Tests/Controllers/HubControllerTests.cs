using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Wrappers;
using Moq;
using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
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

        private void VerifyMocks()
        {
            _mockGattCharacteristicWrapper.VerifyAll();
            _mockGattCharacteristicWrapper.VerifyNoOtherCalls();

            _mockLegoHub.VerifyAll();
            _mockLegoHub.VerifyNoOtherCalls();
        }

        [Fact]
        public void ToString_CleansBleDeviceId()
        {
            _mockLegoHub.SetupGet(h => h.HubType).Returns(Models.Enums.HubType.BoostMoveHub);
            _hubController = new HubController(_mockLegoHub.Object, "BluetoothLE#BluetoothLE34:34:23");

            var result = _hubController.ToString();

            VerifyMocks();
            Assert.Equal("BoostMoveHub (34:34:23)", result);
        }

        [Fact]
        public async Task InitializeAsync_EnableNotificationsSuccessful_CorrectMocksAreCalled()
        {
            _mockGattCharacteristicWrapper.Setup(x => x.WriteValueWithResultAsync(It.IsAny<IBuffer>()));
            _mockGattCharacteristicWrapper.Setup(x => x.AddValueChangedHandler(It.IsAny<Func<IBuffer, Task>>()));
            _mockGattCharacteristicWrapper.Setup(x => x.EnableNotificationsAsync()).ReturnsAsync(true);

            await _hubController.InitializeAsync(null, _mockGattCharacteristicWrapper.Object);

            VerifyMocks();
            _mockGattCharacteristicWrapper.Verify(x => x.WriteValueWithResultAsync(It.IsAny<IBuffer>()), Times.Exactly(2));
        }

        [Fact]
        public async Task InitializeAsync_EnableNotificationsFails_CorrectMocksAreCalled()
        {
            _mockGattCharacteristicWrapper.Setup(x => x.WriteValueWithResultAsync(It.IsAny<IBuffer>()));
            _mockGattCharacteristicWrapper.Setup(x => x.AddValueChangedHandler(It.IsAny<Func<IBuffer, Task>>()));
            _mockGattCharacteristicWrapper.Setup(x => x.EnableNotificationsAsync()).ReturnsAsync(false);
            _mockGattCharacteristicWrapper.Setup(x => x.RemoveValueChangedHandler());

            await _hubController.InitializeAsync(null, _mockGattCharacteristicWrapper.Object);

            VerifyMocks();
            _mockGattCharacteristicWrapper.Verify(x => x.WriteValueWithResultAsync(It.IsAny<IBuffer>()), Times.Exactly(2));
        }
    }
}
