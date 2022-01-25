using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Vortex
{
    public static class NetCreate
    {
        /// <summary>
        /// Creates a new Netelligence database by scripting the tables in the named database. This returns the settings Hashtable with the values that have been entered, if the database has correctly scripted. Currently only designed around using MS SQL Server.
        /// </summary>
        /// <param name="DataSource">Database Server to use</param>
        /// <param name="DataCatalogue">Database on server to use</param>
        /// <param name="ObjectTable">What to call the node table</param>
        /// <param name="LinkTable">What to call the Link table</param>
        /// <param name="ContentLength">How long in bytes the content field should be</param>
        /// <param name="ServerCode">What the code is for this server (default 0000)</param>
        /// <param name="DatabaseUser">The username for the database (if required)</param>
        /// <param name="DatabasePwd">The password for the database (if required)</param>
        /// <returns></returns>
        public static void CreateNetelligenceData(string DataSource, string DataCatalogue, string ObjectTable, string LinkTable, int ContentLength = 1024, string ServerCode = "0000", string DatabaseUser = null, string DatabasePwd = null)
        {
            Comms_DatabaseSQLServer SQL = new Comms_DatabaseSQLServer(DataSource, DataCatalogue, DatabaseUser, DatabasePwd);
            // Connect to the database:
            // Now Script the tables:
            // The Node Table:
            SQL.DataNonSelect("CREATE TABLE [dbo].[" + ObjectTable + "]" +
                "([ItemID][uniqueidentifier] NOT NULL, [UserID][uniqueidentifier] NOT NULL, " +
                "[TypeID][uniqueidentifier] NOT NULL, [ApplicationID][uniqueidentifier] NOT NULL, " +
                "[Title][varchar](512) NULL,[SubContent][varchar](512) NULL,[Content][varchar](" + ContentLength.ToString() + ") NULL, " +
                "[Value1][float] NULL,[Value2][float] NULL,[Value3][float] NULL, " +
                "[Date1][datetime] NULL,[Date2][datetime] NULL,[Date3][datetime] NULL,[BooleanItem][char](1) NULL, " +
                "[Created][datetime] NOT NULL CONSTRAINT [DF_" + ObjectTable + "_Created]  DEFAULT (getdate()),[LastUpdated][datetime] NOT NULL CONSTRAINT [DF_" + ObjectTable + "_LastUpdated]  DEFAULT (getdate()),[Active][char](1) NOT NULL, " +
                "CONSTRAINT[PK_" + ObjectTable + "] PRIMARY KEY CLUSTERED([ItemID] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]");
            // The Link Table:
            SQL.DataNonSelect("CREATE TABLE [dbo].[" + LinkTable + "]" +
                "([Item1ID][uniqueidentifier] NOT NULL,[Item2ID][uniqueidentifier] NOT NULL, " +
                "[LinkTypeID][uniqueidentifier] NOT NULL,[ExtraInfo][varchar](300) NULL,[Confirmed][char](1) NULL, " +
                "[ValueInfo][float] NULL,[PathID][uniqueidentifier] NULL, " +
                "[LinkCreated][datetime] NOT NULL CONSTRAINT[DF_" + LinkTable + "_LinkCreated]  DEFAULT(getdate()), " +
                "[LinkLastUpdated][datetime] NOT NULL CONSTRAINT[DF_" + LinkTable + "_LinkLastUpdated]  DEFAULT(getdate()), " +
                "CONSTRAINT[PK_" + LinkTable + "] PRIMARY KEY CLUSTERED([Item1ID] ASC,[Item2ID] ASC,[LinkTypeID] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]");
            // And we should be done!
        }

        public static bool ExportNetelligenceData(string ObjectTable, string LinkTable, string FileName, string DataSource, string DataCatalogue, string DatabaseUser, string DatabasePwd, bool preserveGUID, string EncryptPassword = null)
        {
            Node_AccessLayer Net = new Node_AccessLayer("", DatabaseConnectionType.SQLServer, ObjectTable, LinkTable, DataSource, DataCatalogue, DatabaseUser, DatabasePwd);
            // We are going to grab everything, so start with the Types:
            List<Node> FullList = new List<Node>();
            List<Node> Types = Net.GetNodesByType("Type");
            Types = Node_Conversions.OrderList(Types, Field.ItemID);
            foreach (Node item in Types)
                FullList.Add(item);
            Node TypeNode = Net.GetNode(Constants.BlankID);
            StringBuilder sbType = new StringBuilder();
            StringBuilder sbNodes = new StringBuilder();
            StringBuilder sbLinks = new StringBuilder();
            sbType.AppendLine("***Type***");
            sbNodes.AppendLine("***Node***");
            sbLinks.AppendLine("***Link***");
            Hashtable LinkRecords = new Hashtable();
            foreach (Node item in Types)
            {
                sbType.AppendLine(Node_Conversions.ConvertToNodeData(TypeNode, item));
                if (item.Title != "Type")
                {
                    List<Node> TempNodes = Net.GetNodesByType(item.Title);
                    foreach (Node SubItem in TempNodes)
                    {
                        FullList.Add(SubItem);
                        sbNodes.AppendLine(Node_Conversions.ConvertToNodeData(item, SubItem));
                        List<NodeLink> Links = Net.GetAllParentNodeLinks(SubItem.ItemID);
                        if (Links != null)
                            foreach (NodeLink link in Links)
                            {
                                if (LinkRecords.ContainsKey(link.Item1ID.ToString() + ":" + link.Item2ID.ToString()) == false)
                                {
                                    LinkRecords.Add(link.Item1ID.ToString() + ":" + link.Item2ID.ToString(), link);
                                }
                            }
                        Links = Net.GetAllChildNodeLinks(SubItem.ItemID);
                        if (Links != null)
                            foreach (NodeLink link in Links)
                            {
                                if (LinkRecords.ContainsKey(link.Item1ID.ToString() + ":" + link.Item2ID.ToString()) == false)
                                {
                                    LinkRecords.Add(link.Item1ID.ToString() + ":" + link.Item2ID.ToString(), link);
                                }
                            }
                    }
                }
            }

            foreach (NodeLink link in LinkRecords.Values)
            {
                sbLinks.AppendLine(Node_Conversions.ConvertToNodeLinkData(link));
            }
            string FileData = sbType.ToString() + sbNodes.ToString() + sbLinks.ToString();
            if (preserveGUID == false)
            {
                // Need to convert all the nodes to numbers!!
                int counter = 0;
                foreach (Node item in FullList)
                {
                    FileData = FileData.Replace(item.ItemID.ToString(), counter.ToString());
                    counter++;
                }
            }
            System.IO.StreamWriter sw = new System.IO.StreamWriter(FileName);
            sw.WriteLine("***Data***");
            sw.WriteLine("NetworkGUID:" + preserveGUID.ToString());
            sw.Write(FileData);
            sw.Close();
            return true;
        }

        public static bool ImportNetelligenceData(string ObjectTable, string LinkTable, string FileName, string DataSource, string DataCatalogue, string DatabaseUser, string DatabasePwd, string EncryptPassword = null)
        {
            // Going to import this file into the database. hold on to your hats!!
            Node_AccessLayer Net = new Node_AccessLayer("", DatabaseConnectionType.SQLServer, ObjectTable, LinkTable, DataSource, DataCatalogue, DatabaseUser, DatabasePwd);

            using (StreamReader sr = new StreamReader(FileName))
            {
                // Is this a valid file? First line should be ***Data***:
                string Data = sr.ReadLine();
                //bool preserveGUID = true;
                if (Data == "***Data***")
                {
                    // It's valid: next line says whether it is GUID or not:
                    Data = sr.ReadLine();
                    //if (Data == "NetworkGUID:False")
                    //    preserveGUID = false;
                    // next line should be Type list:
                    Data = sr.ReadLine();
                    List<Node> Types = new List<Node>();
                    List<Node> Nodes = new List<Node>();
                    List<NodeLink> Links = new List<NodeLink>();
                    if (Data == "***Type***")
                    {

                        Data = sr.ReadLine();
                        while (Data != "***Node***")
                        {
                            Node item = Node_Conversions.ConvertFromNodeDataWithGUID(Data, Types);
                            Types.Add(item);
                            Data = sr.ReadLine();
                        }
                        // Now to move onto the other nodes!

                        Data = sr.ReadLine();
                        while (Data != "***Link***")
                        {
                            Node item = Node_Conversions.ConvertFromNodeDataWithGUID(Data, Types);
                            Nodes.Add(item);
                            Data = sr.ReadLine();
                        }
                        // Now for the links!
                        while (sr.EndOfStream == false)
                        {
                            Links.Add(Node_Conversions.ConvertFromNodeLinkData(sr.ReadLine()));
                        }
                        // Complete!
                        sr.Close();
                    }
                    // Now for the fun part - Inserting these into the database!
                    foreach (Node item in Types)
                    {
                        Net.CreateSpecificEmpty(item.ItemID);
                        Net.SaveNode(item, item.ApplicationID, null);
                    }
                    foreach (Node item in Nodes)
                    {
                        Net.CreateSpecificEmpty(item.ItemID);
                        Net.SaveNode(item, item.ApplicationID, null);
                    }
                    // And the links:
                    foreach (NodeLink Link in Links)
                    {
                        Net.SaveLink(Link.Item1ID, Link.Item2ID, Link.LinkTypeID);
                        Net.UpdateLink(Link, LinkField.LinkCreated);
                        Net.UpdateLink(Link, LinkField.LinkLastUpdated);
                        if (Link.ExtraInfo != null)
                            Net.UpdateLink(Link, LinkField.ExtraInfo);
                        if (Link.ValueInfo != null)
                            Net.UpdateLink(Link, LinkField.ValueInfo);
                        if (Link.PathID != Constants.BlankID)
                            Net.UpdateLink(Link, LinkField.PathID);
                        Net.UpdateLink(Link, LinkField.Confirmed);
                    }
                }
            }
            return true;
        }
    }
}
