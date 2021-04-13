using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses;
using System.Threading.Tasks;

namespace BluetoothController
{
    public interface IBluetoothLowEnergyAdapterEventHandler
    {
        public Task HandleDiscoveryAsync(DiscoveredDevice discoveredDevice);
        public Task HandleConnectAsync(IHubController hubController, string errorMessage);
        public Task HandleNotificationAsync(IHubController hubController, Response notification);
        public Task HandleDisconnectAsync(IHubController hubController);
    }
}