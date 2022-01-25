using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Vortex
{
    /// <summary>
    /// This is the main class that an application uses to connect to the network.
    /// Version 1.0
    /// 27/02/2018
    /// </summary>
    public class Comms_Connect
    {
        /// <summary>
        /// 
        /// </summary>
        public event MessageTransfer MessageReceived;
        public event WebServerSentMessage WebMessageReceived;
        public event StringTransfer ErrorRaised;
        public event BlankTransfer MeshUpdate;
        List<Comms_Interface_Connection> Connections = new List<Comms_Interface_Connection>();
        public List<string> ErrorList = new List<string>();
        public string LastError
        {
            get { return ErrorList[ErrorList.Count - 1]; }
        }
        
        public int NumberOfConnections { get { return Connections.Count; } }
        Comms_Sender Sender = new Comms_Sender();
        //Comms_Mesh Mesh;// = new Comms_Mesh();
        Comms_SystemUser _Me = Comms_General.GetMe();
        Comms_Interface_Connection BlankComms = new Comms_Blank(0);

        public Comms_SystemUser Me
        {
            get { return _Me; }
            set { _Me = value; }
        }

        public string CurrentIP;
        Timer time = new Timer(15000);

        public Comms_Connect(bool RunMesh = false)
        {
            BlankComms.MessageReceived += Comms_MessageReceived;
            Sender.SendError += Sender_SendError;
            CurrentIP = Comms_General.GetMyIP();
        }

        private void Time_Elapsed(object sender, ElapsedEventArgs e)
        {
            time.Enabled = false;
            time.Enabled = true;
        }

        private void Sender_SendError(string Message)
        {
            ErrorList.Add(Message);
        }

        private void Mesh_MeshUpdate()
        {
            MeshUpdate?.Invoke();
        }

        /// <summary>
        /// Generates a list of current connections to view on screen
        /// </summary>
        /// <returns></returns>
        public List<string> CurrentConnections()
        {
            List<string> Result = new List<string>();
            foreach (Comms_Interface_Connection c in Connections)
                Result.Add(c.ToString());
            return Result;
        }

        /// <summary>
        /// Send a message to an underlying connection. If the connection protocol/port doesn't exist, it will be created and opened.
        /// </summary>
        /// <param name="Message">The Full Transport Message to Send Out</param>
        /// <returns></returns>
        public bool SendMessage(TransportMessage Message)
        {
            if (CheckSendMessage(Message) == true)
            {
                if (Message.Protocol == ConnectionProtocol.Self)
                    MessageReceived?.Invoke(Message.Data);
                else
                    Sender.InputQueue(Message);
                return true;
            }
            else
            {
                ErrorRaised?.Invoke(LastError);
                return false;
            }
        }

        private bool CheckSendMessage(TransportMessage Message)
        {
            // Don't allow internal message commands:
            if (Message.Data.Command == CommandList.InternalError)
            {
                ErrorList.Add(System.DateTime.Now.ToString() + " - Cannot use InternalError command (Protected)");
                return false;
            }
            switch (Message.Protocol)
            {
                case ConnectionProtocol.TCP:
                    if (Message.Port.IsPort() == false)
                    {
                        ErrorList.Add(System.DateTime.Now.ToString() + " - TCP Message Requires a Port Number");
                        return false;
                    }
                    if (Message.ToUserID.Machine.IPAddress == null)
                    {
                        ErrorList.Add(System.DateTime.Now.ToString() + " - TCP Message Requires a ToUserID with a v4 IP Address");
                        return false;
                    }
                    break;
                case ConnectionProtocol.TCPv6:
                    if (Message.Port.IsPort() == false)
                    {
                        ErrorList.Add(System.DateTime.Now.ToString() + " - TCPv6 Message Requires a Port Number");
                        return false;
                    }
                    if (Message.ToUserID.Machine.IPAddressv6 == null)
                    {
                        ErrorList.Add(System.DateTime.Now.ToString() + " - TCPv6 Message Requires a ToUserID with a v6 IP Address");
                        return false;
                    }
                    break;
                case ConnectionProtocol.UDP:
                    if (Message.Port.IsPort() == false)
                    {
                        ErrorList.Add(System.DateTime.Now.ToString() + " - UDP Message Requires a Port Number");
                        return false;
                    }
                    //if (Message.ToUserID.Machine.IPAddress == null)
                    //{
                    //    ErrorList.Add(System.DateTime.Now.ToString() + " - UDP Message Requires a ToUserID with a v4 IP Address");
                    //    return false;
                    //}
                    break;
                case ConnectionProtocol.UDPv6:
                    if (Message.Port.IsPort() == false)
                    {
                        ErrorList.Add(System.DateTime.Now.ToString() + " - UDPv6 Message Requires a Port Number");
                        return false;
                    }
                    //if (Message.ToUserID.Machine.IPAddressv6 == null)
                    //{
                    //    ErrorList.Add(System.DateTime.Now.ToString() + " - UDPv6 Message Requires a ToUserID with a v6 IP Address");
                    //    return false;
                    //}
                    break;
                case ConnectionProtocol.WebServer:
                    ErrorList.Add(System.DateTime.Now.ToString() + " - Cannot sent a Webserver a Message - connect using its HTTP address");
                    return false;
                case ConnectionProtocol.NotSet:
                    ErrorList.Add(System.DateTime.Now.ToString() + " - Message hasn't got a protocol set");
                    return false;
                case ConnectionProtocol.WebClient:
                    if (Message.Port.IsURL() == false)
                    {
                        ErrorList.Add(System.DateTime.Now.ToString() + " - WebClient Message Requires a URL set on the Port");
                        return false;
                    }
                    break;
                case ConnectionProtocol.WebService:
                    //if (Message.Port.IsURL() == false)
                    //{
                    //    ErrorList.Add(System.DateTime.Now.ToString() + " - WebService Message Requires a URL set on the Port");
                    //    return false;
                    //}
                    break;
                    

            }
            return true;
        }


        public void Listen(ConnectionProtocol Protocol, TransportPort Port)
        {
            if (CheckListen(Protocol, Port) == true)
                CreateConnection(Protocol, Port);
            else
                ErrorRaised?.Invoke(LastError);
        }

        private bool CheckListen(ConnectionProtocol Protocol, TransportPort Port)
        {
            if(Comms_General.PortInUse(Port) == true)
            {
                ErrorList.Add(System.DateTime.Now.ToString() + " - Port is already in use");
                return false;
            }
            switch (Protocol)
            {
                case ConnectionProtocol.TCP:
                    if (Port.IsPort() == false)
                    {
                        ErrorList.Add(System.DateTime.Now.ToString() + " - TCP Connect Requires a Port Number");
                        return false;
                    }
                    break;
                case ConnectionProtocol.TCPv6:
                    if (Port.IsPort() == false)
                    {
                        ErrorList.Add(System.DateTime.Now.ToString() + " - TCPv6 Connect Requires a Port Number");
                        return false;
                    }
                    break;
                case ConnectionProtocol.UDP:
                    if (Port.IsPort() == false)
                    {
                        ErrorList.Add(System.DateTime.Now.ToString() + " - UDP Connect Requires a Port Number");
                        return false;
                    }
                    break;
                case ConnectionProtocol.WebServer:
                    if (Port.IsGuid() == false)
                    {
                        ErrorList.Add(System.DateTime.Now.ToString() + " - WebServer Connect Requires a Port GUID");
                        return false;
                    }
                    break;
                case ConnectionProtocol.NotSet:
                    ErrorList.Add(System.DateTime.Now.ToString() + " - Connect hasn't got a protocol set");
                    return false;
                case ConnectionProtocol.WebClient:
                    if (Port.IsURL() == false)
                    {
                        ErrorList.Add(System.DateTime.Now.ToString() + " - WebClient Connect Requires a URL set on the Port");
                        return false;
                    }
                    break;
                case ConnectionProtocol.WebService:
                    if (Port.IsURL() == false)
                    {
                        ErrorList.Add(System.DateTime.Now.ToString() + " - WebService Connect Requires a URL set on the Port");
                        return false;
                    }
                    break;
            }
            return true;
        }


        /// <summary>
        /// Create a connection to an external network system
        /// </summary>
        /// <param name="Protocol">The protocol to use</param>
        /// <param name="Port">The port data (int, URL, Guid) for the connection</param>
        /// <returns></returns>
        private Comms_Interface_Connection CreateConnection(ConnectionProtocol Protocol, TransportPort Port)
        {
            Comms_Interface_Connection Comms = null;
            try
            {
                switch (Protocol)
                {
                    case ConnectionProtocol.NotSet:
                        Comms = new Comms_Blank(Port);
                        break;
                    case ConnectionProtocol.UDP:
                        Comms = new Comms_UDP(Port);
                        break;
                    case ConnectionProtocol.UDPv6:
                        Comms = new Comms_UDPv6(Port);
                        break;
                    case ConnectionProtocol.TCP:
                        Comms = new Comms_TCP(Port);
                        break;
                    case ConnectionProtocol.TCPv6:
                        Comms = new Comms_TCPv6(Port);
                        break;
                    case ConnectionProtocol.WebServer:
                        Comms_WebServer temp = new Comms_WebServer(Port);
                        temp.WebMessageReceived += Temp_WebMessageReceived;
                        Comms = temp;
                        break;
                }
            }
            catch (Exception err)
            {
                ErrorList.Add(System.DateTime.Now.ToString() + " - Error in Creating Listening Connection - " + err.Message);
                ErrorRaised?.Invoke(LastError);
            }
            if (Comms != null)
            {
                //if (Connections.Count == 0)
                //    Comms.MessageReceived += Comms_MessageReceived;
                Connections.Add(Comms);
                Me.Connections.Add(new Comms_ConnectionDetail(Comms.ToString()));
            }
            return Comms;
        }

        private byte[] Temp_WebMessageReceived(Guid Port, string URL, string Data)
        {
            if (WebMessageReceived != null)
                return WebMessageReceived(Port, URL, Data);
            else
            {
                ErrorList.Add(System.DateTime.Now.ToString() + " - Message Received from WebServer but not connected to the code!");
                ErrorRaised?.Invoke(LastError);
                return Encoding.ASCII.GetBytes("<html><title>Netelligence WebServer</title><body>The application has made a connection for receiving messages here but hasn't connected up any code yet!</body></html>");
            }
        }

        /// <summary>
        /// What to do when a message is recieved by an underlying connection:
        /// </summary>
        /// <param name="Message">The Message data as a byte stream</param>
        /// <returns></returns>
        private void Comms_MessageReceived(byte[] Message)
        {
            MessageData Check = Message;
            bool transfer = true;
            switch(Check.Command)
            {
                case CommandList.InternalError:
                    transfer = false;
                    break;
            }
            // If not internal, send it through!
            if(transfer)
                MessageReceived?.Invoke(Message);
        }

        /// <summary>
        /// Closes a connection that is found with the listed protocol and port
        /// </summary>
        /// <param name="Protocol">Which protocol the connection is using</param>
        /// <param name="Port">The port data (int, URL, Guid) for the connection</param>
        /// <returns></returns>
        public bool CloseConnection(ConnectionProtocol Protocol, TransportPort Port)
        {
            bool Closed = false;
            foreach (Comms_Interface_Connection C in Connections)
            {
                if (C.Protocol == Protocol)
                {
                    if (Port.IsGuid() && C.Port.ToGuid() == Port.ToGuid())
                    {
                        C.CloseConnection();
                        Closed = true;
                        break;
                    }
                    else if (Port.IsPort() && C.Port.ToInt() == Port.ToInt())
                    {
                        C.CloseConnection();
                        Closed = true;
                        break;
                    }
                    else if (Port.IsURL() && C.Port.ToURL() == Port.ToURL())
                    {
                        C.CloseConnection();
                        Closed = true;
                        break;
                    }
                }
            }
            // Remove any connections that have now been closed:
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].Protocol == ConnectionProtocol.NotSet)
                {
                    Connections.RemoveAt(i);
                    i--;
                }
            }
            return Closed;
        }

        /// <summary>
        /// Signal that the program is ready to close down - shut down all connections and background processes!
        /// </summary>
        public void ShutdownApplication()
        {
            CloseAllConnections();
        }

        /// <summary>
        /// Closes all current network connections. If you are shutting down your program, you should use ShutdownApplication instead!
        /// </summary>
        public void CloseAllConnections()
        {
            if (Connections.Count > 0)
            {
                Connections[0].MessageReceived -= Comms_MessageReceived;
                foreach (Comms_Interface_Connection c in Connections)
                {
                    c.CloseConnection();
                }
                Connections.Clear();
            }
            GC.Collect();
        }
        
    }
}
