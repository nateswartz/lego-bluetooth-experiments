using BluetoothController.Commands.Abstract;
using BluetoothController.Models;
using Xunit;

namespace BluetoothController.Tests.Util
{
    public class TestCommand : CommandType
    {
        public TestCommand(MessageType messageType) : base(messageType)
        { }
    }

    public class CommandTypeTests
    {
        [Fact]
        public void AddHeader_AddsCorrectHeader()
        {
            var messageType = new MessageType("05");
            var commandWithHeader = new TestCommand(messageType).AddHeader("000102");

            Assert.Equal("060005000102", commandWithHeader);
        }
    }
}
