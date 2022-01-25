using System;

namespace Vortex
{
    /// <summary>
    /// The Transport port is a flexible struct that can hold a port number, URL or GUID depending on the type of connection being used for
    /// </summary>
    public struct TransportPort
    {
        int Port;
        string URL;
        Guid UniqueID;

        /// <summary>
        /// Use the TransportPort as a number - TCP, UDP etc
        /// </summary>
        /// <param name="PortNumber">The port number to use</param>
        public TransportPort(int PortNumber)
        {
            Port = PortNumber;
            URL = "";
            UniqueID = System.Guid.Empty;
        }

        /// <summary>
        /// Use the TransportPort as a URL - WebService
        /// </summary>
        /// <param name="URL">The URL the webservice sits under</param>
        public TransportPort(string URL)
        {
            Port = -1;
            this.URL = URL;
            UniqueID = System.Guid.Empty;
        }

        /// <summary>
        /// Use the TransportPort as a GUID - Create a local WebServer
        /// </summary>
        /// <param name="WebServerID">Your Webserver GUID you wish to use</param>
        public TransportPort(Guid WebServerID)
        {
            Port = -1;
            this.URL = "";
            UniqueID = WebServerID;
        }

        /// <summary>
        /// Hey cortana, is this an integer?
        /// </summary>
        /// <returns>Yes or No</returns>
        public bool IsPort()
        {
            return Port != -1;
        }

        /// <summary>
        /// Hey cortana, is this a URL?
        /// </summary>
        /// <returns>Yes or No</returns>
        public bool IsURL()
        {
            return URL != "";
        }

        /// <summary>
        /// Hey cortana, is this a GUID?
        /// </summary>
        /// <returns>Yes or No</returns>
        public bool IsGuid()
        {
            if (IsPort() == false && IsURL() == false)
                return true;
            return false;
        }

        /// <summary>
        /// Hey cortana, get me the integer!
        /// </summary>
        /// <returns>The Port</returns>
        public int ToInt()
        {
            return Port;
        }

        /// <summary>
        /// Hey cortana, get me the URL!
        /// </summary>
        /// <returns>The Port</returns>
        public string ToURL()
        {
            return URL;
        }

        /// <summary>
        /// Hey cortana, get me the GUID!
        /// </summary>
        /// <returns>The Port</returns>
        public Guid ToGuid()
        {
            return UniqueID;
        }

        public override string ToString()
        {
            if (IsPort())
                return Port.ToString();
            if (IsGuid())
                return UniqueID.ToString();
            else
                return URL;
        }

        /// <summary>
        /// Can cast directly from an integer
        /// </summary>
        /// <param name="Port">The port number</param>
        public static implicit operator TransportPort(int Port)
        {
            TransportPort temp = new TransportPort(Port);
            return temp;
        }

        public static implicit operator TransportPort(string URL)
        {
            TransportPort temp = new TransportPort(URL);
            return temp;
        }

        public static implicit operator TransportPort(Guid WebServerGuid)
        {
            TransportPort temp = new TransportPort(WebServerGuid);
            return temp;
        }

        public static implicit operator int(TransportPort data)
        {
            if (data.IsPort())
                return data.ToInt();
            else
                return -1;
        }

    }


}
