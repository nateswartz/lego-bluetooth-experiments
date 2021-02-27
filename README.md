# lego-bluetooth-experiments
A collection of projects to interface with Powered Up smart hubs.
- The BluetoothController project is the low level library that interfaces with the bluetooth device.  This is the main project in this repo.
- The BluetoothLibraryTester project is a scratchpad with some samples of how to use the library.
- The LegoBoostController project shows how to use the library in a UWP application.

The following is a simple example of using the library.  To use it, simply run the program, then power on a hub.
```
class Program
{
    static BluetoothLowEnergyAdapter _adapter;
    static HubController _controller;

    static async Task Main(string[] args)
    {
        _adapter = new BluetoothLowEnergyAdapter(HandleDiscover, HandleConnect, HandleNotification);
        Console.WriteLine("Searching for devices...");
        _adapter.StartBleDeviceWatcher();

        while (_controller == null)
        {
            await Task.Delay(100);
        }

        await _controller.ExecuteCommandAsync(new HubNameCommand());
        await Task.Delay(1000);
        await _controller.ExecuteCommandAsync(new ShutdownCommand());
    }

    static async Task HandleNotification(HubController controller, string message)
    {
        Console.WriteLine($"{controller.HubType}: {message}");
        await Task.CompletedTask;
    }

    static async Task HandleDiscover(DiscoveredDevice device)
    {
        Console.WriteLine($"Discovered device: {device.Name}");
        await Task.CompletedTask;
    }

    static async Task HandleConnect(HubController controller, string errorMessage)
    {
        if (controller != null)
        {
            _controller = controller;

            Console.WriteLine($"Connected device: {Enum.GetName(typeof(HubType), controller.HubType)}");

            await Task.CompletedTask;
        }
        else
        {
            Console.WriteLine($"Failed to connect: {errorMessage}");
        }
    }
}
```

## Credits
I used the sharpbrick/powered-up github project as an example of how to use the Windows bluetooth libraries. 
I leveraged the work of the JorgePe/BOOSTreveng github project to figure out the messaging protocol before Lego released official documentation.

Note: This project is not associated with or supported by The LEGO Group in any way.
