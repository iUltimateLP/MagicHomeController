/*
    MagicHomeController is a .NET library to imitate a LED strip controller controlled by the
    chinese MagicHome app. Other names might be ZENGGE or FLUX.

    Reverse engineered, coded and maintained with love by Jonathan Verbeek - 2020
*/

using System;

namespace MagicHomeController
{
    /// <summary>
    /// A class representing a color
    /// </summary>
    public class Color
    {
        /// <summary>
        /// The amount of red
        /// </summary>
        public byte Red = 0x00;

        /// <summary>
        /// The amount of green
        /// </summary>
        public byte Green = 0x00;

        /// <summary>
        /// The amount of blue
        /// </summary>
        public byte Blue = 0x00;

        /// <summary>
        /// The amount of white (for RGBW support)
        /// </summary>
        public byte White = 0x00;

        /// <summary>
        /// Constructor taking in the color parameters
        /// </summary>
        public Color(byte Red, byte Green, byte Blue, byte White = 0x00)
        {
            this.Red = Red;
            this.Green = Green;
            this.Blue = Blue;
            this.White = White;
        }

        /// <summary>
        /// Constructor taking in a hex color string
        /// </summary>
        public Color(string Hex)
        {
            // Cut out a #, if any
            if (Hex.StartsWith("#"))
            {
                Hex = Hex.Substring(1);
            }

            // Read the hex string into bytes
            byte[] bytes = new byte[Hex.Length / 2];
            for (int i = 0; i < Hex.Length; i+= 2)
            {
                bytes[i / 2] = Convert.ToByte(Hex.Substring(i, 2), 16);
            }

            // Apply the bytes
            this.Red = bytes[0];
            this.Blue = bytes[1];
            this.Green = bytes[2];
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "(R = " + Red + ", G = " + Green + ", B = " + Blue + (White > 0x00 ? ", W = " + White : "") + ")";
        }
    }

    /// <summary>
    /// A predefined set of colors
    /// </summary>
    public class Colors
    {
        public static Color Black = new Color(0, 0, 0);
        public static Color Red = new Color(255, 0, 0);
        public static Color Blue = new Color(0, 0, 255);
        public static Color Green = new Color(0, 255, 0);
        public static Color Purple = new Color(255, 0, 255);
        public static Color Cyan = new Color(0, 255, 255);
        public static Color Yellow = new Color(255, 255, 0);
        public static Color White = new Color(255, 255, 255);
        public static Color WarmWhite = new Color(0, 0, 0, 255);
    }
}
