using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Vortex
{
    class Comms_Sender
    {
        public event StringTransfer SendError;
        List<TransportMessage> Messages = new List<TransportMessage>();
        Thread bgnd;

        public void InputQueue(TransportMessage Message)
        {
            Messages.Add(Message);
            if (bgnd == null || bgnd.ThreadState == ThreadState.Unstarted || bgnd.ThreadState == ThreadState.Stopped)
            {
                bgnd = StartSend();
                bgnd.Start();
            }
        }

        public Thread StartSend()
        {
            var t = new Thread(() => ProcessMessages());
            return t;
        }

        private void RemoveFromQueue()
        {
            Messages.RemoveAt(0);
        }

        private void ProcessMessages()
        {
            Sec_Crypt Crypt = new Sec_Crypt();
            Crypt.SetupAESEncrypt(Comms_General.EncryptedChannel.PacketConfuse);
            List<MessagePacket> temp = new List<MessagePacket>();
            while (Messages.Count > 0)
            {
                TransportMessage tempMessage = Messages[0];
                if (Comms_General.EncryptedChannel.CommandEncrypted(tempMessage.Data.Command) == true)
                    tempMessage.Data.Data = Crypt.QuickAESEncrypt(tempMessage.Data.Data);
                switch (tempMessage.Protocol)
                {
                    case ConnectionProtocol.TCP:

                        temp = Comms_General.ConvertToPackets(tempMessage.ToUserID.Machine.IPAddress, tempMessage.Data);
                        foreach(MessagePacket Packet in temp)
                        {
                            Socket clientSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            try //Will this work?
                            {
                                clientSock.Connect(Packet.Address, tempMessage.Port.ToInt());
                                clientSock.Send(Packet.ToByteArray());
                            }
                            catch
                            {
                                SendError?.Invoke("Could not send TCP Message to " + Packet.Address);
                            }
                            finally
                            {
                                clientSock.Close();
                            }
                            Thread.Sleep(1);
                            clientSock.Close();
                            Thread.Sleep(1);
                        }
                        break;
                    case ConnectionProtocol.TCPv6:
                        temp = Comms_General.ConvertToPackets(tempMessage.ToUserID.Machine.IPAddressv6, tempMessage.Data);
                        foreach (MessagePacket Packet in temp)
                        {
                            Socket clientSock = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                            try //Will this work?
                            {
                                clientSock.Connect(Packet.Address, tempMessage.Port.ToInt());
                                clientSock.Send(Packet.ToByteArray());
                            }
                            catch
                            {
                                SendError?.Invoke("Could not send TCPv6 Message to " + Packet.Address);
                            }
                            finally
                            {
                                clientSock.Close();
                            }
                            Thread.Sleep(1);
                        }
                        break;
                    case ConnectionProtocol.UDP:
                        UdpClient client = new UdpClient();
                        temp = Comms_General.ConvertToPackets(tempMessage.ToUserID.Machine.IPAddress, tempMessage.Data);
                        foreach (MessagePacket Packet in temp)
                        {
                            try
                            {
                                client.Send(Packet.ToByteArray(), Packet.ToByteArray().Length, new IPEndPoint(IPAddress.Broadcast, tempMessage.Port.ToInt()));
                                Thread.Sleep(1);
                            }
                            catch
                            {
                                SendError?.Invoke("Could not send UDP Message to " + Packet.Address);
                            }
                        }
                        client.Close();
                        break;
                }
                RemoveFromQueue();
            }
        }
    }
}
