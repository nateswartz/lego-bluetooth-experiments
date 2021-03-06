﻿using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Models;
using BluetoothController.Models.Enums;
using BluetoothController.Responses.Device.Data;
using BluetoothController.Responses.Device.Info;
using BluetoothController.Responses.Device.State;
using BluetoothController.Responses.Hub;
using System.Linq;

namespace BluetoothController.Responses
{
    internal static class ResponseFactory
    {
        public static Response CreateResponse(string notification, IHubController controller)
        {
            var messageType = MessageTypes.GetByCode(notification.Substring(4, 2));

            if (messageType == MessageTypes.HubProperty)
                return HandleHubProperty(controller, notification);
            if (messageType == MessageTypes.HubAction)
                return new HubActionResponse(notification);
            if (messageType == MessageTypes.Error)
                return new Error(notification);
            if (messageType == MessageTypes.HubAttachedDetachedIO)
                return HandleIOConnectionStateChange(controller, notification);
            if (messageType == MessageTypes.PortInformation)
                return new PortInfo(notification);
            if (messageType == MessageTypes.PortModeInformation)
                return new PortModeInfo(notification);
            if (messageType == MessageTypes.PortValueSingle)
                return HandlePortValueUpdate(controller, notification);
            if (messageType == MessageTypes.PortInputFormatSingle)
                return HandleNotificationStateUpdate(controller, notification);
            if (messageType == MessageTypes.PortOutputFeedback)
                return new PortOutputFeedback(notification);

            return new Response(notification);
        }

        private static Response HandleNotificationStateUpdate(IHubController controller, string notification)
        {
            var portState = new PortNotificationState(notification);
            controller.Hub.GetPortByID(portState.Port).NotificationMode = portState.Mode;
            return portState;
        }

        private static Response HandleHubProperty(IHubController _, string notification)
        {
            var deviceInfo = new HubInfo(notification);
            return deviceInfo.DeviceType switch
            {
                HubInfoType.HubName => new HubName(notification),
                HubInfoType.ButtonState => new ButtonStateMessage(notification),
                HubInfoType.FirmwareVersion => new FirmwareVersion(notification),
                HubInfoType.SystemType => new SystemType(notification),
                _ => deviceInfo,
            };
        }

        private static Response HandleIOConnectionStateChange(IHubController controller, string notification)
        {
            var portInfo = new PortState(notification);

            if (portInfo.StateChangeEvent == DeviceState.Detached)
            {
                return HandleIODetached(controller, portInfo);
            }
            return HandleIOAttached(controller, portInfo);
        }

        private static Response HandleIODetached(IHubController controller, PortState portInfo)
        {
            var hub = controller.Hub;
            var detachedPort = hub.Ports.Where(p => p.PortID == portInfo.Port).FirstOrDefault();
            if (detachedPort != null)
            {
                portInfo.DeviceType = detachedPort.DeviceType;
                detachedPort.DeviceType = IoDeviceTypes.None;
            }
            return portInfo;
        }

        private static Response HandleIOAttached(IHubController controller, PortState portInfo)
        {
            AddIODeviceToHubPortList(controller, portInfo);

            return portInfo;
        }

        private static void AddIODeviceToHubPortList(IHubController controller, PortState portInfo)
        {
            var hub = controller.Hub;

            if (hub.GetPortByID(portInfo.Port) == null)
            {
                hub.Ports.Add(new HubPort
                {
                    PortID = portInfo.Port,
                    DeviceType = portInfo.DeviceType
                });
            }
            else
            {
                hub.GetPortByID(portInfo.Port).DeviceType = portInfo.DeviceType;
            }
        }

        private static Response HandlePortValueUpdate(IHubController controller, string notification)
        {
            var sensorData = new SensorData(notification);
            var hub = controller.Hub;
            var port = hub.GetPortByID(sensorData.Port);

            if (port?.DeviceType == IoDeviceTypes.ColorDistanceSensor)
                return new ColorDistanceData(notification, port.NotificationMode);
            if (port?.DeviceType == IoDeviceTypes.BoostTachoMotor)
                return new BoostMotorData(notification, port.NotificationMode);
            if (port?.DeviceType == IoDeviceTypes.TrainMotor)
                return new TrainMotorData(notification);
            if (port?.DeviceType == IoDeviceTypes.TiltSensor)
                return new TiltData(notification);
            if (port?.DeviceType == IoDeviceTypes.RemoteButton)
                return new RemoteButtonData(notification);
            if (port?.DeviceType == IoDeviceTypes.VoltageSensor)
                return new VoltageData(notification);
            if (port?.DeviceType == IoDeviceTypes.DistanceSensor)
                return new DistanceData(notification);
            if (port?.DeviceType == IoDeviceTypes.ColorSensor)
                return new ColorData(notification, port.NotificationMode);

            return sensorData;
        }
    }
}
