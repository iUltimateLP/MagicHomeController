/*
    MagicHomeController is a .NET library to imitate a LED strip controller controlled by the
    chinese MagicHome app. Other names might be ZENGGE or FLUX.

    Reverse engineered, coded and maintained with love by Jonathan Verbeek - 2020
*/

using System;

namespace MagicHomeController.Example
{
    /// <summary>
    /// Example program demonstrating the use of the MagicHomeController Library
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        static void Main(string[] args)
        {
            // Create a new controller interface which will handle all the logic
            ControllerInterface magicHome = new ControllerInterface();

            // Create a new fake controller
            Controller controller = new Controller();

            // Hook the OnPowerStateChange event so we can trigger something when the user switches us on or off
            controller.OnPowerStateChange += (bool power, Controller c) => {
                Console.WriteLine("Power changed: " + power);
            };

            // Hook the OnColorChange event so we can trigger something when the user changes the color
            controller.OnColorChange += (Color color, Controller c) =>
            {
                Console.WriteLine("Color changed: " + color);
            };

            // Add the controller to the interface and start the spoofing.
            // You can enable the debug flag if you wish to see the communication
            magicHome.Debug = false;
            magicHome.AddController(controller);
            magicHome.Start();

            Console.WriteLine("Fake controller successfully started. Open your MagicHome app!");

            // Wait and don't close the program directly again
            Console.ReadLine();
        }
    }
}
