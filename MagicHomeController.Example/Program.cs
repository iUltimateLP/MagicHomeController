using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicHomeController;

namespace MagicHomeController.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            ControllerInterface controllerInterface = new ControllerInterface();

            Controller controller = new Controller();
            controller.OnPowerStateChange += (bool power, Controller c) => {
                Console.WriteLine("Power changed: " + power);
            };
            controller.OnColorChange += (Color color, Controller c) =>
            {
                Console.WriteLine("Color changed: " + color);
            };

            controllerInterface.AddController(controller);
            controllerInterface.Start();

            Console.ReadLine();
        }
    }
}
