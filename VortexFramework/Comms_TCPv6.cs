using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Vortex
{
    class Comms_TCPv6 : Comms_Interface_Connection
    {
        Thread t1;
        MemoryStream ms = new MemoryStream();
        static int CurrentSendPort = 0;
        static bool StayAlive = true;
        Socket listener;

        // Client socket and Receive buffer
        public class StateObject { public Socket workSocket = null; public const int BufferSize = 1024; public byte[] buffer = new byte[BufferSize]; }
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public Comms_TCPv6(TransportPort Port) : base(Port)
        {
            Protocol = ConnectionProtocol.TCPv6;
            StayAlive = true;
            CurrentSendPort = Port;
            t1 = new Thread(new ThreadStart(StartListening));
            t1.Start();
        }
        
        private void StartListening()
        {
            byte[] bytes = new Byte[1024];
            IPEndPoint ipEnd = new IPEndPoint(IPAddress.IPv6Any, Port);
            listener = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(ipEnd);
                listener.Listen(100);
                while (StayAlive)
                {
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }
            }
            catch //(Exception err)
            {
                
            }
        }

        public override void CloseConnection()
        {
            StayAlive = false;
            listener.Close();
            try
            {
                t1.Abort();
            }
            catch
            { }
        }
        
        public void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                allDone.Set();
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);
                StateObject state = new StateObject
                {
                    workSocket = handler
                };
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
            }
            catch
            { }
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                ms = new MemoryStream();
                ms.Write(state.buffer, 0, bytesRead);
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
            }
            else
            {
                ReceiveQueue.InputQueue((MessagePacket)ms.ToArray());
            }
        }
    }
}
