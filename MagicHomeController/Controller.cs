/*
    MagicHomeController is a .NET library to imitate a LED strip controller controlled by the
    chinese MagicHome app. Other names might be ZENGGE or FLUX.

    Reverse engineered, coded and maintained with love by Jonathan Verbeek - 2020
*/

using System.Net;

namespace MagicHomeController
{
    /// <summary>
    /// This is the main interface for a fake controller. It is used to hook events which we receive
    /// from the app.
    /// </summary>
    public class Controller
    {
        /// <summary>
        /// This is the version of the controller, the app can display that. Did not observe any
        /// functionality changes when changing this
        /// </summary>
        public byte ModelVersion = 0x33;

        /// <summary>
        /// Fake firmware version we send to the app. I've seen controllers with v6 and v8.
        /// </summary>
        public byte FirmwareVersion = 0x08;

        /// <summary>
        /// Whether this controller is turned on or off
        /// </summary>
        public bool Power = false;

        /// <summary>
        /// The current color of this controller
        /// </summary>
        public Color Color = Colors.Black;

        /// <summary>
        /// The address of this controller in the network
        /// </summary>
        public readonly IPEndPoint NetworkAddress;

        /// <summary>
        /// Called when the power state changes
        /// </summary>
        public event Events.PowerStateChangeHandler OnPowerStateChange;

        /// <summary>
        /// Called when the color changes
        /// </summary>
        public event Events.ColorChangeHandler OnColorChange;

        /// <summary>
        /// Returns the raw controller data to send to the MagicHome app
        /// </summary>
        public byte[] GetRawControllerData()
        {
            byte[] controller = new byte[13]
            {
                0x81,                           // The command ID
                ModelVersion,                   // Model version, displayed in the app
                (byte)(Power ? 0x23 : 0x24),    // Power state, 0x23 means on, 0x24 means off
                0x61,                           // Pattern mode, 0x61 means "single color"
                0x23,                           // Not sure
                0x09,                           // Pattern speed
                Color.Red,                      // R color
                Color.Green,                    // G color
                Color.Blue,                     // B color
                Color.White,                    // W color (if any)
                FirmwareVersion,                // Firmware version, displayed in the app
                0x00,                           // Not sure
                0x00                            // Not sure
            };

            // Return it
            return controller;
        }

        /// <summary>
        /// Sets the color of this controller and fires the OnColorChange event
        /// </summary>
        public void SetColor(Color newColor)
        {
            // Set the color
            Color = newColor;

            // Fire the event
            OnColorChange(newColor, this);
        }

        /// <summary>
        /// Sets the power of this controller and fires the OnPowerStateChange event
        /// </summary>
        public void SetPowerState(bool newState)
        {
            // Set the state
            Power = newState;

            // Fire the event
            OnPowerStateChange(newState, this);
        }
    }
}
