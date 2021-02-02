using BluetoothBase.Bluetooth;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BluetoothWinRT
{
    public class WinRTBluetoothService : IBluetoothService
    {
        private GattDeviceService _service;
        public Guid Uuid => _service.Uuid;

        public WinRTBluetoothService(GattDeviceService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        ~WinRTBluetoothService() => Dispose(false);
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
        protected virtual void Dispose(bool disposing)
        {
            _service.Dispose();
            _service = null;
        }

        public async Task<IBluetoothCharacteristic> GetCharacteristicAsync(Guid guid)
        {
            var characteristics = await _service.GetCharacteristicsForUuidAsync(guid);

            if (characteristics.Status == GattCommunicationStatus.Success && characteristics.Characteristics.Count() > 0)
            {
                var characteristic = characteristics.Characteristics.FirstOrDefault();

                return new WinRTBluetoothCharacteristic(characteristic);
            }
            else
            {
                return null;
            }
        }
    }

}
