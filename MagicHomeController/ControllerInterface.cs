/*
    MagicHomeController is a .NET library to imitate a LED strip controller controlled by the
    chinese MagicHome app. Other names might be ZENGGE or FLUX.

    Reverse engineered, coded and maintained with love by Jonathan Verbeek - 2020
*/

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MagicHomeController
{
    /// <summary>
    /// The main class handling all the logic around the fake controllers.
    /// </summary>
    public class ControllerInterface
    {
        // MagicHome uses these ports to communicate (found through WireShark)
        private const int MAGICHOME_UDP_PORT = 48899;
        private const int MAGICHOME_TCP_PORT = 5577;

        // MagicHome uses this message to search for devices (also found through WireShark)
        private const string MAGICHOME_DISCOVERY_MESSAGE = "HF-A11ASSISTHREAD";

        // The UDP Internet Protocol endpoint and client used for communicating with the app over UDP
        private IPEndPoint udpEndPoint = new IPEndPoint(IPAddress.Any, MAGICHOME_UDP_PORT);
        private UdpClient udpSocket = new UdpClient(MAGICHOME_UDP_PORT);

        // The TCP listener for the TCP communication
        private TcpListener tcpSocket = new TcpListener(IPAddress.Parse("0.0.0.0"), MAGICHOME_TCP_PORT);

        // The controller object we use
        private Controller controller;

        /// <summary>
        /// Called when the UDP socket receives any data
        /// </summary>
        private void UDPReceiveCallback(IAsyncResult asyncResult)
        {
            // End receiving (because we're async) and read what we received
            byte[] data = udpSocket.EndReceive(asyncResult, ref udpEndPoint);
            string message = Encoding.UTF8.GetString(data);

            // Did the app send us the magic discovery message?
            if (message == MAGICHOME_DISCOVERY_MESSAGE)
            {
                // The UDP protocol is only used to look for devices. Qualified devices return this string
                // containing the IP address, MAC address and Model Code (reverse engineered through WireShark)
                string answer = Utilities.GetLocalIPAddress() + "," + Utilities.GetLocalMACAddress() + ",AK001-ZJ2101";
                byte[] rawAnswer = Encoding.Default.GetBytes(answer);

                // Send this fake device string to the UDP endpoint which asked. From now on, the app will communicate with us over TCP
                udpSocket.BeginSend(rawAnswer, rawAnswer.Count(), udpEndPoint, UDPSendCallback, null);
            }
        }

        /// <summary>
        /// Called when the UDP socket has sent any data
        /// </summary>
        private void UDPSendCallback(IAsyncResult asyncResult)
        {
            // Stop sending (async)
            udpSocket.EndSend(asyncResult);

            // Be ready to receive new data over UDP again
            udpSocket.BeginReceive(UDPReceiveCallback, null);
        }

        /// <summary>
        /// Called when the TCP socket receives any data
        /// </summary>
        private void TCPReceiveCallback(IAsyncResult asyncResult)
        {
            // Accept the incoming connection (async) and get the network strea containing our data
            TcpClient client = tcpSocket.EndAcceptTcpClient(asyncResult);
            NetworkStream networkStream = client.GetStream();

            // Read all the data while we're still connected. This makes sure we read every bit from the MagicHome app
            // because it will close the connection itself when it's done
            while (client.Connected)
            {
                // Read the message we received
                byte[] tcpMessage = new byte[60];
                networkStream.Read(tcpMessage, 0, tcpMessage.Length);

                // Now we will handle the different commands the MagicHome app can send us. 
                HandleMagicHomeCommand(ref networkStream, tcpMessage);
            }

            // The client (MagicHome app) closed our connection, so we can close it too
            client.Close();

            // Begin to accept new TCP connections again
            tcpSocket.BeginAcceptTcpClient(TCPReceiveCallback, null);
        }

        /// <summary>
        /// Reads the raw data received from the MagicHome app and acts accordingly
        /// </summary>
        private void HandleMagicHomeCommand(ref NetworkStream networkStream, byte[] payload)
        {
            // All of these were reverse engineered using an Android packet filter
            // The first byte is always the command ID
            byte command = payload[0];

            // Command: 0x81 0x8a 0x8b 0x96 - ask about the light's status
            if (command == 0x81 && payload[1] == 0x8a && payload[2] == 0x8b && payload[3] == 0x96)
            {
                // Get the raw controller data to send
                byte[] controllerData = controller.GetRawControllerData();

                // Write the data to the TCP connection
                networkStream.Write(controllerData, 0, controllerData.Length);
            }
            // Command: 0x71 ON/OFF REMOTE/LOCAL - toggle the light's power state
            else if (command == 0x71)
            {
                // As above, 0x23 means the light should turn on, 0x24 means the light should turn off
                bool turnOn = payload[1] == 0x23;

                // Indicates whether the light was toggeled via the local network or remotly (0x0f means local, 0xf0 means remote)
                bool local = payload[2] == 0x0f;

                // Apply this to the controller
                controller.SetPowerState(turnOn);

                // The payload response for this command is four bytes:
                byte[] responseData = new byte[4] {
                        0xf0, // The inverse of tcpMessage[2]
                        0x71, // The command ID
                        payload[1], // The new state (which is the same as we were told in the command)
                        0x00  // Checksum
                    };

                // Calculate the checksum for the payload and set the last byte to it
                responseData[3] = Utilities.CalculateChecksum(responseData);

                // Write the data to the TCP connection
                networkStream.Write(responseData, 0, responseData.Length);
            }
            // Command: 0x31 RR GG BB WW - set the color of the light
            else if (command == 0x31)
            {
                // Parse all color components
                byte r = payload[1];
                byte g = payload[2];
                byte b = payload[3];
                byte w = payload[4];

                // Tell the controller
                controller.SetColor(new Color(r, g, b, w));
                
                // Don't need to send a response here
            }
        }

        /// <summary>
        /// Adds a controller. Notice that because only one IP/MAC can be one controller, you can only add one
        /// </summary>
        public void AddController(Controller Controller)
        {
            this.controller = Controller;
        }

        /// <summary>
        /// Start the networking functions to insert a fake controller into the network
        /// </summary>
        public void Start()
        {
            if (controller == null)
            {
                throw new ControllerException("No controller set!");
            }

            // Start listening for UDP data
            udpSocket.BeginReceive(UDPReceiveCallback, null);

            // Start listening for TCP data
            tcpSocket.Start();
            tcpSocket.BeginAcceptTcpClient(TCPReceiveCallback, null);
        }
    }
}
