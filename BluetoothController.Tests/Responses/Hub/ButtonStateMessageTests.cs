using BluetoothController.Responses.Hub;
using Xunit;

namespace BluetoothController.Tests.EventHandlers
{
    public class ButtonStateMessageTests
    {
        [Theory]
        [InlineData("00", ButtonState.Released)]
        [InlineData("01", ButtonState.Pressed)]
        [InlineData("06", ButtonState.Unknown)]
        public void Constructor_SetsCorrectValue(string buttonStateByte, ButtonState expectedState)
        {
            var irrelevantBytes = "0000000000";
            var buttonStateMessage = new ButtonStateMessage($"{irrelevantBytes}{buttonStateByte}");

            Assert.Equal(expectedState, buttonStateMessage.State);
        }
    }
}
