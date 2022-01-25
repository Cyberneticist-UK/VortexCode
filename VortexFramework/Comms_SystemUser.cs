using System;
using System.Collections.Generic;
using System.Text;

namespace Vortex
{
    /// <summary>
    /// The Comms_SystemUser Construct allows you to send a message to a person whether they are on the local network or on the system (or both!)
    /// </summary>
    public class Comms_SystemUser
    {
        public Struct_Machine Machine = new Struct_Machine();
        public Struct_Netelligence Netelligence = new Struct_Netelligence();
        public List<Comms_ConnectionDetail> Connections = new List<Comms_ConnectionDetail>();
        
        public override string ToString()
        {
            string Connex = "";
            foreach (Comms_ConnectionDetail item in Connections)
            {
                Connex += item.ToString()+"#@#";
            }
            return Machine.IPAddress + "~" + Machine.IPAddressv6 + "~" + Machine.ComputerName + "~" + Machine.LoggedInAs + "~" + Netelligence.UserID.ToString() + "~" + Netelligence.Username + "~" + Netelligence.FullName + "~"+Connex;
        }

        public byte[] ToByteArray()
        {
            return Encoding.ASCII.GetBytes(ToString());
        }

        public Comms_SystemUser()
        {

        }

        public Comms_SystemUser(string Data)
        {
            Machine = new Struct_Machine();
            string[] Items = Data.Split(new char[] { '~' });
            if (Items[0] == "")
                Machine.IPAddress = null;
            else
                Machine.IPAddress = Items[0];

            if (Items[1] == "")
                Machine.IPAddressv6 = null;
            else
                Machine.IPAddressv6 = Items[1];
            if (Items[2] == "")
                Machine.ComputerName = null;
            else
                Machine.ComputerName = Items[2];
            if (Items[3] == "")
                Machine.LoggedInAs = null;
            else
                Machine.LoggedInAs = Items[3];
            if (Items[4] == "")
                Netelligence.UserID = Guid.Empty;
            else
                Netelligence.UserID = Guid.Parse(Items[4]);

            if (Items[5] == "")
                Netelligence.Username = null;
            else
                Netelligence.Username = Items[5];
            if (Items[6] == "")
                Netelligence.FullName = null;
            else
                Netelligence.FullName = Items[6];
            Connections = new List<Comms_ConnectionDetail>();
            if (Items.Length > 7 && Items[7] != "")
            {
                string Connex = Items[7];
                Connex = Connex.Replace("#@#", "~");
                string[] Items2 = Connex.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(string i in Items2)
                {
                    if(i != "")
                    Connections.Add(new Comms_ConnectionDetail(i));
                }
            }
            

        }

        /// <summary>
        /// Create a System User that is on the LAN but not logged in:
        /// </summary>
        /// <param name="IPAddress"></param>
        /// <param name="ComputerName"></param>
        public Comms_SystemUser(string IPAddress, string ComputerName, string LoggedInAs)
        {
            Machine = new Struct_Machine();
            this.Machine.LoggedInAs = LoggedInAs;
            this.Machine.IPAddress = IPAddress;
            this.Machine.IPAddressv6 = null;
            this.Machine.ComputerName = ComputerName;
            this.Netelligence.UserID = System.Guid.Empty;
            this.Netelligence.Username = null;
            this.Netelligence.FullName = null;
            Connections = new List<Comms_ConnectionDetail>();
        }

        /// <summary>
        /// Create a System User that is logged in but not on the LAN:
        /// </summary>
        /// <param name="NetelligenceID"></param>
        /// <param name="NetelligenceUsername"></param>
        /// <param name="FullName"></param>
        public Comms_SystemUser(Guid NetelligenceID, string NetelligenceUsername, string FullName)
        {
            Machine = new Struct_Machine();
            this.Machine.IPAddress = null;
            this.Machine.IPAddressv6 = null;
            this.Machine.ComputerName = null;
            Machine.LoggedInAs = null;
            this.Netelligence.UserID = NetelligenceID;
            this.Netelligence.Username = NetelligenceUsername;
            this.Netelligence.FullName = FullName;
            Connections = new List<Comms_ConnectionDetail>();
        }

        /// <summary>
        /// Create a System user that is logged in and on the LAN:
        /// </summary>
        /// <param name="IPAddress"></param>
        /// <param name="ComputerName"></param>
        /// <param name="NetelligenceID"></param>
        /// <param name="NetelligenceUsername"></param>
        /// <param name="FullName"></param>
        public Comms_SystemUser(string IPAddress, string IPAddressv6, string ComputerName, string LoggedInAs, Guid NetelligenceID, string NetelligenceUsername, string FullName)
        {
            Machine = new Struct_Machine();
            this.Machine.IPAddress = IPAddress;
            this.Machine.IPAddressv6 = IPAddressv6;
            this.Machine.LoggedInAs = LoggedInAs;
            this.Machine.ComputerName = ComputerName;
            this.Netelligence.UserID = NetelligenceID;
            this.Netelligence.Username = NetelligenceUsername;
            this.Netelligence.FullName = FullName;
            Connections = new List<Comms_ConnectionDetail>();
        }

        /// <summary>
        /// Tells you if this user is logged in or not:
        /// </summary>
        /// <returns></returns>
        public bool LoggedIn()
        {
            return (Netelligence.UserID == Guid.Empty);
        }

        /// <summary>
        /// Tells you if this user is on the LAN or not:
        /// </summary>
        /// <returns></returns>
        public bool OnLAN()
        {
            return (Machine.IPAddress != null) && (Machine.IPAddressv6 != null);
        }
    }
}
