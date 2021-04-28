using BluetoothController.Models.Enums;
using BluetoothController.Responses.Device.Info;
using Xunit;

namespace BluetoothController.Tests.Responses.Device.Info
{
    public class PortInfoTests
    {
        [Fact]
        public void Constructor_SetsCorrectPort()
        {
            var header = "000000";
            var footer = "000000000000000";
            var messageBody = $"{header}02{footer}";
            var portInfoMessage = new PortInfo(messageBody);

            Assert.Equal("02", portInfoMessage.Port);
        }

        [Fact]
        public void Constructor_SetsCorrectInformationType()
        {
            var header = "00000000";
            var footer = "0000000000000";
            var messageBody = $"{header}01{footer}";
            var portInfoMessage = new PortInfo(messageBody);

            Assert.Equal(InformationType.ModeInfo, portInfoMessage.InfoType);
        }
    }
}
