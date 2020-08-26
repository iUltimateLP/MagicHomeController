/*
    MagicHomeController is a .NET library to imitate a LED strip controller controlled by the
    chinese MagicHome app. Other names might be ZENGGE or FLUX.

    Reverse engineered, coded and maintained with love by Jonathan Verbeek - 2020
*/

using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace MagicHomeController
{
    /// <summary>
    /// A few utility functios
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// Returns the local IP address of this machine
        /// </summary>
        public static string GetLocalIPAddress()
        {
            // Get the DNS host list
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            // Find the first IP address which has the InnerNetwork family
            string localIP = host.AddressList.FirstOrDefault((IPAddress addr) => { return addr.AddressFamily == AddressFamily.InterNetwork; }).ToString();

            // Return it
            return localIP;
        }

        /// <summary>
        /// Returns the MAC address of the local machine
        /// </summary>
        public static string GetLocalMACAddress()
        {
            // Search for a interface which is up, and not the loopback interface
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault();
        }

        /// <summary>
        /// Calculates the one-byte checksum of a payload. MagicHome uses an algorithm to verify the integrity
        /// of the commands and payloads. However, I cracked it using a simple calculator and some logic thinking
        /// </summary>
        public static byte CalculateChecksum(byte[] bytes)
        {
            // The checksum algorithm works as follows:
            // All bytes are summed together
            int sum = 0;
            foreach (byte b in bytes)
            {
                sum += b;
            }

            // Then the resulting number gets converted to a hex string
            string str = sum.ToString("X");

            // Of that string, the last two digits are taken
            str = str.Substring(str.Length - 2, 2);

            // And directly converted into a byte (without encoding)
            return Convert.ToByte(str, 16);
        }

        /// <summary>
        /// Converts the bytes to a readable hex string
        /// </summary>
        public static string BytesToHexString(byte[] bytes)
        {
            // Will hold the result
            string outString = "";

            // Go through each byte and append it's string representation to the string
            for (int i = 0; i < bytes.Length; i++)
            {
                outString += bytes[i].ToString("X") + " ";
            }

            // Return it
            return outString;
        }
    }
}
