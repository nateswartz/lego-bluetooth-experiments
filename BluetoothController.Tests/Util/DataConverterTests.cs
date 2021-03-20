using BluetoothController.Util;
using System;
using System.Linq;
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

        [Fact]
        public void HexStringToByteArray_ProducesExpectedByteArray()
        {
            var hexString = "030b";
            var result = DataConverter.HexStringToByteArray(hexString);

            Assert.Equal(2, result.Count());
            Assert.Equal(0x03, result[0]);
            Assert.Equal(0x0b, result[1]);
        }

        [Fact]
        public void HexStringToByteArray_InvalidString_ThrowsException()
        {
            var hexString = "test";
            Assert.Throws<FormatException>(() => DataConverter.HexStringToByteArray(hexString));
        }

        [Fact]
        public void MillisecondsToHex_ProducesExpectedValue()
        {
            var result = DataConverter.MillisecondsToHex(100);
            Assert.Equal("6400", result);
        }

        [Fact]
        public void PowerPercentageToHex_Clockwise_ProducesExpectedValue()
        {
            var result = DataConverter.PowerPercentageToHex(20, true);
            Assert.Equal("14", result);
        }

        [Fact]
        public void PowerPercentageToHex_NotClockwise_ProducesExpectedValue()
        {
            var result = DataConverter.PowerPercentageToHex(20, false);
            Assert.Equal("EB", result);
        }

        [Fact]
        public void PowerPercentageToHex_0_ProducesExpectedValue()
        {
            var result = DataConverter.PowerPercentageToHex(0, false);
            Assert.Equal("FF", result);
        }
    }
}
