/*
    MagicHomeController is a .NET library to imitate a LED strip controller controlled by the
    chinese MagicHome app. Other names might be ZENGGE or FLUX.

    Reverse engineered, coded and maintained with love by Jonathan Verbeek - 2020
*/

using System;

namespace MagicHomeController
{
    /// <summary>
    /// The exception class this library can throw
    /// </summary>
    public class ControllerException : Exception
    {
        /// <summary>
        /// Message of the exception
        /// </summary>
        new public string Message { get; private set; }

        /// <summary>
        /// Constructor taking in the message
        /// </summary>
        public ControllerException(string Message)
        {
            this.Message = Message;
        }
    }
}
