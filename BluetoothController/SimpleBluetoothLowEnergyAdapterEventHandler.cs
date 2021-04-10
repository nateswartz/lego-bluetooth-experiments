using BluetoothController.Controllers;
using BluetoothController.Models;
using System.Threading.Tasks;

namespace BluetoothController
{
    public class SimpleBluetoothLowEnergyAdapterEventHandler : IBluetoothLowEnergyAdapterEventHandler
    {
        public Task HandleDiscoveryAsync(DiscoveredDevice discoveredDevice)
        {
            return Task.CompletedTask;
        }

        public Task HandleConnectAsync(IHubController hubController, string errorMessage)
        {
            return Task.CompletedTask;
        }

        public Task HandleNotificationAsync(IHubController hubController, string notification)
        {
            return Task.CompletedTask;
        }

        public Task HandleDisconnectAsync(IHubController hubController)
        {
            return Task.CompletedTask;
        }
    }
}