using System;
using System.Net;
using System.Net.Sockets;

namespace Vortex
{
    /// <summary>
    /// This is a UDP interface to the server.
    /// </summary>
    class Comms_UDPv6: Comms_Interface_Connection
    {
        private UdpClient udp = null;
        private bool Listen = true;
        private Guid CloseGUID = System.Guid.NewGuid();
        static IPEndPoint ip;
        

        public Comms_UDPv6(TransportPort Port) : base(Port)
        {
            Protocol = ConnectionProtocol.UDPv6;
            Listen = true;
            ip = new IPEndPoint(IPAddress.Broadcast, Port.ToInt());
            StartListening();
        }

        public override void CloseConnection()
        {
            udp.Close();
            udp.Dispose();
            Listen = false;
        }

        private void StartListening()
        {
            Listen = true;
            if(udp == null)
                udp = new UdpClient(Port.ToInt(), AddressFamily.InterNetworkV6);
            udp.BeginReceive(Receive, new object());
        }

        private void Receive(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.IPv6Any, Port.ToInt());
            try
            {
                MessagePacket BroadcastMessage = udp.EndReceive(ar, ref ip);
                if (BroadcastMessage.ToByteArray().Length > 0)
                    ReceiveQueue.InputQueue(BroadcastMessage);
            }
            catch
            { }
            finally
            {
                if (Listen == true)
                    StartListening();
            }
        }

        
    }
}
