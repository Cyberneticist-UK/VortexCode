using System;
using System.Collections.Generic;
using System.Timers;
using System.Threading;

namespace Vortex
{
    /// <summary>
    /// This holds all the necessary details for the in-built Mesh network
    /// </summary>
    class Comms_Mesh
    {
        // Just to add a delay to request Netelligence Login:
        
        public event BlankTransfer MeshUpdate;
        public event WebServerSentMessage WebMessageReceived;
        public event MessageTransfer MessageReceived;
        public bool IamMaster = true;
        Comms_UDP Broadcast;
        Comms_TCP Direct;
        Comms_WebServer Server;
        Comms_Sender Sender = new Comms_Sender();
        public Comms_SystemUser Me = Comms_General.GetMe();

        public Struct_Person People = new Struct_Person();
        
        Random rnd = new Random((System.DateTime.Now.Hour + System.DateTime.Now.Minute + System.DateTime.Now.Second) * System.DateTime.Now.Millisecond);

        Guid LoginKey = Guid.NewGuid();
        //public webClient.DirectSoapClient talkToWeb = new webClient.DirectSoapClient();
        public bool GotGroupInfo = false;
        Thread bgndGroups;
        System.Timers.Timer pulseWeb = new System.Timers.Timer();

        /// <summary>
        /// The Mesh network is there to create listeners for an underlying Peer to Peer network.
        /// </summary>
        public Comms_Mesh()
        {
            // See if there is already a Mesh running on this machine:
            if (Comms_General.PortInUse(13510) == false)
            {
                // Set up the connections:
                Broadcast = new Comms_UDP(13510);
                int Port = rnd.Next(1000, 65500);

                // Make sure the ports are random but not already in use:
                while (Comms_General.PortInUse(Port) == true)
                    Port = rnd.Next(1000, 65500);
                Direct = new Comms_TCP(Port);
                Me.Connections.Add(new Comms_ConnectionDetail(Direct.ToString()));

                Server = new Comms_WebServer(new TransportPort(Guid.NewGuid()));
                Me.Connections.Add(new Comms_ConnectionDetail(Server.ToString()));

                // Set up the listeners:
                Broadcast.MessageReceived += Broadcast_MessageReceived;
                Server.WebMessageReceived += Server_WebMessageReceived;
                // Send a message to say who we are:
                MessageData Data = new MessageData();
                Data.Command = CommandList.ActivateMesh;
                Data.Data = Me.ToByteArray();
                TransportMessage BCastMessage = new TransportMessage(Broadcast.Protocol, Broadcast.Port, Data);
                Sender.InputQueue(BCastMessage);
                pulseWeb.Interval = 3000;
                pulseWeb.Elapsed += PulseWeb_Elapsed;
            }
        }

        private void PulseWeb_Elapsed(object sender, ElapsedEventArgs e)
        {
            pulseWeb.Stop();
            if (Comms_General.InterNetwork == true)
            {
                // Get Machine List:
                if (Me.Netelligence.MyMachines != null)
                {
                    string GroupData = talkToWeb.GetMyMachines(Me.Netelligence.UserNode.ToString());
                    string[] SeparatedGroups = GroupData.Split(new char[1] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string item in SeparatedGroups)
                    {
                        if (item != "")
                        {
                            if (GotMachines.IndexOf(item) == -1)
                            {
                                Me.Netelligence.MyMachines.Add(item);
                                GotMachines += item + "\t";
                            }
                        }
                    }
                }



                List<string> WindowData = new List<string>();

                int Count = 0;
                try
                {
                    Count = talkToWeb.CountWindows(Me.Machine.ComputerName, Me.Netelligence.UserNode.ToString());
                }
                catch { }
                if (Count > 0)
                {
                    // webClient.ArrayOfString Windows = talkToWeb.CheckForWindows(Me.Machine.ComputerName, Me.Netelligence.UserNode.ToString());
                    string[] Windows = talkToWeb.CheckForWindows(Me.Machine.ComputerName, Me.Netelligence.UserNode.ToString());
                    foreach (string item in Windows)
                    {
                        WindowData.Add(item);
                    }
                }
                try
                {
                    Count = talkToWeb.CountWindows("All", Me.Netelligence.UserNode.ToString());
                }
                catch { Count = 0; }
                if (Count > 0)
                {
                    //webClient.ArrayOfString Windows = talkToWeb.CheckForWindows("All", Me.Netelligence.UserNode.ToString());
                    string[] Windows = talkToWeb.CheckForWindows("All", Me.Netelligence.UserNode.ToString());
                    foreach (string item in Windows)
                    {
                        WindowData.Add(item);
                    }
                }
                Node temp;
                MessageData X;
                foreach (string item in WindowData)
                {
                    temp = item;
                    X = new MessageData();
                    X.Command = CommandList.SendWindow;
                    X.DataString = temp.Content;
                    X.SessionGUID = temp.UserID;
                    X.WindowGUID = temp.ItemID;
                    MessageReceived?.Invoke(X);
                }
            }
            pulseWeb.Start();
        }

        public Thread StartGettingGroups()
        {
            var t = new Thread(() => GetGroupsAndMachines());
            return t;
        }

        // Make sure every machine is only listed once
        string GotMachines = "";

        private void GetGroupsAndMachines()
        {
            if (Comms_General.InterNetwork == true)
            {
                Me.Netelligence.MyMachines = new List<string>();
                string GroupData = talkToWeb.GetMyMachines(Me.Netelligence.UserNode.ToString());
                string[] SeparatedGroups = GroupData.Split(new char[1] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string item in SeparatedGroups)
                {
                    if (item != "")
                    {
                        if (GotMachines.IndexOf(item) == -1)
                        {
                            Me.Netelligence.MyMachines.Add(item);
                            GotMachines += item + "\t";
                        }
                    }
                }

                Me.Netelligence.Groups = new List<Struct_Group>();
                GroupData = talkToWeb.GetGroups(Me.Netelligence.UserNode.UserID.ToString());
                SeparatedGroups = GroupData.Split(new char[] { '\t' });
                foreach (string item in SeparatedGroups)
                {
                    if (item != "")
                    {
                        Node Group = item;
                        Struct_Group GroupStr = new Struct_Group(Group, new List<Node>());
                        GroupData = talkToWeb.GetGroupMembers(Group.ItemID.ToString());
                        string[] Members = GroupData.Split(new char[] { '\t' });
                        foreach (string Member in Members)
                        {
                            if (Member != "")
                                GroupStr.Members.Add(Member);
                        }
                        Me.Netelligence.Groups.Add(GroupStr);
                    }
                }
                GotGroupInfo = true;
            }
        }

        public void UpdateNetelligenceDetails(string loginToken)
        {
            Me.Netelligence.AddUser((Node)loginToken);
            Me.Netelligence.UserNode.Content = Me.Machine.ComputerName;
            Me.Netelligence.UserNode.Names[(int)Field.Content] = "Machine";
            
            string Name = talkToWeb.GetMember(Me.Netelligence.UserNode.ToString());
            if (Name != "Not Found")
            {
                // Add the member and create the groups:
                Me.Netelligence.AddMember(Name);
                if (bgndGroups == null || bgndGroups.ThreadState == ThreadState.Unstarted || bgndGroups.ThreadState == ThreadState.Stopped)
                {
                    bgndGroups = StartGettingGroups();
                    bgndGroups.Start();
                }
            }
            
            // New Code - but this could cause issues!
            // Update the mesh to say that we have a Netelligence UID too!
            MessageData Data = new MessageData();
            Data.Command = CommandList.ActivateMesh;
            Data.Data = Me.ToByteArray();
            TransportMessage BCastMessage = new TransportMessage(Broadcast.Protocol, Broadcast.Port, Data);
            Sender.InputQueue(BCastMessage);
            pulseWeb.Start();
        }
        

        private byte[] Server_WebMessageReceived(Guid Port, string URL, string Data)
        {
            return WebMessageReceived?.Invoke(Port, URL, Data);
        }

        private Comms_SystemUser UpdateUserList(Comms_SystemUser friend, bool AddIfNotFound)
        {
            bool Found = false;
            for (int i = 0; i < People.Count; i++)
            {
                if (People[i].Machine.ComputerName == friend.Machine.ComputerName)
                {
                    People[i] = friend;
                    Found = true;
                    break;
                }
            }
            if (AddIfNotFound == true && Found == false)
                People.Add(friend);
            return friend;
        }

        private void Broadcast_MessageReceived(byte[] Message)
        {
            MessageData Data = Message;
            switch (Data.Command)
            {
                case CommandList.UpdateMesh:
                    UpdateUserList(new Comms_SystemUser(Data.DataString), false);
                    break;
                case CommandList.ActivateMesh:
                    Comms_SystemUser fri2 = UpdateUserList(new Comms_SystemUser(Data.DataString), true);
                    if (IamMaster == true && People[People.Count - 1].Machine.ComputerName != Me.Machine.ComputerName)
                    {
                        // Tell them who is boss!
                        MessageData Boss = new MessageData();
                        Boss.Command = CommandList.YouAreSlave;
                        Boss.DataString = "";
                        foreach (Comms_SystemUser item in People.GetPeople)
                        {
                            Boss.DataString += item.ToString() + "!";
                        }
                        foreach (Comms_ConnectionDetail det in fri2.Connections)
                        {
                            if (det.Protocol == Direct.Protocol)
                            {
                                TransportMessage TCastMessage = new TransportMessage(det.Protocol, det.Port, Boss, People[People.Count - 1]);
                                Sender.InputQueue(TCastMessage);
                                break;
                            }
                        }
                    }
                    break;
                case CommandList.DeactivateMesh:
                    for (int i = 0; i < People.Count; i++)
                    {
                        if (new Comms_SystemUser(Data.DataString).Machine.ComputerName == People[i].Machine.ComputerName)
                        {
                            People.RemoveAt(i);
                            break;
                        }
                    }
                    break;
                case CommandList.YouAreSlave:
                    IamMaster = false;
                    string[] MeetNewPeople = Data.DataString.Split(new char[] { '!' });
                    foreach (string friend in MeetNewPeople)
                    {
                        if (friend != "")
                        {
                            UpdateUserList(new Comms_SystemUser(friend), true);
                        }
                    }
                    break;
                case CommandList.YouAreMaster:
                    IamMaster = true;
                    break;
            }
            MeshUpdate?.Invoke();
        }

        private void PassTheFlame()
        {
            int counter = 0;
            while (counter < People.Count && People[counter].Machine.ComputerName == Me.Machine.ComputerName)
                counter++;
            if(counter < People.Count)
            {
                // Tell them they are now the boss!
                MessageData Boss = new MessageData();
                Boss.Command = CommandList.YouAreMaster;
                Boss.DataString = Me.ToString();
                foreach (Comms_ConnectionDetail det in People[counter].Connections)
                {
                    if (det.Protocol == Direct.Protocol)
                    {
                        TransportMessage TCastMessage = new TransportMessage(det.Protocol, det.Port, Boss, People[counter]);
                        Sender.InputQueue(TCastMessage);
                    }
                }
                
            }
        }

        public void Disconnect()
        {
            try
            {
                if (Broadcast != null)
                {
                    if (IamMaster == true)
                    {
                        PassTheFlame();
                    }
                    MessageData Data = new MessageData();
                    Data.Command = CommandList.DeactivateMesh;
                    Data.Data = Me.ToByteArray();
                    TransportMessage BCastMessage = new TransportMessage(Broadcast.Protocol, Broadcast.Port, Data);
                    Sender.InputQueue(BCastMessage);
                    // Close the connections:
                    Broadcast.MessageReceived -= Broadcast_MessageReceived;
                    Broadcast.CloseConnection();
                    Direct.CloseConnection();
                    Server.CloseConnection();
                }
            }
            catch
            {
                // Close the connections:
                Broadcast.MessageReceived -= Broadcast_MessageReceived;
                Broadcast.CloseConnection();
                Direct.CloseConnection();
                Server.CloseConnection();
            }
        }
    }

    
}
