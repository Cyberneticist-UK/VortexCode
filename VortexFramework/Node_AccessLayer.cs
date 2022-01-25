using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.IO;

namespace Vortex
{
    public enum RegisterStatus { UsernameTaken, Accepted }

    /// <summary>
    /// The layer on top of the database that turns it from being a standard database into a Netelligence Graph Structure!
    /// Notes on what needs doing:-
    /// Queries need to stop using * !!
    /// </summary>
    public class Node_AccessLayer
    {
        public Interface_DataConnector dc;
        public Dictionary<string, Node> TypeNodes = new Dictionary<string, Node>();
        public Dictionary<string, Node> LinkTypeNodes = new Dictionary<string, Node>();
        public Dictionary<Guid, Node> Nodes = new Dictionary<Guid, Node>();
        public string ObjectTable, LinkTable;
        int ContentLength = 1024;
        public Node CurrentlyLoggedIn = null;
        public string serverLocation = "";

        /// <summary>
        /// The new Netelligence Cache!
        /// The data depends on the type! Nothing=Remote; Access or local database, Filename and password if required, if it's SQL server: Server, Catalogue, [Username and Password].
        /// </summary>
        /// <param name="Type">What type of connection that you want to use</param>
        /// <param name="Data">The needed parameters for the connection</param>
        public Node_AccessLayer(string serverLocation, DatabaseConnectionType Type, string ObjectTable, string LinkTable, params string[] Data)
        {
            // New for 2021 - serverLocation, if set this is where extended nodes are saved as a file!
            if (serverLocation != "")
            {
                serverLocation = serverLocation.Replace("\\NodeExtend", "");
                if (Directory.Exists(serverLocation) == true)
                    this.serverLocation = serverLocation;
                else
                {
                    // Could be in a sub directory trying to access the folder - so go back a few steps!
                    string[] Folders = serverLocation.Split(new char[] { '\\' });
                    List<string> FolderList = new List<string>();
                    foreach (string item in Folders)
                    {
                        if (item.Trim() != "")
                        {
                            FolderList.Add(item);
                        }
                    }
                    // c:\Windows\Sub\sub\sub\VortexCommand
                    while (FolderList.Count > 0)
                    {
                        serverLocation = "";
                        for (int i = 0; i < FolderList.Count; i++)
                        {
                            if (i != FolderList.Count - 2)
                                serverLocation += FolderList[i] + "\\";
                        }
                        if (Directory.Exists(serverLocation) == true)
                        {
                            this.serverLocation = serverLocation;
                            FolderList.Clear();
                        }
                        else
                        {
                            FolderList.RemoveAt(FolderList.Count - 2);
                        }
                    }

                }

                this.serverLocation += "\\NodeExtend\\";
            }
            this.ObjectTable = ObjectTable;
            this.LinkTable = LinkTable;
            switch(Type)
            {
                case DatabaseConnectionType.Remote:
                    //dc = new Comms_DatabaseRemote();
                    break;
                case DatabaseConnectionType.Access:
                    if(Data.Length == 1)
                        // No password
                        dc = new Comms_DatabaseOLEDB(Data[0]);
                    else if (Data.Length == 2)
                        // Password
                        dc = new Comms_DatabaseOLEDB(Data[0], Data[1]);
                    break;
                case DatabaseConnectionType.LocalMDB:
                    if (Data.Length >= 1)
                        // No password here!
                        dc = new Comms_DatabaseLocalSQL(Data[0]);
                    break;
                case DatabaseConnectionType.SQLServer:
                    if (Data.Length == 4)
                        // Full server detail:
                        dc = new Comms_DatabaseSQLServer(Data[0], Data[1], Data[2], Data[3]);
                    if (Data.Length == 2)
                        // Full server detail:
                        dc = new Comms_DatabaseSQLServer(Data[0], Data[1]);
                    break;
                case DatabaseConnectionType.MySQL:
                    if (Data.Length == 4)
                        // Full server detail:
                        dc = new Comms_DatabaseMySQL(Data[0], Data[1], Data[2], Data[3]);
                    if (Data.Length == 2)
                        // Full server detail:
                        dc = new Comms_DatabaseMySQL(Data[0], Data[1]);
                    break;
            }

            //CreateTypeNodes();
        }

        #region Log the user in

        /// <summary>
        /// Try and log the user in with the current cookie they have:
        /// </summary>
        /// <param name="Cookie">Cookie supplied by the website</param>
        /// <param name="UserAgent">Current Useragent</param>
        /// <returns>True if logged in, false if not</returns>
        public bool LogMeIn(string Cookie, string UserAgent)
        {
            // Turn the cookie into a login node:
            Node item = Cookie;
            if (item == null)
                return false;
            // Is this cookie still in the system database?
            Node CheckItemExists = GetNode(item.ItemID);
            // If not, can't log in!
            if (CheckItemExists == null)
                return false;
            // if it is, check the user agent is correct:
            if (item.SubContent == Sec_Crypt.CalculateChecksumData(UserAgent))
            {
                // Create the user node:
                CurrentlyLoggedIn = GetNode(item.UserID);
                return true;
            }
            else
            {
                // This used to return false - just seeing if there is some useragent issue at present
                CurrentlyLoggedIn = GetNode(item.UserID);
                return true;
            }
        }

        //public bool LogMeIn(string UserID)
        //{
        //    CurrentlyLoggedIn = GetNode(Guid.Parse(UserID));
        //    return true;
        //}

        public bool LogMeIn(string UsernameOrEmail, string Password, string UserAgent, out Node Session)
        {
            Session = new Node(Constants.BlankID);
            Session.Access = this;
            // Get the details:
            string UserID = dc.DataScalar("select ItemID from " +
                ObjectTable + 
                " where (Title = '" +
                UsernameOrEmail.Replace("'", "''") + "' or SubContent = '" +
                UsernameOrEmail.Replace("'", "''") + "') and TypeID = '" +
                GetNodeType("User") + 
                "' and Active='Y'");
            if (UserID == "")
            {
                // Could not find:
                Session.Content = "Initial Database Fail";
                return false;
            }
            Node User = GetNode(Guid.Parse(UserID));
            User.Content.Type = ContentType.BlowfishEncrypted;
            string temp = "0";
            if (User.Value1 != null)
                temp = User.Value1.ToString();
            if ((User.Content == Convert.ToString(Password + temp)) == true)// BCryptEmbed.CheckPassword(Password + User.Value1.ToString(), User.Content.ToDataString()) == true)
            {
                CurrentlyLoggedIn = User;
                // We have the UserID = set up the session:-
                Session.UserID = Guid.Parse(UserID);
                Session = CreateNode(Constants.BlankID, Session, "Session");
                Session.UserID = Guid.Parse(UserID);
                Session.Title = "Session";
                Session.Content = "Worked!";
                Session.BooleanItem = false;
                Session.SubContent = Sec_Crypt.CalculateChecksumData(UserAgent);
                SaveNode(ref Session, Constants.BlankID, null);

                // Link the session to the User:
                SaveLink(ref User, ref Session, "Standard", Constants.BlankID, null);
                return true;
            }
            else
            {
                // Could not find:
                Session.Content = "Password Match Not Found";
                return false;
            }
            
        }

        public bool LogMeInLink(string UsernameOrEmail, string UserAgent, out Node Session, out Node User)
        {
            Session = new Node(Constants.BlankID);
            Session.Access = this;
            // Get the details:
            string UserID = dc.DataScalar("select ItemID from " +
                ObjectTable +
                " where (Title = '" +
                UsernameOrEmail.Replace("'", "''") + "' or SubContent = '" +
                UsernameOrEmail.Replace("'", "''") + "') and TypeID = '" +
                GetNodeType("User") +
                "' and Active='Y'");
            if (UserID == "")
            {
                // Could not find:
                Session.Content = "Initial Database Fail";
                User = null;
                return false;
            }
            User = GetNode(Guid.Parse(UserID));
            // User.Content.Type = ContentType.BlowfishEncrypted;
            string temp = "0";
            if (User.Value1 != null)
                temp = User.Value1.ToString();
            // if ((User.Content == Convert.ToString(Password + temp)) == true)// BCryptEmbed.CheckPassword(Password + User.Value1.ToString(), User.Content.ToDataString()) == true)
            {
                CurrentlyLoggedIn = User;
                // We have the UserID = set up the session:-
                Session.UserID = Guid.Parse(UserID);
                Session = CreateNode(Constants.BlankID, Session, "Session");
                Session.UserID = Guid.Parse(UserID);
                Session.Title = "Session";
                Session.Content = "Worked!";
                Session.BooleanItem = false;
                Session.SubContent = Sec_Crypt.CalculateChecksumData(UserAgent);
                SaveNode(ref Session, Constants.BlankID, null);

                // Link the session to the User:
                SaveLink(ref User, ref Session, "Standard", Constants.BlankID, null);
                return true;
            }
            //else
            //{
            //    // Could not find:
            //    Session.Content = "Password Match Not Found";
            //    return false;
            //}

        }

        public void Logout(Guid UserID, Guid SessionID)
        {
            DeleteLink(UserID, SessionID);
            //DeleteNode(SessionID);
            CurrentlyLoggedIn = null;
        }
        #endregion

        #region Creating, Saving and Deleting Nodes

        public Node CreateNode(Guid ApplicationID, Node SessionID, string Type)
        {
            Node NewNode = GetEmpty();
            NewNode.Access = this;
            NewNode.ApplicationID = ApplicationID;
            // Needs to be userID!!
            if (SessionID == null)
                NewNode.UserID = ApplicationID;
            else
                NewNode.UserID = SessionID.UserID;
            if (Type != "Type")
            {
                NewNode.TypeID = GetNodeType(Type);
                NewNode.Names = new NodeContent(GetNode(NewNode.TypeID).Content, ContentType.List);
            }
            else
            {
                NewNode.TypeID = Constants.BlankID;
                NewNode.Names = new NodeContent("Type Name,,Fields,Version,,,,,,", ContentType.List);
            }
            return NewNode;
        }

        public string RegisterUser(string Username, string Password, string Email, int UserType, out RegisterStatus Status, Node SessionNode)
        {
            string ItemID = dc.DataScalar("select itemID from " + ObjectTable + " where TypeID = '" + GetNodeType("User") + "' and Title='" + Username.Replace("'", "''") + "'");
            if (ItemID != "")
            {
                // Username already taken:
                Status = RegisterStatus.UsernameTaken;
                return "";
            }
            else
            {
                // We'll allow it!
                Status = RegisterStatus.Accepted;
                Node newUser = CreateNode(Constants.BlankID, SessionNode, "User");
                newUser.Title = Username;
                newUser.Value1 = UserType;
                newUser.Content = BCryptEmbed.HashPassword((Password + UserType.ToString()).Replace("'", "''"), BCryptEmbed.GenerateSalt(12));
                newUser.SubContent = Email;
                newUser.Active = false;
                SaveNode(ref newUser, Constants.BlankID, SessionNode);
                return newUser.ItemID.ToString();
                // Should all be ready!
            }
        }


        public Node GetEmpty()
        {
            // we need to create a node from the empty nodes in the database
            string EmptyTypeID = "0CE934B0-0000-4545-89F7-F0C4A259A9AC"; // GetNodeType("Empty").ToString();
            dc.DataNonSelect("update " + ObjectTable + " set Active='N' where TypeID = '" + EmptyTypeID + "' and Created < datediff(day,1,GETDATE())");
            string UID = dc.DataScalar("select ItemID from " + ObjectTable + " where TypeID = '" + EmptyTypeID + "' and Active = 'N'");
            if (UID == null || UID == "")
            {
                CreateEmpty(10);
                UID = dc.DataScalar("select ItemID from " + ObjectTable + " where TypeID = '" + EmptyTypeID + "' and Active = 'N'");
            }
            dc.DataNonSelect("Update " + ObjectTable + " set Active='Y', Created = GetDate(), LastUpdated = GetDate() where ItemID = '" + UID + "'");
            return new Node(System.Guid.Parse(UID));
        }

        public void CreateEmpty(int NumberToCreate)
        {
            string GUID = System.Guid.NewGuid().ToString();
            string EmptyTypeID = "0CE934B0-0000-4545-89F7-F0C4A259A9AC";// GetNodeType("Empty").ToString();
            GUID = GUID.Substring(0, 9) + "0000" + GUID.Substring(13, 23);
            dc.DataOpen();
            for (int i = 0; i < NumberToCreate; i++)
            {
                // dc.DataNonSelectBatch("insert into tbl_NetNode (ItemID, Active, TypeID, UserID, ApplicationID) values (NewID(), 'N', '0CE934B0-0000-4545-89F7-F0C4A259A9AC', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000')");
                dc.DataNonSelectBatch("insert into " + ObjectTable + " (ItemID, Active, TypeID, UserID, ApplicationID) values ('" + GUID + "', 'N', '" + EmptyTypeID + "', '" + Constants.BlankID + "', '" + Constants.BlankID + "')");
                GUID = System.Guid.NewGuid().ToString();
                GUID = GUID.Substring(0, 9) + "0000" + GUID.Substring(13, 23);
            }
            dc.DataClose();
        }

        public void CreateSpecificEmpty(Guid ItemID)
        {
            dc.DataNonSelect("insert into " + ObjectTable + " (ItemID, Active, TypeID, UserID, ApplicationID, Title, Content) values ('" + ItemID.ToString() + "', 'N', '" + Constants.BlankID + "', '" + Constants.BlankID + "', '" + Constants.BlankID + "', 'Type', 'Type Name,,Type Fields,,,,,,,')");
        }

        public bool SaveNode(Node Node, Guid ApplicationID, Node Session)
        {
            return SaveNode(ref Node, ApplicationID, Session);
        }

        public bool SaveNode(ref Node Node, Guid ApplicationID, Node Session)
        {
            if (ValidateNode(ref Node, ref ApplicationID, ref Session) == true)
            {
                // Stage 1: work out which fields have changed: 
                NodeChanges Changes = NodeDataFilter(ref Node);
                Guid ExtendedID = Constants.ExtendedContentID;

                // Save it!

                // Stage 2: If the content has changed, remove any existing extended data nodes for this node:
                if (Changes.Content == true)
                    dc.DataNonSelect("delete from " + ObjectTable
                         + " where TypeID = '" + ExtendedID.ToString() + "' and Title = '" + Node.ItemID + "'");
               // if(serverLocation != "")
               // {
                    if(File.Exists(serverLocation + Node.ItemID) == true)
                        File.Delete(serverLocation + Node.ItemID);
               // }

                // Stage 3: Get the data that needs to be updated:

                string Updates = TypeFilter(ref Node, ref Changes);
                // Stage 4: If there is anything to update:
                if (Updates.Length > 0)
                {
                    // Update the node, or if number of nodes > 1 truncate the content of the node and add extra content:
                    // Stage 1: how many nodes do we need?
                    //if (Node.Content == null)
                    //    Node.Content = "";
                    int Nodes = Convert.ToInt32(Node.Content.ToString().Replace("'", "''").Length) / ContentLength;
                    if (ContentLength < Convert.ToInt32(Node.Content.ToString().Replace("'", "''").Length))
                        Nodes++;
                    // Update the main node:
                    dc.DataNonSelect("update " + ObjectTable + " set " + Updates + " LastUpdated = GetDate() where ItemID = '" + Node.ItemID + "'");
                    if (Nodes > 1)
                    {
                        // Create the extended data - the first one will be truncated anyway when the code did it's bit:
                        string Rest = Node.Content.ToString().Replace("'", "''").Substring(ContentLength);
                        int counter = 0;
                        if (serverLocation == "")
                        {
                            while (Rest.Length > 0)
                            {

                                Node Extended = CreateNode(ApplicationID, Session, "Extended Content");
                                if (Rest.Length > ContentLength)
                                {
                                    Extended.Content = Rest.Substring(0, ContentLength);
                                    Rest = Rest.Substring(ContentLength);
                                }
                                else
                                {
                                    Extended.Content = Rest;
                                    Rest = "";
                                }
                                dc.DataNonSelect("update " + ObjectTable + " set Content = '" + Extended.Content + "', Created = GetDate(), Title = '" + Node.ItemID + "', TypeID = '" + ExtendedID + "', ApplicationID = '" + Node.ApplicationID + "', UserID = '" + Node.UserID + "', Value1 = " + counter.ToString() + ", LastUpdated = GetDate() where ItemID = '" + Extended.ItemID + "'");
                                counter++;

                            }
                        }
                        else
                        {
                            File.WriteAllText(serverLocation + Node.ItemID, Node.Content.ToString().Substring(ContentLength));
                        }
                    }
                }
            }
            return true;
        }

        private bool ValidateNode(ref Node Node, ref Guid ApplicationID, ref Node Session)
        {
            return true;
            //if (TypesKeyName.ContainsKey("Session") == true)
            //{
            //    if ((Session == null) && (ApplicationID == Constants.BlankID) && (Node.TypeID == ((Node)TypesKeyName["Session"]).ItemID))
            //    {
            //        // This is a new session being created - That's fine!
            //        Session = new Node(Node.ItemID);
            //        Session.UserID = Node.UserID;
            //        return true;
            //    }
            //}
            //return true;
        }

        /// <summary>
        /// Filter out non-changed data from the node
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="Node"></param>
        private NodeChanges NodeDataFilter(ref Node Node)
        {
            NodeChanges changes = new NodeChanges();
            // Get the Type Node:
            Node OriginalNode = GetNode(Node.ItemID);
            if (OriginalNode == null)
            {
                CreateSpecificEmpty(Node.ItemID);
                OriginalNode = GetNode(Node.ItemID);
            }
            OriginalNode.Access = this;
            if (OriginalNode.TypeID != Node.TypeID)
                changes.TypeID = true;
            if (OriginalNode.Content != Node.Content)
                changes.Content = true;
            if (OriginalNode.Date1 != Node.Date1)
                changes.Date1 = true;
            if (OriginalNode.Date2 != Node.Date2)
                changes.Date2 = true;
            if (OriginalNode.Date3 != Node.Date3)
                changes.Date3 = true;

            if (OriginalNode.SubContent != Node.SubContent)
                changes.SubContent = true;
            if (OriginalNode.Title != Node.Title)
                changes.Title = true;
            if (OriginalNode.Value1 != Node.Value1)
                changes.Value1 = true;
            if (OriginalNode.Value2 != Node.Value2)
                changes.Value2 = true;
            if (OriginalNode.Value3 != Node.Value3)
                changes.Value3 = true;

            if (OriginalNode.BooleanItem != Node.BooleanItem)
                changes.BooleanValue = true;


            if (OriginalNode.Active != Node.Active)
                changes.Active = true;

            return changes;
        }

        /// <summary>
        /// Filter out information not included in this type
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="Node"></param>
        /// <param name="Changed"></param>
        private string TypeFilter(ref Node Node, ref NodeChanges Changed)
        {
            // Get the Type Node:
            string[] Contents;

            Node TypeNode = GetNode(Node.TypeID);
            TypeNode.Access = this;
            // Take out the array:
            Contents = TypeNode.Content;

            string UpdateList = "";
            if (Changed.TypeID == true)
            {
                // Should be a brand new update! e.g. changed from a "Empty" type!
                UpdateList += "Created = GetDate(), TypeID = '" + Node.TypeID + "', ApplicationID = '" + Node.ApplicationID + "', UserID = '" + Node.UserID + "', ";
            }
            if ((Contents[0] != "") && (Changed.Title))
            {
                if(Node.Title != null)
                    UpdateList += "Title = '" + Node.Title.Replace("'", "''") + "', ";
            }
            if ((Contents[1] != "") && (Changed.SubContent))
            {
                if (Node.SubContent != null)
                    UpdateList += "SubContent = '" + Node.SubContent.Replace("'", "''") + "', ";
            }
            if ((Contents[2] != "") && (Changed.Content))
            {

                if (Node.Content.ToString().Replace("'", "''").Length > ContentLength)
                {
                    UpdateList += "Content = '" + Node.Content.ToString().Replace("'", "''").Substring(0, ContentLength) + "', ";
                }
                else
                    UpdateList += "Content = '" + Node.Content.ToString().Replace("'", "''") + "', ";

            }


            if (Contents.Length > 3 && Node.Value1 != null && (Contents[3] != "") && (Changed.Value1))
                UpdateList += "Value1 = " + Node.Value1 + ", ";

            if (Contents.Length > 4 && Node.Value2 != null && (Contents[4] != "") && (Changed.Value2))
                UpdateList += "Value2 = " + Node.Value2 + ", ";

            if (Contents.Length > 5 && Node.Value3 != null && (Contents[5] != "") && (Changed.Value3))
                UpdateList += "Value3 = " + Node.Value3 + ", ";

            if (Contents.Length > 6 && (Changed.Date1) && (Contents[6] != "")  && (Node.Date1 != null))
                UpdateList += "Date1 = '" + Node.Date1.ToString("yyyy-MM-dd HH:mm:ss") + "', ";

            if (Contents.Length > 7 && (Changed.Date2) && (Contents[7] != "") && (Node.Date2 != null))
                UpdateList += "Date2 = '" + Node.Date2.ToString("yyyy-MM-dd HH:mm:ss") + "', ";

            if (Contents.Length > 8 && (Changed.Date3) && (Contents[8] != "") && (Node.Date3 != null))
                UpdateList += "Date3 = '" + Node.Date3.ToString("yyyy-MM-dd HH:mm:ss") + "', ";

            if (Contents.Length > 9 && (Changed.BooleanValue) && (Contents[9] != ""))
            {
                if (Node.BooleanItem == true)
                    UpdateList += "BooleanItem = 'Y', ";
                else
                    UpdateList += "BooleanItem = 'N', ";

            }
            if (Changed.Active)
            {
                if (Node.Active == true)
                    UpdateList += "Active = 'Y', ";
                else
                    UpdateList += "Active = 'N', ";

            }
            return UpdateList;
        }


        public void DeleteNode(Guid itemID)
        {
            // Delete the node:
            dc.DataNonSelect("delete from " + ObjectTable + " where ItemID = '" + itemID.ToString() + "'");
            // Delete the links:
            // DeleteLink(itemID, itemID);
        }

        public void DeleteLink(Guid ParentNodeID, Guid ChildNodeID)
        {
            // Delete the links:
            dc.DataNonSelect("delete from " + LinkTable + " where Item1ID = '" + ParentNodeID.ToString() + "' and Item2ID = '" + ChildNodeID.ToString() + "'");
        }

        /// <summary>
        /// If the Node specified has no children or parents left, it is deleted.
        /// </summary>
        /// <param name="ParentNodeID"></param>
        /// <param name="ChildNodeID"></param>
        /// <param name="NodeID"></param>
        public void DeleteLink2(Guid ParentNodeID, Guid ChildNodeID, Guid NodeID)
        {
            DeleteLink(ParentNodeID, ChildNodeID);
            if (GetParents(NodeID).Count == 0 && GetChildren(NodeID).Count == 0)
            {
                DeleteNode(NodeID);
            }
        }
        #endregion

        #region Dealing with Links

        public NodeLink SaveLink(ref Node ParentNode, ref Node ChildNode, string LinkType, Guid ApplicationID, Node Session)
        {
            if (ValidateNodeLink(ref ParentNode, ref ChildNode, ref ApplicationID, ref Session) == true)
            {
                dc.DataNonSelect("insert into " + LinkTable + " (Item1ID, Item2ID, LinkTypeID) values ('" + ParentNode.ItemID + "', '" + ChildNode.ItemID + "', '" + GetNodeLinkType(LinkType).ToString() + "')");
            }
            NodeLink X = GetNodeLink(ParentNode.ItemID, ChildNode.ItemID);
            return X;
        }

        public NodeLink SaveLink(Guid ParentNodeID, Guid ChildNodeID, string LinkType, string ApplicationID, Node Session)
        {
            dc.DataNonSelect("insert into " + LinkTable + " (Item1ID, Item2ID, LinkTypeID) values ('" + ParentNodeID.ToString() + "', '" + ChildNodeID.ToString() + "', '" + GetNodeLinkType(LinkType).ToString() + "')");
            NodeLink X = GetNodeLink(ParentNodeID, ChildNodeID);
            return X;
        }

        public NodeLink SaveLink(Guid ParentNodeID, Guid ChildNodeID, Guid LinkTypeID)
        {
            dc.DataNonSelect("insert into " + LinkTable + " (Item1ID, Item2ID, LinkTypeID) values ('" + ParentNodeID.ToString() + "', '" + ChildNodeID.ToString() + "', '" + LinkTypeID.ToString() + "')");
            NodeLink X = GetNodeLink(ParentNodeID, ChildNodeID);
            return X;
        }

        public void UpdateLink(NodeLink Link, LinkField Update)
        {
            string Field = "";
            switch (Update)
            {
                case LinkField.Confirmed:
                    if (Link.Confirmed)
                        Field = "Confirmed = 'Y'";
                    else
                        Field = "Confirmed = 'N'";
                    break;
                case LinkField.ExtraInfo:
                    Field = "ExtraInfo = '" + Link.ExtraInfo.Replace("'", "''") + "'";
                    break;
                case LinkField.LinkTypeID:
                    Field = "LinkTypeID = '" + Link.LinkTypeID + "'";
                    break;
                case LinkField.PathID:
                    Field = "PathID = '" + Link.PathID + "'";
                    break;
                case LinkField.ValueInfo:
                    Field = "ValueInfo = " + Link.ValueInfo.ToString();
                    break;
                case LinkField.LinkCreated:
                    Field = "LinkCreated = '" + Link.LinkCreated.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    break;
                case LinkField.LinkLastUpdated:
                    Field = "LinkLastUpdated = '" + Link.LinkLastUpdated.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    break;
            }
            dc.DataNonSelect("update " + LinkTable + " set " + Field + " where Item1ID = '" + Link.Item1ID + "' and Item2ID = '" + Link.Item2ID + "'");
        }

        private bool ValidateNodeLink(ref Node ParentNode, ref Node ChildNode, ref Guid ApplicationID, ref Node Session)
        {
            return true;
        }

        #endregion

        #region Get the Node / [Render a node will go here]

        public Node GetNode(Guid NodeID)
        {
            List<Node> N = DataSelectNode(new WhereClauseItem(Field.ItemID, Operator.Equal, NodeID));
            if (N == null)
                return null;
            if (N.Count > 0)
            {
                N[0].Access = this;
                return N[0];
            }
            else
                return null;
        }

        public List<Node> GetNodesByType(string Type, List<WhereClauseItem> Where = null, List<OrderBy> OrderBy = null)
        {
            return GetNodesByType(GetNodeType(Type), Where, OrderBy);
            // List<WhereClauseItem> Where = new List<WhereClauseItem>();
            // Where.Add(new WhereClauseItem(Logic.And, Field.TypeID, Operator.Equal, GetNodeType(Type)));
            // string Query = BasicNodeQuery;
            // return DataNode(Query, Where);
        }
        
        public List<Node> GetNodesByType(Guid Type, List<WhereClauseItem> Where = null, List<OrderBy> OrderBy = null)
        {
            if (Where == null)
                Where = new List<WhereClauseItem>();
            if (Where.Count > 0)
                Where.Add(new WhereClauseItem(Logic.And, Field.TypeID, Operator.Equal, Type));
            else
                Where.Add(new WhereClauseItem(Field.TypeID, Operator.Equal, Type));
            string Query = BasicNodeQuery;
            return DataNode(Query, Where);
        }

        #endregion

        #region Get Node and Link Types

        public Guid GetNodeType(string Type, decimal Version = 1)
        {
            if (TypeNodes.ContainsKey(Type + Version.ToString()) == false)
            {
                // Get from database:
                List<WhereClauseItem> Where = new List<WhereClauseItem>();
                Where.Add(new WhereClauseItem(Field.TypeID, Operator.Equal, Constants.TypeID));
                Where.Add(new WhereClauseItem(Logic.And, Field.Title, Operator.Equal, Type));
                List<Node> N = DataSelectNode(Where);
                foreach (Node item in N)
                {
                    TypeNodes.Add(item.Title + item.Value1.ToString(), item);
                    if (Nodes.ContainsKey(item.ItemID) == false)
                        Nodes.Add(item.ItemID, item);
                }
                if (N.Count == 0)
                {
                    return Guid.Empty;
                }
            }
            // return it:
            return TypeNodes[Type + Version.ToString()].ItemID;
        }

        public string GetNodeType(Guid TypeID)
        {
            if (Nodes.ContainsKey(TypeID) == false)
            {
                // Get from database:
                List<WhereClauseItem> Where = new List<WhereClauseItem>();
                Where.Add(new WhereClauseItem(Field.TypeID, Operator.Equal, Constants.TypeID));
                Where.Add(new WhereClauseItem(Logic.And, Field.ItemID, Operator.Equal, TypeID));
                List<Node> N = DataSelectNode(Where);
                if (N.Count > 0)
                {
                    TypeNodes.Add(N[0].Title + N[0].Value1.ToString(), N[0]);
                    if (Nodes.ContainsKey(TypeID) == false)
                        Nodes.Add(TypeID, N[0]);
                }
                else
                    return null;
            }
            // return it:
            return Nodes[TypeID].Title;
        }

        public Guid GetNodeLinkType(string Type)
        {
            if (LinkTypeNodes.ContainsKey(Type) == false)
            {
                // Get from database:
                List<WhereClauseItem> Where = new List<WhereClauseItem>();
                Where.Add(new WhereClauseItem(Field.TypeID, Operator.Equal, Constants.LinkTypeID));
                Where.Add(new WhereClauseItem(Logic.And, Field.Title, Operator.Equal, Type));
                List<Node> N = DataSelectNode(Where);
                if (N.Count > 0)
                {
                    LinkTypeNodes.Add(Type, N[0]);
                    Nodes.Add(N[0].ItemID, N[0]);
                }
                else
                    return Guid.Empty;
            }
            // return it:
            return LinkTypeNodes[Type].ItemID;
        }

        public string GetNodeLinkType(Guid TypeID)
        {
            if (Nodes.ContainsKey(TypeID) == false)
            {
                // Get from database:
                List<WhereClauseItem> Where = new List<WhereClauseItem>();
                Where.Add(new WhereClauseItem(Field.TypeID, Operator.Equal, Constants.LinkTypeID));
                Where.Add(new WhereClauseItem(Logic.And, Field.ItemID, Operator.Equal, TypeID));
                List<Node> N = DataSelectNode(Where);
                if (N.Count > 0)
                {
                    LinkTypeNodes.Add(N[0].Title, N[0]);
                    Nodes.Add(TypeID, N[0]);
                }
                else
                    return null;
            }
            // return it:
            return Nodes[TypeID].Title;
        }

        #endregion

        #region Get Children Clauses (inc protected base)

        public List<string> GetChildTypes(Guid NodeID)
        {
            DataTable dt = dc.DataSelect("select Title from "+ObjectTable+ " inner join (select TypeID from " + ObjectTable + " inner join " + LinkTable + " on " + LinkTable + ".item2ID = " + ObjectTable + ".itemID where Item1ID = '" + NodeID.ToString()+ "' Group By typeID) as A on A.TypeID = " + ObjectTable + ".itemID");
            List<string> result = new List<string>();
            foreach(DataRow row in dt.Rows)
            {
                result.Add(row.ItemArray[0].ToString());
            }
            return result;
        }

        public List<string> GetParentTypes(Guid NodeID)
        {
            DataTable dt = dc.DataSelect("select Title from " + ObjectTable + " inner join (select TypeID from " + ObjectTable + " inner join " + LinkTable + " on " + LinkTable + ".item1ID = " + ObjectTable + ".itemID where Item2ID = '" + NodeID.ToString() + "' Group By typeID) as A on A.TypeID = " + ObjectTable + ".itemID");
            List<string> result = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                result.Add(row.ItemArray[0].ToString());
            }
            return result;
        }

        public List<Node> GetChildren(Guid NodeID, string Type, List<WhereClauseItem> Where = null, List<OrderBy> OrderBy = null)
        {
            if (Where == null)
                Where = new List<WhereClauseItem>();
            if (Where.Count > 0)
                Where.Add(new WhereClauseItem(Logic.And, Field.Item1ID, Operator.Equal, NodeID));
            else
                Where.Add(new WhereClauseItem(Field.Item1ID, Operator.Equal, NodeID));
            Where.Add(new WhereClauseItem(Logic.And, Field.TypeID, Operator.Equal, GetNodeType(Type)));
            return GetChildren(Where, OrderBy);
        }

        public List<Node> GetChildren(Guid NodeID, List<WhereClauseItem> Where = null, List<OrderBy> OrderBy = null)
        {
            if (Where == null)
                Where = new List<WhereClauseItem>();
            if (Where.Count > 0)
                Where.Add(new WhereClauseItem(Logic.And, Field.Item1ID, Operator.Equal, NodeID));
            else
                Where.Add(new WhereClauseItem(Field.Item1ID, Operator.Equal, NodeID));
            return GetChildren(Where, OrderBy);
        }

        protected List<Node> GetChildren(List<WhereClauseItem> Where, List<OrderBy> OrderBy = null)
        {
            string Query = BasicNodeChildQuery;
            return DataNode(Query, Where, OrderBy);
        }

        public List<Node> GetChildrenTwoParents(Guid Parent1ID, Guid Parent2ID, string Type, List<WhereClauseItem> Where = null, List<OrderBy> OrderBy = null)
        {
            Guid TypeID = GetNodeType(Type);
            string Query = Basic2ParentNodeChildQuery(Parent1ID, Parent2ID, TypeID);
            

            if (Where == null)
                Where = new List<WhereClauseItem>();
            
            return DataNode(Query, Where, OrderBy);
        }

        public List<Node> GetParentTwoChildren(Guid Child1ID, Guid Child2ID, string Type, List<WhereClauseItem> Where = null, List<OrderBy> OrderBy = null)
        {
            Guid TypeID = GetNodeType(Type);
            
            string Query = Basic2ChildNodeParentQuery(Child1ID, Child2ID, TypeID);

            
            if (Where == null)
                Where = new List<WhereClauseItem>();

            return DataNode(Query, Where, OrderBy);
        }

        public List<Node> GetBetweenParentChild(Guid ParentID, Guid ChildID, string Type, List<WhereClauseItem> Where = null, List<OrderBy> OrderBy = null)
        {
            Guid TypeID = GetNodeType(Type);
            string Query = BasicBetweenParentChildNodeQuery(ParentID, ChildID, TypeID);

            if (Where == null)
                Where = new List<WhereClauseItem>();

            return DataNode(Query, Where, OrderBy);
        }

        #endregion

        #region Get Parent Clauses (inc protected base)

        public List<Node> GetParents(Guid NodeID, string Type, List<WhereClauseItem> Where = null, List<OrderBy> OrderBy = null)
        {
            if (Where == null)
                Where = new List<WhereClauseItem>();
            if (Where.Count > 0)
                Where.Add(new WhereClauseItem(Logic.And, Field.Item2ID, Operator.Equal, NodeID));
            else
                Where.Add(new WhereClauseItem(Field.Item2ID, Operator.Equal, NodeID));
            Where.Add(new WhereClauseItem(Logic.And, Field.TypeID, Operator.Equal, GetNodeType(Type)));
            return GetParents(Where, OrderBy);
        }

        public List<Node> GetParents(Guid NodeID, List<WhereClauseItem> Where = null, List<OrderBy> OrderBy = null)
        {
            if (Where == null)
                Where = new List<WhereClauseItem>();
            if (Where.Count > 0)
                Where.Add(new WhereClauseItem(Logic.And, Field.Item2ID, Operator.Equal, NodeID));
            else
                Where.Add(new WhereClauseItem(Field.Item2ID, Operator.Equal, NodeID));
            return GetParents(Where, OrderBy);
        }

        protected List<Node> GetParents(List<WhereClauseItem> Where, List<OrderBy> OrderBy = null)
        {
            string Query = BasicNodeParentQuery;
            return DataNode(Query, Where, OrderBy);
        }

        #endregion


        #region Link Clauses

        public NodeLink GetNodeLink(Guid ParentID, Guid ChildID)
        {
            string Query = BasicNodeLinkQuery;
            DataTable dt = NodeDataStructures.GetNodeLinkTable;
            List<WhereClauseItem> Where = new List<WhereClauseItem>();
            Where.Add(new WhereClauseItem(Field.Item1ID, Operator.Equal, ParentID));
            Where.Add(new WhereClauseItem(Logic.And, Field.Item2ID, Operator.Equal, ChildID));
            GeneralSelect(Query, Where, null, ref dt);
            if (dt.Rows.Count > 0)
                return Node_Conversions.ConvertLinkDataRow(dt.Rows[0]);
            else
                return null;
        }

        public List<NodeLink> GetAllParentNodeLinks(Guid ChildID)
        {
            string Query = BasicNodeLinkQuery;
            DataTable dt = NodeDataStructures.GetNodeLinkTable;
            List<WhereClauseItem> Where = new List<WhereClauseItem>();
            Where.Add(new WhereClauseItem(Field.Item2ID, Operator.Equal, ChildID));
            GeneralSelect(Query, Where, null, ref dt);
            if (dt.Rows.Count > 0)
            {
                List<NodeLink> Links = new List<NodeLink>();
                foreach (DataRow row in dt.Rows)
                {
                    Links.Add(Node_Conversions.ConvertLinkDataRow(row));
                }
                return Links;
            }
            else
                return null;
        }

        public List<NodeLink> GetAllChildNodeLinks(Guid ParentID)
        {
            string Query = BasicNodeLinkQuery;
            DataTable dt = NodeDataStructures.GetNodeLinkTable;
            List<WhereClauseItem> Where = new List<WhereClauseItem>();
            Where.Add(new WhereClauseItem(Field.Item1ID, Operator.Equal, ParentID));
            GeneralSelect(Query, Where, null, ref dt);
            if (dt.Rows.Count > 0)
            {
                List<NodeLink> Links = new List<NodeLink>();
                foreach (DataRow row in dt.Rows)
                {
                    Links.Add(Node_Conversions.ConvertLinkDataRow(row));
                }
                return Links;
            }
            else
                return null;
        }

        #endregion


        #region Private code

        private void CreateTypeNodes()
        {
            List<WhereClauseItem> Where = new List<WhereClauseItem>();
            Where.Add(new WhereClauseItem(Field.TypeID, Operator.Equal, Constants.TypeID));
            List<Node> TypeNames = DataSelectNode(Where, null);
            foreach (Node item in TypeNames)
            {
                TypeNodes.Add(item.Title + item.Value1.ToString(), item);
            }
            Where[0].Value = Constants.LinkTypeID;
            LinkTypeNodes = Node_Conversions.ConvertListToDictionaryString(DataSelectNode(Where, null), Field.Title);
            foreach (Node item in TypeNodes.Values)
            {
                Nodes.Add(item.ItemID, item);
            }
            foreach (Node item in LinkTypeNodes.Values)
            {
                Nodes.Add(item.ItemID, item);
            }
        }

        private List<Node> DataSelectNode(List<WhereClauseItem> Where, List<OrderBy> OrderBy = null)
        {
            string Query = BasicNodeQuery;
            return DataNode(Query, Where, OrderBy);
        }

        private List<Node> DataSelectNode(WhereClauseItem Where)
        {
            List<WhereClauseItem> item = new List<WhereClauseItem>();
            item.Add(Where);
            string Query = BasicNodeQuery;
            return DataNode(Query, item);
        }
        
        private List<Node> DataNode(string Query, List<WhereClauseItem> Where, List<OrderBy> OrderBy = null)
        {
            List<Node> Nodes = new List<Node>();
            DataTable dt = NodeDataStructures.GetNodeTable;
            GeneralSelect(Query, Where, OrderBy, ref dt);
            //List<WhereClauseItem> WhereExtended = new List<WhereClauseItem>();
            //List<Node> Extended;
            //WhereExtended.Add(new WhereClauseItem(Field.TypeID, Operator.Equal, Constants.ExtendedContentID));
            string IDs = "";
            foreach (DataRow row in dt.Rows)
            {
                // Now getting the Extended content no matter what!! Make sure unique items only:
                if (IDs.IndexOf(row["ItemID"].ToString()) == -1)
                {
                //    if (WhereExtended.Count < 2)
                //    {
                //        WhereExtended.Add(new WhereClauseItem(Logic.And, true, Field.Title, Operator.Equal, row.ItemArray[0], false));
                //    }
                //    else
                //    {
                //        WhereExtended.Add(new WhereClauseItem(Logic.Or, Field.Title, Operator.Equal, row.ItemArray[0]));
                //    }
                    Nodes.Add(Node_Conversions.ConvertDataRow(row, this.Nodes));
                    IDs += row["ItemID"].ToString();
                }
            }
            //if (WhereExtended.Count > 1)
            //{
            //    // Now get all the extended items:
            //    WhereExtended[WhereExtended.Count - 1].EndBracket = true;
            //    List<OrderBy> Order = new List<OrderBy>();
            //    Order.Add(new OrderBy(Field.Title));
            //    Order.Add(new OrderBy(Field.Value1));
            //    Extended = DataSelectNode(WhereExtended, Order);
            //    // Only need to process if we have any extended items!
            //    if (Extended.Count > 0)
            //        Nodes = ProcessExtendedItems(Nodes, Extended);
            //}
            foreach(Node item in Nodes)
            {
                item.Access = this;
            }
            return Nodes;
        }

        private List<Node> ProcessExtendedItems(List<Node> Nodes, List<Node> Extended)
        {
            // The only problem with this is that the ordering of the nodes is lost - this needs fixing!
            // Fixed now past chris - Hi from Future Chris! Well, it was only a day ago!
            List<string> IDs = new List<string>();
            foreach (Node i in Nodes)
                IDs.Add(i[Field.ItemID].ToString());
            Hashtable NodeList = Node_Conversions.ConvertList(Nodes, Field.ItemID, true);
            // 23/3/21 - This bit of code needs a huge level of optimisation - grabbing the extended data 
            // is incredibly slow if there is a lot of extended data to grab. Need to make this faster:
            StringBuilder sb = new StringBuilder();
            Node temp;
            string CurrentID = Extended[0].Title;
            temp = (Node)NodeList[Extended[0].Title];
            foreach (Node node in Extended)
            {
                if (node.Title == CurrentID)
                {
                    sb.Append(node.Content);
                }
                else
                {
                    temp.Content += sb.ToString();
                    CurrentID = Extended[0].Title;
                    sb.Clear();
                    temp = (Node)NodeList[node.Title];
                }
                // temp.Content += node.Content;
            }
            if (sb.Length > 0)
                temp.Content += sb.ToString();
            Nodes = Node_Conversions.ConvertList(NodeList, IDs);
            return Nodes;
        }

        private void GeneralSelect(string Query, List<WhereClauseItem> Where, List<OrderBy> OrderBy, ref DataTable dt)
        {
            if (Where != null && Where.Count > 0)
            {
                string WhereClause = " ";
                foreach (WhereClauseItem item in Where)
                {
                    WhereClause += ClauseToString.ConvertWhere(item, ObjectTable) + " ";
                }
                if (Query.ToLower().IndexOf("where") == -1)
                    Query += " where " + WhereClause + " ";
                else if (Query.ToLower().IndexOf("where") > Query.Length - 8)
                    Query += " " + WhereClause + " ";
                else
                    Query += " and (" + WhereClause + ") ";
            }
            if (OrderBy != null && OrderBy.Count > 0)
            {
                bool b4 = false;
                foreach (OrderBy order in OrderBy)
                {
                    if (b4 == false)
                    {
                        Query += "Order By " + order.ToString();
                        b4 = true;
                    }
                    else
                        Query += ", " + order.ToString();
                }
            }
            dt = dc.DataSelect(Query, dt);
        }

        #endregion

        #region Queries

        private string BasicNodeQuery
        {
            get
            {
                return "select " +
                ObjectTable + ".ItemID, " +
                ObjectTable + ".UserID, " +
                ObjectTable + ".TypeID, " +
                ObjectTable + ".ApplicationID, " +
                ObjectTable + ".Title, " +
                ObjectTable + ".SubContent, " +
                ObjectTable + ".Content, " +
                ObjectTable + ".Value1, " +
                ObjectTable + ".Value2, " +
                ObjectTable + ".Value3, " +
                ObjectTable + ".Date1, " +
                ObjectTable + ".Date2, " +
                ObjectTable + ".Date3, " +
                ObjectTable + ".BooleanItem, " +
                ObjectTable + ".Created, " +
                ObjectTable + ".LastUpdated, " +
                ObjectTable + ".Active " +
                "From " +
                ObjectTable + " ";
            }
        }

        private string BasicNodeLinkQuery
        {
            get
            {
                return "select " +
                    "Item1ID, " +
                    "Item2ID, " +
                    "LinkTypeID, " +
                    "ExtraInfo, " +
                    "Confirmed, " +
                    "ValueInfo, " +
                    "PathID, " +
                    "LinkCreated, " +
                    "LinkLastUpdated " +
                    "From " +
                    LinkTable + " ";
            }
        }

        private string BasicNodeParentQuery
        {
            get
            {
                return "select " +
                ObjectTable + ".ItemID, " +
                ObjectTable + ".UserID, " +
                ObjectTable + ".TypeID, " +
                ObjectTable + ".ApplicationID, " +
                ObjectTable + ".Title, " +
                ObjectTable + ".SubContent, " +
                ObjectTable + ".Content, " +
                ObjectTable + ".Value1, " +
                ObjectTable + ".Value2, " +
                ObjectTable + ".Value3, " +
                ObjectTable + ".Date1, " +
                ObjectTable + ".Date2, " +
                ObjectTable + ".Date3, " +
                ObjectTable + ".BooleanItem, " +
                ObjectTable + ".Created, " +
                ObjectTable + ".LastUpdated, " +
                ObjectTable + ".Active, " +

                LinkTable + ".Item1ID, " +
                LinkTable + ".Item2ID, " +
                LinkTable + ".LinkTypeID, " +
                LinkTable + ".ExtraInfo, " +
                LinkTable + ".Confirmed, " +
                LinkTable + ".ValueInfo, " +
                LinkTable + ".PathID, " +
                LinkTable + ".LinkCreated, " +
                LinkTable + ".LinkLastUpdated " +
                "From " +
                ObjectTable +
                " inner join " + LinkTable + " on " + LinkTable + ".Item1ID = " + ObjectTable + ".ItemID ";
            }
        }

        public string ExtendedContentData(Guid NodeID)
        {
            if (File.Exists(serverLocation+NodeID.ToString()) == true)
            {
                return File.ReadAllText(serverLocation+NodeID.ToString());
            }
            else
            {
                string Query = "select " +
                    ObjectTable + ".Content " +
                    "from " + ObjectTable +
                    " where TypeID = '" + Constants.ExtendedContentID + "' and Title = '" + NodeID.ToString() + "' order by Value1";
                StringBuilder sb = new StringBuilder();
                DataTable dt = new DataTable();
                dt = dc.DataSelect(Query, dt);
                foreach (DataRow row in dt.Rows)
                {
                    sb.Append(row.ItemArray[0].ToString());
                }
                return sb.ToString();
            }
        }

        private string BasicNodeChildQuery
        {
            get
            {
                return "select " +
                ObjectTable + ".ItemID, " +
                ObjectTable + ".UserID, " +
                ObjectTable + ".TypeID, " +
                ObjectTable + ".ApplicationID, " +
                ObjectTable + ".Title, " +
                ObjectTable + ".SubContent, " +
                ObjectTable + ".Content, " +
                ObjectTable + ".Value1, " +
                ObjectTable + ".Value2, " +
                ObjectTable + ".Value3, " +
                ObjectTable + ".Date1, " +
                ObjectTable + ".Date2, " +
                ObjectTable + ".Date3, " +
                ObjectTable + ".BooleanItem, " +
                ObjectTable + ".Created, " +
                ObjectTable + ".LastUpdated, " +
                ObjectTable + ".Active, " +

                LinkTable + ".Item1ID, " +
                LinkTable + ".Item2ID, " +
                LinkTable + ".LinkTypeID, " +
                LinkTable + ".ExtraInfo, " +
                LinkTable + ".Confirmed, " +
                LinkTable + ".ValueInfo, " +
                LinkTable + ".PathID, " +
                LinkTable + ".LinkCreated, " +
                LinkTable + ".LinkLastUpdated " +
                "from " + ObjectTable +
                " inner join " + LinkTable + " on " + LinkTable + ".Item2ID = " + ObjectTable + ".ItemID ";
            }
        }

        private string Basic2ChildNodeParentQuery(Guid Child1ID, Guid Child2ID, Guid TypeID)
        {
            return "select * from (select * from " +
                "(select " +
                ObjectTable + ".ItemID, " +
                ObjectTable + ".UserID, " +
                ObjectTable + ".TypeID, " +
                ObjectTable + ".ApplicationID, " +
                ObjectTable + ".Title, " +
                ObjectTable + ".SubContent, " +
                ObjectTable + ".Content, " +
                ObjectTable + ".Value1, " +
                ObjectTable + ".Value2, " +
                ObjectTable + ".Value3, " +
                ObjectTable + ".Date1, " +
                ObjectTable + ".Date2, " +
                ObjectTable + ".Date3, " +
                ObjectTable + ".BooleanItem, " +
                ObjectTable + ".Created, " +
                ObjectTable + ".LastUpdated, " +
                ObjectTable + ".Active From " +
                ObjectTable +
                " inner join " + LinkTable + " on " + LinkTable + ".Item1ID = " + ObjectTable + ".ItemID " +
                ") as Main inner join " +
                LinkTable + " on " +
                "Main.ItemID = " + LinkTable + ".Item1ID where Item2ID = '" + Child1ID + "' and Main.TypeID = '" + TypeID + "') As Y " +
                "inner join " + LinkTable + " on " +
                "Y.ItemID = " + LinkTable + ".Item1ID " +
                "where " + LinkTable + ".Item2ID = '" + Child2ID + "' ";
        }

        private string Basic2ParentNodeChildQuery(Guid Parent1ID, Guid Parent2ID, Guid TypeID)
        {
            return "select * from (select * from " +
                "(select " +
                ObjectTable + ".ItemID, " +
                ObjectTable + ".UserID, " +
                ObjectTable + ".TypeID, " +
                ObjectTable + ".ApplicationID, " +
                ObjectTable + ".Title, " +
                ObjectTable + ".SubContent, " +
                ObjectTable + ".Content, " +
                ObjectTable + ".Value1, " +
                ObjectTable + ".Value2, " +
                ObjectTable + ".Value3, " +
                ObjectTable + ".Date1, " +
                ObjectTable + ".Date2, " +
                ObjectTable + ".Date3, " +
                ObjectTable + ".BooleanItem, " +
                ObjectTable + ".Created, " +
                ObjectTable + ".LastUpdated, " +
                ObjectTable + ".Active From " +
                ObjectTable + " " +
                "inner join " + LinkTable + " on " + LinkTable + ".Item2ID = " + ObjectTable + ".ItemID " +
                ") as Main inner join " +
                LinkTable + " on " +
                "Main.ItemID = " + LinkTable + ".Item2ID where Item1ID = '" + Parent1ID + "' and Main.TypeID = '" + TypeID + "') As Y " +
                "inner join " + LinkTable + " on " +
                "Y.ItemID = " + LinkTable + ".Item2ID " +
                "where " + LinkTable + ".Item1ID = '" + Parent2ID + "' ";
        }

        private string BasicBetweenParentChildNodeQuery(Guid ParentID, Guid ChildID, Guid TypeID)
        {
            return "select * from (select * from " +
                "(select " +
                ObjectTable + ".ItemID, " +
                ObjectTable + ".UserID, " +
                ObjectTable + ".TypeID, " +
                ObjectTable + ".ApplicationID, " +
                ObjectTable + ".Title, " +
                ObjectTable + ".SubContent, " +
                ObjectTable + ".Content, " +
                ObjectTable + ".Value1, " +
                ObjectTable + ".Value2, " +
                ObjectTable + ".Value3, " +
                ObjectTable + ".Date1, " +
                ObjectTable + ".Date2, " +
                ObjectTable + ".Date3, " +
                ObjectTable + ".BooleanItem, " +
                ObjectTable + ".Created, " +
                ObjectTable + ".LastUpdated, " +
                ObjectTable + ".Active From " +
                ObjectTable + " " +
                ") as Main inner join " +
                LinkTable + " on " +
                "Main.ItemID = " + LinkTable + ".Item2ID where Item1ID = '" + ParentID + "' and Main.TypeID = '" + TypeID + "') As Y " +
                "inner join " + LinkTable + " on " +
                "Y.ItemID = " + LinkTable + ".Item1ID " +
                "where " + LinkTable + ".Item2ID = '" + ChildID + "' ";
        }

        #endregion
    }

    public enum DatabaseConnectionType { SQLServer, LocalMDB, Access, Remote, MySQL }
}
