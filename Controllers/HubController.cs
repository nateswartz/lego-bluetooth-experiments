using LegoBoostController.Commands.Boost;
using LegoBoostController.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace LegoBoostController.Controllers
{
    public class HubController
    {
        public GattCharacteristic HubCharacteristic { get; set; }

        public PortState PortState { get; set; } = new PortState();

        public string SelectedBleDeviceId { get; set; }

        public bool IsConnected { get; set; }

        public bool SubscribedForNotifications { get; set; }

        public string GetCurrentExternalMotorPort()
        {
            return PortState.CurrentExternalMotorPort;
        }

        public async Task<bool> ExecuteCommandAsync(IBoostCommand command)
        {
            return await SetHexValueAsync(command.HexCommand);
        }

        public async Task<bool> SetHexValueAsync(string hex)
        {
            if (hex.Contains(" "))
            {
                hex = hex.Replace(" ", "");
            }
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

        private async Task<bool> WriteBufferToMoveHubCharacteristicAsync(IBuffer buffer)
        {
            try
            {
                // BT_Code: Writes the value from the buffer to the characteristic.
                var result = await HubCharacteristic.WriteValueWithResultAsync(buffer);

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
