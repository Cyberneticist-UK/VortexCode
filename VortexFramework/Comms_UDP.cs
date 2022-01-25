using System;
using System.Net;
using System.Net.Sockets;

namespace Vortex
{
    /// <summary>
    /// This is a UDP interface to the server.
    /// </summary>
    class Comms_UDP: Comms_Interface_Connection
    {
        private UdpClient udp = null;
        private bool Listen = true;
        private Guid CloseGUID = System.Guid.NewGuid();
        static IPEndPoint ip;
        

        public Comms_UDP(TransportPort Port) : base(Port)
        {
            Protocol = ConnectionProtocol.UDP;
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
            try
            {
                Listen = true;
                if (udp == null)
                    udp = new UdpClient(Port.ToInt());
                udp.BeginReceive(Receive, new object());
            }
            catch
            {

            }
        }

        private void Receive(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, Port.ToInt());
            try
            {
                MessagePacket BroadcastMessage = udp.EndReceive(ar, ref ip);
                if (BroadcastMessage.ToByteArray().Length > 0)
                    ReceiveQueue.InputQueue(BroadcastMessage);
            }
            catch //(Exception err)
            {

            }
            finally
            {
                if (Listen == true)
                    StartListening();
            }
        }

        
    }
}
