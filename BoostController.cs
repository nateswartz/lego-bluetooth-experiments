using SDKTemplate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace SDKTemplate
{
    public class BoostController
    {
        public GattCharacteristic MoveHubCharacteristic { get; set; }
        private PortState _state;

        public BoostController(PortState portState)
        {
            _state = portState;
        }

        public async Task<bool> RunMotor(Motor motor, int powerPercentage = 100, int timeInMS = 1000, bool clockwise = true)
        {
            string motorToRun = motor.Code;
            if (motor == Motors.External)
            {
                motorToRun = _state.CurrentExternalMotorPort;
            }

            // For time, LSB first
            var time = timeInMS.ToString("X").PadLeft(4, '0');
            time = $"{time[2]}{time[3]}{time[0]}{time[1]}";
            var power = "";
            if (clockwise)
            {
                power = powerPercentage.ToString("X");
            }
            else
            {
                power = (255 - powerPercentage).ToString("X");
            }
            power = power.PadLeft(2, '0');
            var command = $"0c0081{motorToRun}1109{time}{power}647f03";
            return await SetHexValue(command);
        }

        public async Task<bool> SetHexValue(string hex)
        {
            var bytes = Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();

            var writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteBytes(bytes);

            var writeSuccessful = await WriteBufferToMoveHubCharacteristicAsync(writer.DetachBuffer());
            return writeSuccessful;
        }

        public async Task<bool> WriteBufferToMoveHubCharacteristicAsync(IBuffer buffer)
        {
            try
            {
                // BT_Code: Writes the value from the buffer to the characteristic.
                var result = await MoveHubCharacteristic.WriteValueWithResultAsync(buffer);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    //rootPage.NotifyUser("Successfully wrote value to device", NotifyType.StatusMessage);
                    return true;
                }
                else
                {
                    //rootPage.NotifyUser($"Write failed: {result.Status}", NotifyType.ErrorMessage);
                    return false;
                }
            }
            //catch (Exception ex) when (ex.HResult == E_BLUETOOTH_ATT_INVALID_PDU)
            //{
            //    //rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
            //    return false;
            //}
            catch (Exception ex)// when (ex.HResult == E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED || ex.HResult == E_ACCESSDENIED)
            {
                // This usually happens when a device reports that it support writing, but it actually doesn't.
                //rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                return false;
            }
        }
    }
}
