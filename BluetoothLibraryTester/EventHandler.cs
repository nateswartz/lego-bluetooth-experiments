using BluetoothController;
using BluetoothController.Controllers;
using BluetoothController.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BluetoothLibraryTester
{
    class EventHandler : IBluetoothLowEnergyAdapterEventHandler
    {
        private readonly List<IHubController> _controllers;

        public EventHandler(List<IHubController> controllers)
        {
            _controllers = controllers;
        }

        public async Task HandleNotificationAsync(IHubController controller, string message)
        {
            Console.WriteLine($"{controller.Hub.HubType}: {message}");
            await Task.CompletedTask;
        }

        public async Task HandleDiscoveryAsync(DiscoveredDevice device)
        {
            Console.WriteLine($"Discovered device: {device.Name}");
            await Task.CompletedTask;
        }

        public async Task HandleConnectAsync(IHubController controller, string errorMessage)
        {
            if (controller != null)
            {
                _controllers.Add(controller);

                Console.WriteLine($"Connected device: {controller.Hub.HubType}");

                await Task.CompletedTask;
            }
            else
            {
                Console.WriteLine($"Failed to connect: {errorMessage}");
            }
        }

        public async Task HandleDisconnectAsync(IHubController controller)
        {
            _controllers.Remove(controller);
            Console.WriteLine($"Disconnected device: {controller.Hub.HubType}");
            await Task.CompletedTask;
        }
    }
}
