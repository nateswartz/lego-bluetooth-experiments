using BluetoothController.Controllers;
using BluetoothController.Models;
using System.Threading.Tasks;

namespace BluetoothController
{
    public interface IBluetoothLowEnergyAdapterEventHandler
    {
        public Task HandleDiscoveryAsync(DiscoveredDevice discoveredDevice);
        public Task HandleConnectAsync(IHubController hubController, string errorMessage);
        public Task HandleNotificationAsync(IHubController hubController, string notification);
        public Task HandleDisconnectAsync(IHubController hubController);
    }
}