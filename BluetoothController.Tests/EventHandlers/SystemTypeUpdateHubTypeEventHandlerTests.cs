using BluetoothController.Controllers;
using BluetoothController.EventHandlers.Internal;
using BluetoothController.Hubs;
using BluetoothController.Models;
using BluetoothController.Responses.Hub;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace BluetoothController.Tests.EventHandlers
{
    public class SystemTypeUpdateHubTypeEventHandlerTests
    {
        [Theory]
        [InlineData("40", HubType.BoostMoveHub)]
        [InlineData("41", HubType.TwoPortHub)]
        [InlineData("42", HubType.TwoPortHandset)]
        public async Task HandleEventAsync_SetsType(string systemTypeCode, HubType hubType)
        {
            var controllerMock = new Mock<IHubController>();
            var legoHubMock = new Mock<ILegoHub>();

            controllerMock.SetupGet(x => x.Hub).Returns(legoHubMock.Object);

            legoHubMock.SetupSet(x => x.HubType = hubType);

            var eventHandler = new SystemTypeUpdateHubTypeEventHandler(controllerMock.Object);

            await eventHandler.HandleEventAsync(new SystemType($"0000000000{systemTypeCode}"));

            controllerMock.VerifyAll();
            controllerMock.VerifyNoOtherCalls();

            legoHubMock.VerifyAll();
            legoHubMock.VerifyNoOtherCalls();
        }
    }
}
