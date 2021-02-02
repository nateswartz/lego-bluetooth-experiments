using BluetoothBase.Bluetooth;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BluetoothWinRT
{
    public class WinRTBluetoothDevice : IBluetoothDevice
    {
        private BluetoothLEDevice _device;
        public string Name => _device.Name;

        public WinRTBluetoothDevice(BluetoothLEDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
        }

        ~WinRTBluetoothDevice() => Dispose(false);
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

        protected virtual void Dispose(bool disposing)
        {
            _device?.Dispose();
            _device = null;
        }

        public async Task<IBluetoothService> GetServiceAsync(Guid serviceId)
        {
            var gatt = await _device.GetGattServicesForUuidAsync(serviceId);

            if (gatt.Status == GattCommunicationStatus.Success)
            {
                var service = gatt.Services.FirstOrDefault();

                return new WinRTBluetoothService(service);
            }
            else
            {
                return null;
            }
        }
    }

}
