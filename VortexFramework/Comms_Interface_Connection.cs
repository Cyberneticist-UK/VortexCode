using System;
using System.Collections;
using System.Collections.Generic;

namespace Vortex
{
    public struct Comms_ConnectionDetail
    {
        public ConnectionProtocol Protocol;
        public TransportPort Port;

        public override string ToString()
        {
            return ((int)Protocol).ToString() + "#" + Port.ToString();
        }

        public Comms_ConnectionDetail(string connection)
        {
            string[] Splitter = connection.Split(new char[] { '#' });
            Port = new TransportPort();
            Protocol = (ConnectionProtocol)Enum.Parse(typeof(ConnectionProtocol), Splitter[0]);
            switch (Protocol)
            {
                case ConnectionProtocol.TCP:
                    Port = Convert.ToInt32(Splitter[1]);
                    break;
                case ConnectionProtocol.TCPv6:
                    Port = Convert.ToInt32(Splitter[1]);
                    break;
                case ConnectionProtocol.UDP:
                    Port = Convert.ToInt32(Splitter[1]);
                    break;
                case ConnectionProtocol.UDPv6:
                    Port = Convert.ToInt32(Splitter[1]);
                    break;
                case ConnectionProtocol.WebServer:
                    Port = Guid.Parse(Splitter[1]);
                    break;
                case ConnectionProtocol.WebClient:
                    Port = Splitter[1];
                    break;
                case ConnectionProtocol.WebService:
                    Port = Splitter[1];
                    break;
            }
        }
    }

    /// <summary>
    /// This is the interface used to create new connection types on Netelligence
    /// </summary>
    class Comms_Interface_Connection
    {
        public event MessageTransfer MessageReceived;
        public ConnectionProtocol Protocol = ConnectionProtocol.NotSet;
        public TransportPort Port;
        protected string MyIP = Comms_General.GetMyIP();
        protected static Comms_Receive_Queue ReceiveQueue = new Comms_Receive_Queue();



        public Comms_Interface_Connection(TransportPort Port)
        {
            this.Port = Port;
            ReceiveQueue.MessageReceived += ReceiveQueue_MessageReceived;
        }

        private void ReceiveQueue_MessageReceived(byte[] Message)
        {
            MessageReceived?.Invoke(Message);
        }

        /// <summary>
        /// Turns the connection into something that can rebuild the connection correctly
        /// </summary>
        /// <returns>A string of course!</returns>
        public override string ToString()
        {
            return ((int)Protocol).ToString() + "#" + Port.ToString();
        }

        public virtual void CloseConnection()
        {
            this.Protocol = ConnectionProtocol.NotSet;
        }

        

    }

    /// <summary>
    /// The Comms Queue collects packets of data received via the TCP or UDP link and sends them via an event to
    /// The program hosting the queue when the whole message has been received.
    /// </summary>
    class Comms_Receive_Queue
    {
        public event MessageTransfer MessageReceived;
        public Hashtable Packets = new Hashtable();
        Sec_Crypt Crypt = new Sec_Crypt();

        public Comms_Receive_Queue()
        {    
            Crypt.SetupAESEncrypt(Comms_General.EncryptedChannel.PacketConfuse);
        }

        public void InputQueue(MessagePacket Packet)
        {            
            if (Packet.MessageNumber == 0 && Packet.TotalMessages == 1)
            {
                MessageData message = Comms_General.ConvertToMessage(Packet);
                if (Comms_General.EncryptedChannel.CommandEncrypted(message.Command) == true)
                    message.Data = Crypt.QuickAESDecrypt(message.Data);
                MessageReceived?.Invoke(message);
            }
            else
            {
                if (Packets.ContainsKey(Packet.MessageGuid) == false)
                {
                    Packets.Add(Packet.MessageGuid, new List<MessagePacket>());
                }
                ((List<MessagePacket>)Packets[Packet.MessageGuid]).Add(Packet);
                if (Comms_General.CheckPackets(((List<MessagePacket>)Packets[Packet.MessageGuid])) == ErrorCode.No_Error)
                {
                    MessageData message = Comms_General.ConvertToMessage(((List<MessagePacket>)Packets[Packet.MessageGuid]));
                    Packets.Remove(Packet.MessageGuid);
                    if (Comms_General.EncryptedChannel.CommandEncrypted(message.Command) == true)
                        message.Data = Crypt.QuickAESDecrypt(message.Data);
                    MessageReceived?.Invoke(message);
                }
            }
        }

        public void InputQueue(MessageData Data)
        {
            MessageReceived?.Invoke(Data);
        }
    }
}
