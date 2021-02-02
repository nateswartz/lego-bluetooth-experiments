using BluetoothBase.Bluetooth;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

namespace BluetoothWinRT
{
    public class WinRTBluetoothAdapter : IBluetoothAdapter
    {
        private const ushort LegoCompanyId = 919;
        private const ushort OzobotCompanyId = 1003;

        public void Discover(Func<BluetoothDeviceInfo, Task> discoveryHandler, CancellationToken cancellationToken = default)
        {
            BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();
            watcher.ScanningMode = BluetoothLEScanningMode.Active;

            watcher.Received += ReceivedHandler;

            cancellationToken.Register(() =>
            {
                watcher.Stop();
                watcher.Received -= ReceivedHandler;
            });

            watcher.Start();

            async void ReceivedHandler(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
            {
                var info = new BluetoothDeviceInfo();
                if (eventArgs.Advertisement.ManufacturerData.Count > 0)
                {
                    var companyId = eventArgs.Advertisement.ManufacturerData[0].CompanyId;

                    if (companyId != LegoCompanyId && companyId != OzobotCompanyId)
                        return;

                    var reader = DataReader.FromBuffer(eventArgs.Advertisement.ManufacturerData[0].Data);
                    var data = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(data);

                    info.ManufacturerData = data;

                    using (var device = BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress).AsTask().Result)
                    {
                        if (device == null)
                            return;

                        info.Name = device.Name;
                    }

                }
                else
                    return;

                info.BluetoothAddress = eventArgs.BluetoothAddress;

                await discoveryHandler(info);
            }
        }

        public async Task<IBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress)
        {
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);

            return new WinRTBluetoothDevice(device);
        }
    }

}
