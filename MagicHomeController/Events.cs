/*
    MagicHomeController is a .NET library to imitate a LED strip controller controlled by the
    chinese MagicHome app. Other names might be ZENGGE or FLUX.

    Reverse engineered, coded and maintained with love by Jonathan Verbeek - 2020
*/

namespace MagicHomeController
{
    /// <summary>
    /// Holds the delegate definitions
    /// </summary>
    public static class Events
    {
        /// <summary>
        /// Fires when the power state of our fake controller changes
        /// </summary>
        public delegate void PowerStateChangeHandler(bool newState, Controller controller);

        /// <summary>
        /// Fires when the color of our fake controller changes
        /// </summary>
        public delegate void ColorChangeHandler(Color newColor, Controller controller);
    }
}
