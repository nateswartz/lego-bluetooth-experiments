using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses;
using System.Threading.Tasks;

namespace BluetoothController
{
    internal class SimpleBluetoothLowEnergyAdapterEventHandler : IBluetoothLowEnergyAdapterEventHandler
    {
        public Task HandleDiscoveryAsync(DiscoveredDevice discoveredDevice)
        {
            return Task.CompletedTask;
        }

        public Task HandleConnectAsync(IHubController hubController, string errorMessage)
        {
            return Task.CompletedTask;
        }

        public Task HandleNotificationAsync(IHubController hubController, Response notification)
        {
            return Task.CompletedTask;
        }

        public Task HandleDisconnectAsync(IHubController hubController)
        {
            return Task.CompletedTask;
        }
    }
}