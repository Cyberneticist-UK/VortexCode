using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;

namespace Vortex
{
    /// <summary>
    /// A series of functions that are useful in general Networking code in Netelligence!
    /// </summary>
    public static class Comms_General
    {
        /// <summary>
        /// Methods to make sure every required element is included when creating a specific type of connection
        /// </summary>
        public static class Connections
        {
            /// <summary>
            /// Create a point to point message
            /// </summary>
            /// <param name="Protocol">TCP or TCPv6</param>
            /// <param name="Command">Command to send</param>
            /// <param name="MessageData">Data To Send</param>
            /// <param name="ToPerson">the person to send it to</param>
            /// <param name="Port">Which numeric port to use</param>
            /// <returns>Transport Message ready for sending</returns>
            public static TransportMessage CreateTCPStyleMessage(ConnectionProtocol Protocol, CommandList Command, string MessageData, Comms_SystemUser ToPerson, int Port)
            {
                TransportMessage Message = MessageDataContent(Command, MessageData);
                Message.ToUserID = ToPerson;
                Message.Port = Port;
                Message.Protocol = Protocol;
                return Message;
            }

            /// <summary>
            /// Create a broadcast message
            /// </summary>
            /// <param name="Protocol">UDP or UDPv6</param>
            /// <param name="Command">Command to send</param>
            /// <param name="MessageData">Data To Send</param>
            /// <param name="Port">Which numeric port to use</param>
            /// <returns>Transport Message ready for sending</returns>
            public static TransportMessage CreateUDPStyleMessage(ConnectionProtocol Protocol, CommandList Command, string MessageData, int Port)
            {
                TransportMessage Message = MessageDataContent(Command, MessageData);
                Message.Port = Port;
                Message.Protocol = Protocol;
                return Message;
            }
            
            /// <summary>
            /// Create a message for the web server (I dont think this is used yet!)
            /// </summary>
            /// <param name="Command"></param>
            /// <param name="MessageData"></param>
            /// <param name="ToPerson"></param>
            /// <param name="Port"></param>
            /// <returns></returns>
            public static TransportMessage CreateWebServerMessage(CommandList Command, string MessageData, Comms_SystemUser ToPerson, string Port)
            {
                TransportMessage Message = MessageDataContent(Command, MessageData);
                Message.ToUserID = ToPerson;
                Message.Port = Port;
                Message.Protocol = ConnectionProtocol.WebServer;
                return Message;
            }

            static TransportMessage MessageDataContent(CommandList Command, string MessageData)
            {
                TransportMessage Message = new TransportMessage();
                Message.Data.Command = Command;
                Message.Data.DataString = MessageData;
                return Message;
            }
        }

        /// <summary>
        /// Describes what a command is used for
        /// </summary>
        /// <param name="Command">The Command to Describe</param>
        /// <returns>a human readable description</returns>
        public static string Description(CommandList Command)
        {
            switch(Command)
            {
                case CommandList.ActivateMesh:
                    return "Sent over the UDP network when you come online to the Mesh";
                case CommandList.DeactivateMesh:
                    return "Sent over the UDP network when you go offline to the Mesh";
                case CommandList.UpdateMesh:
                    return "Sent over the UDP network when your details change to update the Personal details";
                case CommandList.YouAreMaster:
                    return "Sent to a slave machine to let them know they are now in charge of the mesh";
                case CommandList.YouAreSlave:
                    return "Sent to a slave machine by the master machine to let them know they are just a pawn in the game";
                case CommandList.InternalError:
                    return "Not really shared with the system utilising the library, but can be used to report an error to the system";
                case CommandList.Chat:
                    return "Sending a message from one user to another";
                case CommandList.EncryptedChat:
                    return "As Chat, but this message is encrypted over the comms channel";
                default:
                    return "No Description Found!";
            }
        }

        /// <summary>
        /// Describes what a Protocol is used for
        /// </summary>
        /// <param name="Protocol">The Protocol to Describe</param>
        /// <returns>a human readable description</returns>
        public static string Description(ConnectionProtocol Protocol)
        {
            switch (Protocol)
            {
                case ConnectionProtocol.NotSet:
                    return "We haven't yet set the connection protocol - messages won't be sent via this";
                case ConnectionProtocol.Self:
                    return "Not sent through a comms channel, just sent from one window to another on the same computer";
                case ConnectionProtocol.TCP:
                    return "Uses IP Version 4 to send messages via TCP/IP";
                case ConnectionProtocol.TCPv6:
                    return "Uses IP Version 6 to send messages via TCP/IP";
                case ConnectionProtocol.UDP:
                    return "Uses IP Version 4 to send messages via UDP";
                case ConnectionProtocol.UDPv6:
                    return "Uses IP Version 6 to send messages via UDP";
                case ConnectionProtocol.WebClient:
                    return "Is used to download a file from a URL";
                case ConnectionProtocol.WebServer:
                    return "Not sure now!";
                case ConnectionProtocol.WebService:
                    return "Not sure now!";
                default:
                    return "No Description Found!";
            }
        }


        /// <summary>
        /// Code for whether the connection is encrypted or not
        /// </summary>
        public static class EncryptedChannel
        {
            /// <summary>
            /// Encryption is performed based on the Command name - only certain commands need to be encrypted.
            /// </summary>
            /// <param name="Command">The command to check</param>
            /// <returns>True if this data needs encrypting</returns>
            public static bool CommandEncrypted(CommandList Command)
            {
                if (Command == CommandList.ActivateMesh || 
                    Command == CommandList.DeactivateMesh || 
                    Command == CommandList.UpdateMesh || 
                    Command == CommandList.EncryptedChat ||
                    Command == CommandList.EncryptedText)
                    return true;
                return false;
            }
            
            /// <summary>
            /// This is the Current Encryption String for encrypted message types.
            /// </summary>
            public static string PacketConfuse
            {
                get
                {
                    System.Security.SecureString _EncryptionKey = new System.Security.SecureString();
                    _EncryptionKey.AppendChar('P');
                    _EncryptionKey.AppendChar('o');
                    _EncryptionKey.AppendChar('R');
                    _EncryptionKey.AppendChar('t');
                    _EncryptionKey.AppendChar('A');
                    _EncryptionKey.AppendChar('l');
                    _EncryptionKey.AppendChar('W');
                    _EncryptionKey.AppendChar('o');
                    _EncryptionKey.AppendChar('M');
                    _EncryptionKey.AppendChar('b');
                    _EncryptionKey.AppendChar('A');
                    _EncryptionKey.AppendChar('t');
                    _EncryptionKey.AppendChar('C');
                    _EncryptionKey.AppendChar('h');
                    _EncryptionKey.AppendChar('A');
                    _EncryptionKey.AppendChar('i');
                    _EncryptionKey.AppendChar('R');
                    _EncryptionKey.AppendChar('n');
                    _EncryptionKey.AppendChar('E');
                    _EncryptionKey.AppendChar('o');
                    return _EncryptionKey.ToString();
                }
            }
        }

        /// <summary>
        /// Checks to see whether the port is already currently being used for communication
        /// </summary>
        /// <param name="Port">Port number to check</param>
        /// <returns>True if in use</returns>
        public static bool PortInUse(int Port)
        {
            return (from p in System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners() where p.Port == Port select p).Count() == 1;
        }

        /// <summary>
        /// Checks to see if there is a network associated with this computer and therefore LAN connectivity
        /// </summary>
        public static bool LocalNetwork
        {
            get
            {
                return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            }
        }

        /// <summary>
        /// Checks to see if there is currently internet access with this machine and therefore Internet Connectivity
        /// </summary>
        public static bool InterNetwork
        {
            get
            {
                // Use PING to see if we can query the Google DNS server:
                System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
                try
                {
                    if (p.Send("8.8.8.8").Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        return true;
                    }
                    else
                        return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Opens a file and turns it into a byte array - perfect for sending through the network with!
        /// </summary>
        /// <param name="filePath">The file to turn into a byte stream</param>
        /// <returns>A byte stream for the file</returns>
        public static byte[] ReadFile(string filePath)
        {
            byte[] buffer = new byte[0];
            FileStream fileStream;
            try
            {
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                int length = (int)fileStream.Length;  // get file length
                buffer = new byte[length];            // create buffer
                int count;                            // actual number of bytes read
                int sum = 0;                          // total number of bytes read

                // read until Read method returns 0 (end of the stream has been reached)
                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  // sum is a buffer offset for next reading
                fileStream.Close();
            }
            catch (Exception err)
            {
                string Y = err.ToString();
            }
            finally
            {
                //fileStream.Close();
            }
            return buffer;
        }

        /// <summary>
        /// Gets the basic network information about the current machine. IP address in IPv4 and v6 format.
        /// </summary>
        /// <returns>A Comms System User object prefilled with the Machine and IP</returns>
        public static Comms_SystemUser GetMe()
        {
            Comms_SystemUser temp = new Comms_SystemUser(GetMyIP(), Environment.MachineName, Environment.UserName);
            temp.Machine.IPAddressv6 = GetMyIPv6();
            return temp;
        }

        /// <summary>
        /// This finds out the IP Address (Version 4) of the current computer
        /// </summary>
        /// <returns>An IPv4 Address for your machine</returns>
        public static string GetMyIP()
        {
            string IPAddress = "";
            try
            {
                string strHostName = Dns.GetHostName();
                // Find host by name
                IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
                // Get the IPv4 address for this computer on the local computer:
                foreach (IPAddress ipaddress in iphostentry.AddressList)
                {
                    if (ipaddress.ToString().IndexOf(":") == -1)
                        //if (ipaddress.MapToIPv4().ToString() == ipaddress.ToString())
                        IPAddress = ipaddress.ToString();
                }
            }
            catch (Exception err)
            {
                return "ERROR:" + err.Message;
            }
            return IPAddress;
        }
        
        /// <summary>
        /// This finds out the IP Address (Version 6) of the current computer
        /// </summary>
        /// <returns>An IPv6 Address for your machine</returns>
        public static string GetMyIPv6()
        {
            string IPAddress = "";
            try
            {
                string strHostName = Dns.GetHostName();
                // Find host by name
                IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
                // Get the IPv6 address for this computer on the local computer:
                foreach (IPAddress ipaddress in iphostentry.AddressList)
                {
                    if (ipaddress.ToString().IndexOf(":") != -1)
                        //if (ipaddress.MapToIPv4().ToString() == ipaddress.ToString())
                        IPAddress = ipaddress.ToString();
                }
            }
            catch (Exception err)
            {
                return "ERROR:" + err.Message;
            }
            return IPAddress;
        }

        /// <summary>
        /// Is the string of data able to work as ASCII or does it need sending as Unicode:
        /// </summary>
        /// <param name="MessageData">The string to look at</param>
        /// <returns>True if ASCII, False if Unicode</returns>
        public static bool CanDataBeAscii(string MessageData, out string ASCIIData)
        {
            // Convert the message to bytes:
            Encoding ascii = Encoding.ASCII;
            byte[] stringDataUni = ascii.GetBytes(MessageData);
            ASCIIData = ascii.GetString(stringDataUni);
            return ASCIIData == MessageData;
        }

        /// <summary>
        /// Checks to see if all the packets are for the same message and all received
        /// </summary>
        /// <param name="Packets">The packets to check</param>
        /// <returns>The error message</returns>
        public static ErrorCode CheckPackets(List<MessagePacket> Packets)
        {
            // Check to see if the GUID's all match: GUID_Wrong_In_List

            // check to see if there is a packet number missing:
            byte[] PacketHere = GetPacketList(Packets);
            Guid Guid = Packets[0].MessageGuid;
            foreach(MessagePacket P in Packets)
            {
                // Check here that the GUID's are all correct too!
                if (P.MessageGuid != Guid)
                    return ErrorCode.GUID_Wrong_In_List;
            }
            for (int i = 0; i < PacketHere.Length; i++)
            {
                if(PacketHere[i] == 0)
                {
                    return ErrorCode.Packet_Missing;
                }
            }
            return ErrorCode.No_Error;
        }

        /// <summary>
        /// Returns a byte array with each position representing a packet. if it is a "0" that packet hasn't been received yet.
        /// A "1" in that location means it has. e.g. 01111 would mean a message has been split into 5 packets and we are still
        /// waiting for packet 0.
        /// </summary>
        /// <param name="Packets"></param>
        /// <returns></returns>
        public static byte[] GetPacketList(List<MessagePacket> Packets)
        {
            byte[] PacketHere = new byte[Packets[0].TotalMessages];
            for (int i = 0; i < PacketHere.Length; i++)
            {
                PacketHere[i] = 0;
            }
            foreach (MessagePacket P in Packets)
            {
                PacketHere[P.MessageNumber] = 1;
            }
            return PacketHere;
        }

        /// <summary>
        /// Works out which packet is missing in a list
        /// </summary>
        /// <param name="Packets"></param>
        /// <returns></returns>
        public static int MissingPacketNumber(List<MessagePacket> Packets)
        {
            // check to see if there is a packet number missing:
            byte[] PacketHere = GetPacketList(Packets);

            for (int i = 0; i < PacketHere.Length; i++)
            {
                if (PacketHere[i] == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Convert a single message in bytes into a set of packets to send to the address given
        /// </summary>
        /// <param name="Address">The IP Address, either V4 or V6</param>
        /// <param name="Message">The data to send</param>
        /// <returns>A list of packets to use</returns>
        public static List<MessagePacket> ConvertToPackets(string Address, byte[] Message)
        {
            // Convert the message to bytes packets:
            List<MessagePacket> Packets = new List<MessagePacket>();
            // 1024 is the buffer size, but 24 bytes is the header:
            // 16 bytes GUID, 4 bytes packet number, 4 bytes total packets
            Guid MessageGuid = System.Guid.NewGuid();

            int TotalPackets = (Message.Length / 1000);
            if ((Message.Length % 1000) > 0)
            {
                TotalPackets++;
            }
            for (int i = 0; i < TotalPackets; i++)
            {
                byte[] Data;
                if ((i + 1) * 1000 < Message.Length)
                {
                    Data = new byte[1000];
                    System.Buffer.BlockCopy(Message, i * 1000, Data, 0, Data.Length);
                }
                else
                {
                    Data = new byte[Message.Length - (i * 1000)];
                    System.Buffer.BlockCopy(Message, i * 1000, Data, 0, Data.Length);
                }
                Packets.Add(new MessagePacket(Address, MessageGuid, i, TotalPackets, Data));
            }
            return Packets;
        }


        /// <summary>
        /// Convert a series of packets back into a single message, in a byte array format
        /// </summary>
        /// <param name="Packets"></param>
        /// <returns></returns>
        public static byte[] ConvertToMessage(List<MessagePacket> Packets)
        {
            if (Packets == null)
                return new byte[0];
            // Will assume here that have checked the packets are all correct!
            MessagePacket[] FullList = new MessagePacket[Packets.Count];
            Int64 TotalSize = 0;
            for (int i = 0; i < Packets.Count; i++)
            {
                FullList[Packets[i].MessageNumber] = Packets[i];
                TotalSize += Packets[i].Data.Length;
            }
            int Position = 0;
            byte[] output = new byte[TotalSize];
            for (int i = 0; i < Packets.Count; i++)
            {
                System.Buffer.BlockCopy(Packets[i].Data, 0, output, Position, Packets[i].Data.Length);
                Position += Packets[i].Data.Length;
            }
            return output;
        }

        /// <summary>
        /// Convert a single packet back into a single message, in a byte array format
        /// </summary>
        /// <param name="Packets"></param>
        /// <returns></returns>
        public static byte[] ConvertToMessage(MessagePacket Packet)
        {
            // Will assume here that have checked the packets are all correct!
            byte[] output = new byte[Packet.Data.Length];
            System.Buffer.BlockCopy(Packet.Data, 0, output, 0, Packet.Data.Length);
            return output;
        }


    }
}
