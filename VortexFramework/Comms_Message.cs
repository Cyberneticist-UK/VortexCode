using System;
using System.Text;

namespace Vortex
{
    /// <summary>
    /// This is the main construct that is used for an application to send a message to another person or window.
    /// </summary>
    public struct TransportMessage
    {
        public ConnectionProtocol Protocol;
        public TransportPort Port;
        public Comms_SystemUser ToUserID;
        public MessageData Data;
        public TransportMessage(ConnectionProtocol Protocol, TransportPort Port, MessageData Data, Comms_SystemUser ToUserID)
        {
            this.Protocol = Protocol;
            this.Port = Port;
            this.Data = Data;
            this.ToUserID = ToUserID;
        }

        public TransportMessage(ConnectionProtocol Protocol, TransportPort Port, MessageData Data)
        {
            this.Protocol = Protocol;
            this.Port = Port;
            this.Data = Data;
            this.ToUserID = new Comms_SystemUser();
        }

        public TransportMessage(ConnectionProtocol Protocol, int Port, MessageData Data)
        {
            this.Protocol = Protocol;
            this.Port = Port;
            this.Data = Data;
            this.ToUserID = new Comms_SystemUser();
        }

        public TransportMessage(ConnectionProtocol Protocol, Guid Port, MessageData Data)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = new Comms_SystemUser();
        }

        public TransportMessage(ConnectionProtocol Protocol, string Port, MessageData Data)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = new Comms_SystemUser();
        }

        public TransportMessage(ConnectionProtocol Protocol, int Port, string Data)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = new Comms_SystemUser();
        }

        public TransportMessage(ConnectionProtocol Protocol, Guid Port, string Data)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = new Comms_SystemUser();
        }

        public TransportMessage(ConnectionProtocol Protocol, string Port, string Data)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = new Comms_SystemUser();
        }


        public TransportMessage(ConnectionProtocol Protocol, int Port, MessageData Data, Comms_SystemUser ToUserID)
        {
            this.Protocol = Protocol;
            this.Port = Port;
            this.Data = Data;
            this.ToUserID = ToUserID;
        }

        public TransportMessage(ConnectionProtocol Protocol, Guid Port, MessageData Data, Comms_SystemUser ToUserID)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = ToUserID;
        }

        public TransportMessage(ConnectionProtocol Protocol, string Port, MessageData Data, Comms_SystemUser ToUserID)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = ToUserID;
        }

        public TransportMessage(ConnectionProtocol Protocol, int Port, string Data, Comms_SystemUser ToUserID)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = ToUserID;
        }

        public TransportMessage(ConnectionProtocol Protocol, Guid Port, string Data, Comms_SystemUser ToUserID)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = ToUserID;
        }

        public TransportMessage(ConnectionProtocol Protocol, string Port, string Data, Comms_SystemUser ToUserID)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = ToUserID;
        }

        public TransportMessage(ConnectionProtocol Protocol, int Port, MessageData Data, string IPAddress)
        {
            this.Protocol = Protocol;
            this.Port = Port;
            this.Data = Data;
            this.ToUserID = new Comms_SystemUser(IPAddress, IPAddress, null);
        }

        public TransportMessage(ConnectionProtocol Protocol, Guid Port, MessageData Data, string IPAddress)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = new Comms_SystemUser(IPAddress, IPAddress, null);
        }

        public TransportMessage(ConnectionProtocol Protocol, string Port, MessageData Data, string IPAddress)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = new Comms_SystemUser(IPAddress, IPAddress, null);
        }

        public TransportMessage(ConnectionProtocol Protocol, int Port, string Data, string IPAddress)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = new Comms_SystemUser(IPAddress, IPAddress, null);
        }

        public TransportMessage(ConnectionProtocol Protocol, Guid Port, string Data, string IPAddress)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = new Comms_SystemUser(IPAddress, IPAddress, null);
        }

        public TransportMessage(ConnectionProtocol Protocol, string Port, string Data, string IPAddress)
        {
            this.Protocol = Protocol;
            this.Port = new TransportPort(Port);
            this.Data = Data;
            this.ToUserID = new Comms_SystemUser(IPAddress, IPAddress, null);
        }
    }
    
    /// <summary>
    /// The Data for a message is stored in a single structure for efficient sending over the network
    /// </summary>
    public struct MessageData
    {
        public MessageFormat Format;
        public CommandList Command;
        public Guid WindowGUID;
        public Guid SessionGUID;
        public ErrorCode ErrorCode;
        /// <summary>
        /// Get or set the message data as raw binary data
        /// </summary>
        public byte[] Data;
        public MessageData(MessageFormat Format = MessageFormat.Unicode)
        {
            this.Format = Format;
            Command = 0;
            WindowGUID = Guid.Empty;
            SessionGUID = Guid.Empty;
            ErrorCode = ErrorCode.No_Error;
            switch(this.Format)
            {
                case MessageFormat.Binary:
                    Data = new MessageDataBinary();
                    break;
                case MessageFormat.Bitmap:
                    Data = new MessageDataBitmap();
                    break;
                case MessageFormat.Node:
                    Data = new MessageDataNode();
                    break;
                case MessageFormat.Unicode:
                    Data = new MessageDataUnicode();
                    break;
                default:
                    Data = new MessageDataASCII();
                    break;
            }
        }

        public static implicit operator MessageData(byte[] data)
        {
            MessageData temp = new MessageData();
            if (data.Length > 0)
            {
                Encoding ascii = Encoding.ASCII;
                MessageFormat Format = (MessageFormat)data[0];
                temp.Format = Format;
                temp.Command = (CommandList)data[1];
                byte[] a = new byte[16];
                System.Buffer.BlockCopy(data, 2, a, 0, 16);
                temp.WindowGUID = new Guid(a);
                System.Buffer.BlockCopy(data, 18, a, 0, 16);
                temp.SessionGUID = new Guid(a);
                // Now for the main data:
                temp.Data = new byte[data.Length - 34];
                System.Buffer.BlockCopy(data, 34, temp.Data, 0, data.Length - 34);
            }
            return temp;
        }

        public static implicit operator MessageData(string data)
        {
            MessageData temp = new MessageData();
            temp.DataString = data;
            return temp;
        }

        public static implicit operator byte[] (MessageData data)
        {
            if (data.Data == null)
                data.Data = new byte[0];
            // Convert the message to bytes:
            // First two bytes are the format and the command:
            byte[] Comm = new byte[] { Convert.ToByte((int)data.Format), Convert.ToByte((int)data.Command) };
            // The output is the command and format length, plus the 2 GUID, then the data:
            byte[] output = new byte[Comm.Length + 32 + data.Data.Length];
            // Copy the command:
            System.Buffer.BlockCopy(Comm, 0, output, 0, Comm.Length);
            // Next 16 bytes are the Window GUID
            System.Buffer.BlockCopy(data.WindowGUID.ToByteArray(), 0, output, Comm.Length, 16);
            // Next 16 bytes are the Session GUID
            System.Buffer.BlockCopy(data.SessionGUID.ToByteArray(), 0, output, Comm.Length + 16, 16);
            // Finally we add in the data:
            System.Buffer.BlockCopy(data.Data, 0, output, Comm.Length+32, data.Data.Length);
            return output;
        }

        /// <summary>
        /// Get or set the message data as a string
        /// </summary>
        public string DataString
        {
            get
            {
                switch (Format)
                {
                    default:
                        return Encoding.ASCII.GetString(Data);
                    case MessageFormat.Unicode:
                        return Encoding.Unicode.GetString(Data);
                }
            }
            set
            {
                switch (Format)
                {
                    default:
                        if (Comms_General.CanDataBeAscii((string)value, out string X) == false)
                        {
                            Format = MessageFormat.Unicode;
                            Data = Encoding.Unicode.GetBytes(value);
                        }
                        else
                            Data = Encoding.ASCII.GetBytes(value);
                        break;
                    case MessageFormat.Unicode:
                        Data = Encoding.Unicode.GetBytes(value);
                        break;
                }
            }
        }
    }

    public struct MessagePacket
    {
        public Guid MessageGuid;
        public int MessageNumber;
        public int TotalMessages;
        public byte[] Data;
        /// <summary>
        /// Note that the address doesn't get sent through with the message, just used to send the packets
        /// </summary>
        public string Address;

        public MessagePacket(string Address, Guid MessageGuid, int MessageNumber, int TotalMessages, byte[] Data)
        {
            this.Address = Address;
            this.MessageGuid = MessageGuid;
            this.MessageNumber = MessageNumber;
            this.TotalMessages = TotalMessages;
            this.Data = Data;
        }

        public static implicit operator MessagePacket(byte[] data)
        {
            MessagePacket temp = new MessagePacket();
            byte[] number = new byte[4];
            byte[] Guid = new byte[16];
            System.Buffer.BlockCopy(data, 0, Guid, 0, 16);
            temp.MessageGuid = new System.Guid(Guid);

            temp.MessageNumber = BitConverter.ToInt32(data, 16);
            temp.TotalMessages = BitConverter.ToInt32(data, 20);

            temp.Data = new byte[data.Length - 24];
            System.Buffer.BlockCopy(data, 24, temp.Data, 0, data.Length-24);
            
            return temp;
        }

        public byte[] ToByteArray()
        {
            byte[] number = BitConverter.GetBytes(MessageNumber);
            byte[] total = BitConverter.GetBytes(TotalMessages);

            byte[] output = new byte[16 + number.Length + total.Length + Data.Length];
            System.Buffer.BlockCopy(MessageGuid.ToByteArray(), 0, output, 0, 16);

            System.Buffer.BlockCopy(number, 0, output, 16, number.Length);

            System.Buffer.BlockCopy(total, 0, output, 16 + number.Length, total.Length);

            System.Buffer.BlockCopy(Data, 0, output, 16 + number.Length + total.Length, Data.Length);
            return output;
        }
    }

    public struct MessageDataASCII
    {
        public byte Command;
        public string Data;
        public ErrorCode ErrorCode;

        public static implicit operator MessageDataASCII(byte[] data)
        {
            MessageDataASCII temp = new MessageDataASCII();
            Encoding ascii = Encoding.ASCII;
            byte Type = data[0];
            switch (Type)
            {
                case 0:
                    // This is ASCII:
                    temp.Command = data[1];
                    byte[] output = new byte[data.Length - 2];
                    System.Buffer.BlockCopy(data, 2, output, 0, data.Length - 2);
                    temp.Data = ascii.GetString(output);
                    break;
                case 1:
                    MessageDataUnicode tmp = data;
                    temp = tmp.ToASCII();
                    break;
            }
            return temp;
        }

        public static implicit operator byte[] (MessageDataASCII data)
        {
            // Convert the message to bytes:
            Encoding ascii = Encoding.ASCII;
            byte[] Comm = new byte[] { 0, data.Command };
            byte[] stringData = ascii.GetBytes(data.Data);
            byte[] output = new byte[Comm.Length + stringData.Length];
            System.Buffer.BlockCopy(Comm, 0, output, 0, Comm.Length);
            System.Buffer.BlockCopy(stringData, 0, output, Comm.Length, stringData.Length);
            return output;
        }

        public MessageDataASCII(int Command, string Data)
        {
            this.Command = Convert.ToByte(Command);
            this.Data = Data;
            ErrorCode = ErrorCode.No_Error;
        }

        public MessageDataASCII(CommandList Command, string Data)
        {
            this.Command = Convert.ToByte((int)Command);
            this.Data = Data;
            ErrorCode = ErrorCode.No_Error;
        }

    }

    public struct MessageDataUnicode
    {
        public byte Command;
        public string Data;
        public ErrorCode ErrorCode;

        public MessageDataASCII ToASCII()
        {
            MessageDataASCII temp = new MessageDataASCII();
            temp.Command = Command;
            if (Comms_General.CanDataBeAscii(Data, out temp.Data) == false)
                temp.ErrorCode = ErrorCode.Message_Is_Unicode;
            else
                temp.ErrorCode = ErrorCode.No_Error;
            return temp;
        }

        public static implicit operator MessageDataUnicode(byte[] data)
        {
            MessageDataUnicode temp = new MessageDataUnicode();
            Encoding uni = Encoding.Unicode;
            byte Type = data[0];
            switch (Type)
            {
                case 1:
                    // This is Unicode:
                    temp.Command = data[1];
                    byte[] output = new byte[data.Length - 2];
                    System.Buffer.BlockCopy(data, 2, output, 0, data.Length - 2);
                    temp.Data = uni.GetString(output);
                    break;
                case 0:
                    // This is ASCII:
                    MessageDataASCII tmp = data;
                    temp.Command = tmp.Command;
                    temp.Data = tmp.Data;
                    temp.ErrorCode = tmp.ErrorCode;
                    break;
            }
            return temp;
        }

        public static implicit operator byte[] (MessageDataUnicode data)
        {
            // Convert the message to bytes:
            Encoding uni = Encoding.Unicode;
            byte[] Comm = new byte[] { 1, data.Command };
            byte[] stringData = uni.GetBytes(data.Data);
            byte[] output = new byte[Comm.Length + stringData.Length];
            System.Buffer.BlockCopy(Comm, 0, output, 0, Comm.Length);
            System.Buffer.BlockCopy(stringData, 0, output, Comm.Length, stringData.Length);
            return output;
        }


        public MessageDataUnicode(int Command, string Data)
        {
            this.Command = Convert.ToByte(Command);
            this.Data = Data;
            ErrorCode = ErrorCode.No_Error;
        }

        public MessageDataUnicode(CommandList Command, string Data)
        {
            this.Command = Convert.ToByte((int)Command);
            this.Data = Data;
            ErrorCode = ErrorCode.No_Error;
        }

    }

    /// <summary>
    /// Note - currently not built!!
    /// </summary>
    public struct MessageDataNode
    {
        public byte Command;
        public string Data;
        public ErrorCode ErrorCode;

        public static implicit operator MessageDataNode(byte[] data)
        {
            MessageDataNode temp = new MessageDataNode();
            Encoding ascii = Encoding.ASCII;
            byte Type = data[0];
            switch (Type)
            {
                case 0:
                    // This is ASCII:
                    temp.Command = data[1];
                    byte[] output = new byte[data.Length - 2];
                    System.Buffer.BlockCopy(data, 2, output, 0, data.Length - 2);
                    temp.Data = ascii.GetString(output);
                    break;
            }
            return temp;
        }

        public static implicit operator byte[] (MessageDataNode data)
        {
            // Convert the message to bytes:
            Encoding ascii = Encoding.ASCII;
            byte[] Comm = new byte[] { 0, data.Command };
            byte[] stringData = ascii.GetBytes(data.Data);
            byte[] output = new byte[Comm.Length + stringData.Length];
            System.Buffer.BlockCopy(Comm, 0, output, 0, Comm.Length);
            System.Buffer.BlockCopy(stringData, 0, output, Comm.Length, stringData.Length);
            return output;
        }

        public MessageDataNode(int Command, string Data)
        {
            this.Command = Convert.ToByte(Command);
            this.Data = Data;
            ErrorCode = ErrorCode.No_Error;
        }

        public MessageDataNode(CommandList Command, string Data)
        {
            this.Command = Convert.ToByte((int)Command);
            this.Data = Data;
            ErrorCode = ErrorCode.No_Error;
        }

    }

    /// <summary>
    /// Note - currently not built!!
    /// </summary>
    public struct MessageDataBitmap
    {
        public byte Command;
        public string Data;
        public ErrorCode ErrorCode;

        public static implicit operator MessageDataBitmap(byte[] data)
        {
            MessageDataBitmap temp = new MessageDataBitmap();
            Encoding ascii = Encoding.ASCII;
            byte Type = data[0];
            switch (Type)
            {
                case 0:
                    // This is ASCII:
                    temp.Command = data[1];
                    byte[] output = new byte[data.Length - 2];
                    System.Buffer.BlockCopy(data, 2, output, 0, data.Length - 2);
                    temp.Data = ascii.GetString(output);
                    break;
            }
            return temp;
        }

        public static implicit operator byte[] (MessageDataBitmap data)
        {
            // Convert the message to bytes:
            Encoding ascii = Encoding.ASCII;
            byte[] Comm = new byte[] { 0, data.Command };
            byte[] stringData = ascii.GetBytes(data.Data);
            byte[] output = new byte[Comm.Length + stringData.Length];
            System.Buffer.BlockCopy(Comm, 0, output, 0, Comm.Length);
            System.Buffer.BlockCopy(stringData, 0, output, Comm.Length, stringData.Length);
            return output;
        }

        public MessageDataBitmap(int Command, string Data)
        {
            this.Command = Convert.ToByte(Command);
            this.Data = Data;
            ErrorCode = ErrorCode.No_Error;
        }

        public MessageDataBitmap(CommandList Command, string Data)
        {
            this.Command = Convert.ToByte((int)Command);
            this.Data = Data;
            ErrorCode = ErrorCode.No_Error;
        }

    }

    /// <summary>
    /// Note - currently not built!!
    /// </summary>
    public struct MessageDataBinary
    {
        public byte Command;
        public string Data;
        public ErrorCode ErrorCode;

        public static implicit operator MessageDataBinary(byte[] data)
        {
            MessageDataBinary temp = new MessageDataBinary();
            Encoding ascii = Encoding.ASCII;
            byte Type = data[0];
            switch (Type)
            {
                case 0:
                    // This is ASCII:
                    temp.Command = data[1];
                    byte[] output = new byte[data.Length - 2];
                    System.Buffer.BlockCopy(data, 2, output, 0, data.Length - 2);
                    temp.Data = ascii.GetString(output);
                    break;
            }
            return temp;
        }

        public static implicit operator byte[] (MessageDataBinary data)
        {
            // Convert the message to bytes:
            Encoding ascii = Encoding.ASCII;
            byte[] Comm = new byte[] { 0, data.Command };
            byte[] stringData = ascii.GetBytes(data.Data);
            byte[] output = new byte[Comm.Length + stringData.Length];
            System.Buffer.BlockCopy(Comm, 0, output, 0, Comm.Length);
            System.Buffer.BlockCopy(stringData, 0, output, Comm.Length, stringData.Length);
            return output;
        }

        public MessageDataBinary(int Command, string Data)
        {
            this.Command = Convert.ToByte(Command);
            this.Data = Data;
            ErrorCode = ErrorCode.No_Error;
        }

        public MessageDataBinary(CommandList Command, string Data)
        {
            this.Command = Convert.ToByte((int)Command);
            this.Data = Data;
            ErrorCode = ErrorCode.No_Error;
        }

    }

}
