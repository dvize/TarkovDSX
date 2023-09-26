using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using UnityEngine;

namespace DSX
{
    public class DualSenseConnection
    {
        private readonly UdpClient _client;
        private readonly IPEndPoint _endPoint;

        public const int ControllerIndex = 0;

        public DualSenseConnection()
        {
            _client = new UdpClient();

            var portNumber = 6969; 
            _endPoint = new IPEndPoint(Triggers.localhost, portNumber);
        }

        public void Send(Packet theInstructions)
        {
            var data = Encoding.ASCII.GetBytes(Triggers.PacketToJson(theInstructions));
            _client.Send(data, data.Length, _endPoint);
        }

        /// <summary>
        /// All of the trigger functionality broken into methods to send
        /// </summary>

        


    }
}