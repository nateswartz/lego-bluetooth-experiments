using BluetoothController.Util;
using Xunit;

namespace BluetoothController.Tests.Util
{
    public class DataConverterTests
    {
        [Fact]
        public void ByteArrayToString_ProducesExpectedString()
        {
            var byteArray = new byte[2] { 0x03, 0x0a };
            var result = DataConverter.ByteArrayToString(byteArray);

            Assert.Equal("030a", result);
        }

        [Fact]
        public void ByteArrayToString_HandlesEmptyArray()
        {
            var byteArray = new byte[0];
            var result = DataConverter.ByteArrayToString(byteArray);

            Assert.Equal("", result);
        }
    }
}
