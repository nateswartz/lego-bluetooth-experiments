﻿using BluetoothController.Controllers;
using BluetoothController.Responses;
using BluetoothController.Responses.Hub;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    internal class SystemTypeUpdateHubTypeEventHandler : IEventHandler
    {
        private readonly HubController _controller;

        public Type HandledEvent { get; } = typeof(SystemType);

        public SystemTypeUpdateHubTypeEventHandler(HubController controller)
        {
            _controller = controller;
        }

        public async Task HandleEventAsync(Response response)
        {
            var data = (SystemType)response;
            _controller.Hub.HubType = data.HubType;
            await Task.CompletedTask;
        }
    }
}
