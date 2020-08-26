# MagicHomeController
This is a .NET library which fakes a MagicHome/ZENGGE/FLUX LED-Controller, using TCP and UDP sockets.

## How does it work?
I reverse engineered the MagicHome protocol which is in use between the smartphone app MagicHome and a real LED controller. 
Figuring out how it works allows us to implement a fake controller in the network. You can read more about the reverse engineering process [here](https://github.com/iUltimateLP/MagicHomeController/blob/master/REVERSE_ENGINEERING.md).

## Example
The project contains two projects, `MagicHomeController` and `MagicHomeController.Example`. The first project is a library which you can implement in your own projects, and the second project is a simple example which logs out the commands to console.

Here's how you can implement it:
```cs
// This is the interface which handles all the logic
ControllerInterface controllerInterface = new ControllerInterface();

// Create a new controller object which will contain the variables and can call events from the MagicHome app
Controller controller = new Controller();
controller.OnPowerStateChange += (bool power, Controller c) => {
    Console.WriteLine("Power changed: " + power);
};
controller.OnColorChange += (Color color, Controller c) => {
    Console.WriteLine("Color changed: " + color);
};

// Adds the controller to the interface and starts the spoofing process
controllerInterface.AddController(controller);
controllerInterface.Start();

// Keep this window open
Console.ReadLine();
```

## And now?
My original reason I wanted to do this is because I wanted to connect my Logitech RGB keyboard with this, so it is synchronized with the LED strips in my room. What you do with this is up to you.

The events `OnPowerStateChange` and `OnColorChange` should be all you need.
