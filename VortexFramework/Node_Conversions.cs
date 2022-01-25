using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Vortex
{
    /// <summary>
    /// This class performs operations to convert between DataRows and Nodes and work on the lists:
    /// </summary>
    public static class Node_Conversions
    {
        /// <summary>
        /// Take a row from a "Node" Datatable and turn it into a proper node!
        /// </summary>
        /// <param name="row">The row from the datatable</param>
        /// <returns>A newly minted Node</returns>
        public static Node ConvertDataRow(DataRow row, Dictionary<Guid, Node> TypeIDTable)
        {
            Node temp = new Node((Guid)row["ItemID"]);
            temp.Active = row["Active"].ToString() == "Y";
            temp.ApplicationID = (Guid)row["ApplicationID"];
            temp.BooleanItem = row["BooleanItem"].ToString() == "Y";
            temp.Content = row["Content"].ToString();
            temp.CheckedExtended = false;
            temp.Created = (DateTime)row["Created"];
            if (row["Date1"].ToString() != "")
                temp.Date1 = (DateTime)row["Date1"];
            if (row["Date2"].ToString() != "")
                temp.Date2 = (DateTime)row["Date2"];
            if (row["Date3"].ToString() != "")
                temp.Date3 = (DateTime)row["Date3"];
            temp.LastUpdated = (DateTime)row["LastUpdated"];
            temp.SubContent = row["SubContent"].ToString();
            temp.Title = row["Title"].ToString();
            temp.TypeID = (Guid)row["TypeID"];
            temp.UserID = (Guid)row["UserID"];
            if (row["Value1"] != null && row["Value1"].ToString() != "")
                temp.Value1 = Convert.ToDecimal(row["Value1"]);
            if (row["Value2"] != null && row["Value2"].ToString() != "")
                temp.Value2 = Convert.ToDecimal(row["Value2"]);
            if (row["Value3"] != null && row["Value3"].ToString() != "")
                temp.Value3 = Convert.ToDecimal(row["Value3"]);
            
                if (row.Table.Columns.Contains("Item1ID") && row["Item1ID"] != null && row["Item1ID"].ToString() != "")
                {
                    temp.LINKDATA = true;
                    temp.Item1ID = (Guid)row["Item1ID"];
                    temp.Item2ID = (Guid)row["Item2ID"];
                    temp.LinkTypeID = (Guid)row["LinkTypeID"];
                    temp.ExtraInfo = row["ExtraInfo"].ToString();
                    temp.Confirmed = row["Confirmed"].ToString() == "Y";
                    if (row["ValueInfo"] != null && row["ValueInfo"].ToString() != "")
                        temp.ValueInfo = Convert.ToDecimal(row["ValueInfo"]);
                    if (row["PathID"] != null && row["PathID"].ToString() != "")
                        temp.PathID = (Guid)row["PathID"];
                    temp.LinkCreated = (DateTime)row["LinkCreated"];
                    temp.LinkLastUpdated = (DateTime)row["LinkLastUpdated"];
                }
            

            // Need to get the names for the fields:
            if (TypeIDTable.ContainsKey(temp.TypeID))
                temp.Names = TypeIDTable[temp.TypeID].Content;
            return temp;
        }

        /// <summary>
        /// Take a row from a "NodeLink" Datatable and turn it into a proper nodelink!
        /// </summary>
        /// <param name="row">The row from the datatable</param>
        /// <returns>A newly minted NodeLink</returns>
        public static NodeLink ConvertLinkDataRow(DataRow row)
        {
            NodeLink temp = new NodeLink();
            temp.Item1ID = (Guid)row["Item1ID"];
            temp.Item2ID = (Guid)row["Item2ID"];
            temp.Confirmed = (row["Confirmed"].ToString() == "Y");
            temp.ExtraInfo = row["ExtraInfo"].ToString();
            temp.LinkCreated = (DateTime)row["LinkCreated"];
            temp.LinkLastUpdated = (DateTime)row["LinkLastUpdated"];
            temp.LinkTypeID = (Guid)row["LinkTypeID"];
            if (row["PathID"].ToString() != "")
                temp.PathID = (Guid)row["PathID"];
            if (row["ValueInfo"] != null && row["ValueInfo"].ToString() != "")
                temp.ValueInfo = Convert.ToDecimal(row["ValueInfo"]);
            return temp;
        }

        /// <summary>
        /// Take a list of nodes and turn it into a HashTable
        /// </summary>
        /// <param name="Nodes">The list of nodes</param>
        /// <param name="FieldName">Which field to make the key</param>
        /// <param name="FieldAsAString">Should the key be a string? Important if key is a guid and trying to find a string!</param>
        /// <returns>a Hashtable of the nodes!</returns>
        public static Hashtable ConvertList(List<Node> Nodes, Field FieldName, bool FieldAsAString)
        {
            if (Nodes == null)
                return new Hashtable();
            Hashtable X = new Hashtable();
            if (FieldAsAString)
            {
                foreach (Node item in Nodes)
                {
                    if (X.ContainsKey(Convert.ToString(item[FieldName])) == false)
                        X.Add(Convert.ToString(item[FieldName]), item);
                }
            }
            else
            {
                foreach (Node item in Nodes)
                {
                    if (X.ContainsKey(item[FieldName]) == false)
                        X.Add(item[FieldName], item);
                }
            }
            return X;
        }

        /// <summary>
        /// Take a list of nodes and turn it into a Dictionary
        /// </summary>
        /// <param name="Nodes">The list of nodes</param>
        /// <param name="FieldName">Which field to make the key</param>
        /// <returns>a Dictionary of the nodes!</returns>
        public static Dictionary<string, Node> ConvertListToDictionaryString(List<Node> Nodes, Field FieldName)
        {
            if (Nodes == null)
                return new Dictionary<string, Node>();
            Dictionary<string, Node> X = new Dictionary<string, Node>();

            foreach (Node item in Nodes)
            {
                if (X.ContainsKey(Convert.ToString(item[FieldName])) == false)
                    X.Add(Convert.ToString(item[FieldName]), item);
            }

            return X;
        }

        /// <summary>
        /// Take a list of nodes and turn it into a Dictionary
        /// </summary>
        /// <param name="Nodes">The list of nodes</param>
        /// <param name="FieldName">Which field to make the key</param>
        /// <returns>a Dictionary of the nodes!</returns>
        public static Dictionary<Guid, Node> ConvertListToDictionaryGuid(List<Node> Nodes, Field FieldName)
        {
            if (Nodes == null)
                return new Dictionary<Guid, Node>();
            Dictionary<Guid, Node> X = new Dictionary<Guid, Node>();

            foreach (Node item in Nodes)
            {
                if (X.ContainsKey((Guid)(item[FieldName])) == false)
                    X.Add((Guid)(item[FieldName]), item);
            }

            return X;
        }



        /// <summary>
        /// Convert a hashtable back into a list of nodes!
        /// </summary>
        /// <param name="Nodes">The hashtable to turn into a list</param>
        /// <returns>The list!</returns>
        public static List<Node> ConvertList(Hashtable Nodes)
        {
            List<Node> X = new List<Node>();
            foreach (Node item in Nodes.Values)
            {
                X.Add(item);
            }
            return X;
        }

        /// <summary>
        /// Convert a hashtable back into a list of nodes in a correct order!
        /// </summary>
        /// <param name="Nodes">The Node Hashtable</param>
        /// <param name="OrderedID">The ID's in the order wanted</param>
        /// <returns></returns>
        public static List<Node> ConvertList(Hashtable Nodes, List<string> OrderedID)
        {
            List<Node> X = new List<Node>();
            foreach (string item in OrderedID)
            {
                X.Add((Node)Nodes[item]);
            }
            return X;
        }

        public static string NodeData(Node item, Node TypeNode)
        {
            string[] TypeFields = TypeNode.Content;
            string X = "ItemID:" + item.ItemID.ToString() + "\r\n" +
            "UserID:" + item.UserID.ToString() + "\r\n" +
            "TypeID:" + item.TypeID.ToString() + " (" + TypeNode.Title + ")\r\n" +
            "ApplicationID:" + item.ApplicationID.ToString() + "\r\n" +
            TypeFields[0] + " (Title):" + item.Title.ToString() + "\r\n" +
            TypeFields[1] + " (SubContent):" + item.SubContent.ToString() + "\r\n" +
            TypeFields[2] + " (Content):" + item.Content.ToString() + "\r\n";
            if (item.Value1 != null)
                X += TypeFields[3] + " (Value1):" + item.Value1 + "\r\n";
            if (item.Value2 != null)
                X += TypeFields[4] + " (Value2):" + item.Value2 + "\r\n";
            if (item.Value3 != null)
                X += TypeFields[5] + " (Value3):" + item.Value3 + "\r\n";
            X += TypeFields[6] + " (Date1):" + item.Date1.ToString() + "\r\n" +
            TypeFields[7] + " (Date2):" + item.Date2.ToString() + "\r\n" +
            TypeFields[8] + " (Date3):" + item.Date3.ToString() + "\r\n" +
            TypeFields[9] + " (BooleanItem):" + item.BooleanItem.ToString() + "\r\n" +
            "Created:" + item.Created.ToString() + "\r\n" +
            "LastUpdated:" + item.LastUpdated.ToString() + "\r\n" +
            "Active:" + item.Active.ToString();
            return X;
        }

        /// <summary>
        /// Reorganise a list to sort it by a field:
        /// </summary>
        /// <param name="Nodes">The list</param>
        /// <param name="OrderField">Which field to order by</param>
        /// <returns>The sorted list!</returns>
        public static List<Node> OrderList(List<Node> Nodes, Field OrderField)
        {
            if (Nodes.Count < 2)
                return Nodes;
            // return Nodes.OrderBy(o => o[OrderField]).ToList();
            // Version 2 - the original wasn't working with dates!!!
            TreeSort Sort = new TreeSort();
            foreach(Node item in Nodes)
            {
                Sort.Push(item, OrderField);
            }
            return Sort.SortedList();
        }

        /// <summary>
        /// Reorganise a list to sort it by a field, but does it in opposite order:
        /// </summary>
        /// <param name="Nodes">The list</param>
        /// <param name="OrderField">Which field to order by</param>
        /// <returns>The sorted list!</returns>
        public static List<Node> OrderListDesc(List<Node> Nodes, Field OrderField)
        {
            Nodes = OrderList(Nodes, OrderField);
            Nodes.Reverse();
            return Nodes;
        }

        public static List<Node> NodesNotInListTwo(List<Node> ListOne, List<Node> ListTwo)
        {
            List<Node> results = new List<Node>();
            string IDs = "";
            foreach (Node item in ListTwo)
            {
                IDs += item.ItemID.ToString();
            }
            foreach (Node item in ListOne)
            {
                if (IDs.IndexOf(item.ItemID.ToString()) == -1)
                    results.Add(item);
            }
            return results;
        }

        public static string GetTypeFieldName(Node TypeNode, Field NodeField)
        {
            // Take out the array:
            // Title, SubContent, Content, Value1, Value2, Value3, Date1, Date2, Date3, BooleanItem
            string[] Contents = TypeNode.Content;
            switch (NodeField)
            {
                case Field.Title:
                    return Contents[0];
                case Field.SubContent:
                    return Contents[1];
                case Field.Content:
                    return Contents[2];
                case Field.Value1:
                    return Contents[3];
                case Field.Value2:
                    return Contents[4];
                case Field.Value3:
                    return Contents[5];
                case Field.Date1:
                    return Contents[6];
                case Field.Date2:
                    return Contents[7];
                case Field.Date3:
                    return Contents[8];
                case Field.BooleanItem:
                    return Contents[9];
            }
            return "";

        }

        /// <summary>
        /// This writes out a Node in the correct format for a file if we are preserving the GUIDs.
        /// </summary>
        /// <param name="TypeNode">The Type Node for the Item Node</param>
        /// <param name="item">The node itself</param>
        /// <returns></returns>
        public static string ConvertToNodeData(Node TypeNode, Node item)
        {
            string Data = item.ItemID.ToString() + "\t" + item.ApplicationID.ToString() + "\t" + item.UserID.ToString() + "\t" + TypeNode.Title + "\t" + item.Created.ToString("dd-MMM-yyyy HH:mm:ss") + "\t" + item.LastUpdated.ToString("dd-MMM-yyyy HH:mm:ss");
            if (GetTypeFieldName(TypeNode, Field.Title) != "")
                Data += "\t" + item.Title.Replace("\t", "*~tab~*").Replace("\r", "").Replace("\n", "*~newLine~*");
            if (GetTypeFieldName(TypeNode, Field.SubContent) != "")
                Data += "\t" + item.SubContent.Replace("\t", "*~tab~*").Replace("\r", "").Replace("\n", "*~newLine~*");
            if (GetTypeFieldName(TypeNode, Field.Content) != "")
                Data += "\t" + item.Content.ToString().Replace("\t", "*~tab~*").Replace("\r", "").Replace("\n", "*~newLine~*");
            if (GetTypeFieldName(TypeNode, Field.Value1) != "")
                if (item.Value1 != null)
                    Data += "\t" + item.Value1.ToString();
                else
                    Data += "\t";
            if (GetTypeFieldName(TypeNode, Field.Value2) != "")
                if (item.Value2 != null)
                    Data += "\t" + item.Value2.ToString();
                else
                    Data += "\t";
            if (GetTypeFieldName(TypeNode, Field.Value3) != "")
                if (item.Value3 != null)
                    Data += "\t" + item.Value3.ToString();
                else
                    Data += "\t";
            if (GetTypeFieldName(TypeNode, Field.Date1) != "")
                if (item.Date1 != null)
                    Data += "\t" + item.Date1.ToString("dd-MMM-yyyy HH:mm:ss");
                else
                    Data += "\t";
            if (GetTypeFieldName(TypeNode, Field.Date2) != "")
                if (item.Date2 != null)
                    Data += "\t" + item.Date2.ToString("dd-MMM-yyyy HH:mm:ss");
                else
                    Data += "\t";
            if (GetTypeFieldName(TypeNode, Field.Date3) != "")
                if (item.Date3 != null)
                    Data += "\t" + item.Date3.ToString("dd-MMM-yyyy HH:mm:ss");
                else
                    Data += "\t";
            if (GetTypeFieldName(TypeNode, Field.BooleanItem) != "")
                Data += "\t" + item.BooleanItem.ToString();
            return Data;
        }

        public static Node ConvertFromNodeDataWithGUID(string NodeData, List<Node> Types)
        {
            string[] Parts = NodeData.Split(new char[] { '\t' });
            Node item = new Node(Guid.Parse(Parts[0]));
            Node TypeNode = new Node(Constants.BlankID);
            TypeNode.Title = "Type";
            TypeNode.Content = "Type Name,,Type Fields,,,,,,,";
            item.ApplicationID = Guid.Parse(Parts[1]);
            item.UserID = Guid.Parse(Parts[2]);
            if (Parts[3] == "Type")
                item.TypeID = Constants.BlankID;
            else
            {
                foreach (Node T in Types)
                {
                    if (T.Title == Parts[3])
                    {
                        TypeNode = T;
                        item.TypeID = T.ItemID;
                        break;
                    }
                }
            }
            item.Created = Convert.ToDateTime(Parts[4]);
            item.LastUpdated = Convert.ToDateTime(Parts[5]);
            int counter = 6;
            if (GetTypeFieldName(TypeNode, Field.Title) != "")
            {
                item.Title = Parts[counter].Replace("*~tab~*", "\t").Replace("*~newLine~*", "\r\n");
                counter++;
            }

            if (GetTypeFieldName(TypeNode, Field.SubContent) != "")
            {
                item.SubContent = Parts[counter].Replace("*~tab~*", "\t").Replace("*~newLine~*", "\r\n");
                counter++;
            }

            if (GetTypeFieldName(TypeNode, Field.Content) != "")
            {
                item.Content = Parts[counter].Replace("*~tab~*", "\t").Replace("*~newLine~*", "\r\n");
                counter++;
            }

            if (GetTypeFieldName(TypeNode, Field.Value1) != "")
            {
                if (Parts[counter] != "")
                    item.Value1 = Convert.ToDecimal(Parts[counter]);
                counter++;
            }

            if (GetTypeFieldName(TypeNode, Field.Value2) != "")
            {
                if (Parts[counter] != "")
                    item.Value2 = Convert.ToDecimal(Parts[counter]);
                counter++;
            }

            if (GetTypeFieldName(TypeNode, Field.Value3) != "")
            {
                if (Parts[counter] != "")
                    item.Value3 = Convert.ToDecimal(Parts[counter]);
                counter++;
            }

            if (GetTypeFieldName(TypeNode, Field.Date1) != "")
            {
                if (Parts[counter] != "")
                    item.Date1 = Convert.ToDateTime(Parts[counter]);
                counter++;
            }
            if (GetTypeFieldName(TypeNode, Field.Date2) != "")
            {
                if (Parts[counter] != "")
                    item.Date2 = Convert.ToDateTime(Parts[counter]);
                counter++;
            }
            if (GetTypeFieldName(TypeNode, Field.Date3) != "")
            {
                if (Parts[counter] != "")
                    item.Date3 = Convert.ToDateTime(Parts[counter]);
                counter++;
            }

            if (GetTypeFieldName(TypeNode, Field.BooleanItem) != "")
            {
                item.BooleanItem = bool.Parse(Parts[counter]);
                counter++;
            }

            return item;
        }

        public static string ConvertToNodeLinkData(NodeLink item)
        {
            string Data = item.Item1ID + "\t" +
                item.Item2ID + "\t" + item.LinkTypeID.ToString() +
                "\t" + item.LinkCreated.ToString("dd-MMM-yyyy HH:mm:ss") + "\t" + item.LinkLastUpdated.ToString("dd-MMM-yyyy HH:mm:ss");
            if (item.PathID != null)
                Data += "\t" + item.PathID.ToString();
            else
                Data += "\tNull";

            if (item.ExtraInfo != null)
                Data += "\t" + item.ExtraInfo.ToString().Replace("\t", "*~tab~*").Replace("\r", "").Replace("\n", "*~newLine~*");
            else
                Data += "\tNull";

            if (item.ValueInfo != null)
                Data += "\t" + item.ValueInfo.ToString();
            else
                Data += "\tNull";

            Data += "\t" + item.Confirmed.ToString();
            return Data;
        }

        public static NodeLink ConvertFromNodeLinkData(string item)
        {
            NodeLink Data = new NodeLink();
            string[] Parts = item.Split(new char[] { '\t' });
            Data.Item1ID = Guid.Parse(Parts[0]);
            Data.Item2ID = Guid.Parse(Parts[1]);
            Data.LinkTypeID = Guid.Parse(Parts[2]);
            Data.LinkCreated = Convert.ToDateTime(Parts[3]);
            Data.LinkLastUpdated = Convert.ToDateTime(Parts[4]);
            Data.PathID = Guid.Parse(Parts[5]);
            if (Parts[6] != "Null")
                Data.ExtraInfo = Parts[6].Replace("*~tab~*", "\t").Replace("*~newLine~*", "\r\n");
            if (Parts[7] != "Null")
                Data.ValueInfo = Convert.ToDecimal(Parts[7]);
            Data.Confirmed = bool.Parse(Parts[8]);
            return Data;
        }

    }

    /// <summary>
    /// Used to change the new structured where and orderby clauses into SQL equivalents
    /// </summary>
    public static class ClauseToString
    {
        /// <summary>
        /// Convert a where clause into a format suitable to sql query:
        /// </summary>
        /// <param name="item">the where clause</param>
        /// <param name="Table">The table the where item comes from</param>
        /// <returns>the fragment of a SQL where clause</returns>
        public static string ConvertWhere(WhereClauseItem item, string Table)
        {
            string Response = "";
            if (item.Field != Field.nullItem)
            {
                if (item.LogicToPrevious != Logic.nullItem)
                    Response += item.LogicToPrevious.ToString() + " ";
                if (item.StartBracket)
                    Response += "(";
                if (item.Field == Field.JUSTUSEVALUE)
                {
                    Response += item.Value.ToString();
                }
                else if ((int)item.Field < 18)
                    Response += Table + "." + item.Field.ToString() + OperatorAsString(item.Operator) + ValueAsString(item.Field, item.Value, item.Operator);
                else
                    Response += item.Field.ToString() + OperatorAsString(item.Operator) + ValueAsString(item.Field, item.Value, item.Operator);
                if (item.EndBracket)
                    Response += ")";
            }
            return Response;
        }

        /// <summary>
        /// Changes the field in a node into the database-suitable equivalent!
        /// </summary>
        /// <param name="Field">Which field from the database</param>
        /// <param name="Value">The value as taken from the node</param>
        /// <returns>a string representation of the value</returns>
        public static string ValueAsString(Field Field, object Value, Operator Current)
        {
            switch (Field)
            {
                case Field.Value1:
                    return ((NodeValue)Value).ToString();
                case Field.Value2:
                    return ((NodeValue)Value).ToString();
                case Field.Value3:
                    return ((NodeValue)Value).ToString();
                case Field.Active:
                    if ((bool)Value == true)
                        return "'Y'";
                    else
                        return "'N'";
                case Field.BooleanItem:
                    if ((bool)Value == true)
                        return "'Y'";
                    else
                        return "'N'";
                case Field.ApplicationID:
                    return "'" + ((Guid)Value).ToString() + "'";
                case Field.Confirmed:
                    if ((bool)Value == true)
                        return "'Y'";
                    else
                        return "'N'";
                case Field.Content:
                    if (Current == Operator.StartsWith)
                        return "'" + Convert.ToString(Value).Replace("'", "''") + "%'";
                    if (Current == Operator.EndsWith)
                        return "'%" + Convert.ToString(Value).Replace("'", "''") + "'";
                    if (Current == Operator.Contains)
                        return "'%" + Convert.ToString(Value).Replace("'", "''") + "%'";
                    return "'" + Convert.ToString(Value).Replace("'", "''") + "'";
                case Field.Created:
                    return "'" + ((DateTime)Value).ToString("dd-MMM-yyyy HH:mm:ss") + "'";
                case Field.Date1:
                    return "'" + ((DateTime)Value).ToString("dd-MMM-yyyy HH:mm:ss") + "'";
                case Field.Date2:
                    return "'" + ((DateTime)Value).ToString("dd-MMM-yyyy HH:mm:ss") + "'";
                case Field.Date3:
                    return "'" + ((DateTime)Value).ToString("dd-MMM-yyyy HH:mm:ss") + "'";
                case Field.ExtraInfo:
                    if (Current == Operator.StartsWith)
                        return "'" + Convert.ToString(Value).Replace("'", "''") + "%'";
                    if (Current == Operator.EndsWith)
                        return "'%" + Convert.ToString(Value).Replace("'", "''") + "'";
                    if (Current == Operator.Contains)
                        return "'%" + Convert.ToString(Value).Replace("'", "''") + "%'";
                    return "'" + Convert.ToString(Value).Replace("'", "''") + "'";
                case Field.Item1ID:
                    return "'" + Convert.ToString(Value) + "'";
                case Field.Item2ID:
                    return "'" + Convert.ToString(Value) + "'";
                case Field.ItemID:
                    return "'" + Convert.ToString(Value) + "'";
                case Field.LastUpdated:
                    return "'" + ((DateTime)Value).ToString("dd-MMM-yyyy HH:mm:ss") + "'";
                case Field.LinkCreated:
                    return "'" + ((DateTime)Value).ToString("dd-MMM-yyyy HH:mm:ss") + "'";
                case Field.LinkLastUpdated:
                    return "'" + ((DateTime)Value).ToString("dd-MMM-yyyy HH:mm:ss") + "'";
                case Field.LinkTypeID:
                    return "'" + Convert.ToString(Value) + "'";
                case Field.PathID:
                    return "'" + Convert.ToString(Value) + "'";
                case Field.SubContent:
                    if (Current == Operator.StartsWith)
                        return "'" + Convert.ToString(Value).Replace("'", "''") + "%'";
                    if (Current == Operator.EndsWith)
                        return "'%" + Convert.ToString(Value).Replace("'", "''") + "'";
                    if (Current == Operator.Contains)
                        return "'%" + Convert.ToString(Value).Replace("'", "''") + "%'";
                    return "'" + Convert.ToString(Value).Replace("'", "''") + "'";
                case Field.Title:
                    if (Current == Operator.StartsWith)
                        return "'" + Convert.ToString(Value).Replace("'", "''") + "%'";
                    if (Current == Operator.EndsWith)
                        return "'%" + Convert.ToString(Value).Replace("'", "''") + "'";
                    if (Current == Operator.Contains)
                        return "'%" + Convert.ToString(Value).Replace("'", "''") + "%'";
                    return "'" + Convert.ToString(Value).Replace("'", "''") + "'";
                case Field.TypeID:
                    return "'" + Convert.ToString(Value) + "'";
                case Field.UserID:
                    return "'" + Convert.ToString(Value) + "'";
                default:
                    return "";
            }
        }

        /// <summary>
        /// In a where clause, the operator is in words and this changes 
        /// </summary>
        /// <param name="Op">The operator</param>
        /// <returns>The string equivalent</returns>
        public static string OperatorAsString(Operator Op)
        {
            switch (Op)
            {
                case Operator.Equal:
                    return "=";
                case Operator.Greater:
                    return ">";
                case Operator.GreaterEqual:
                    return ">=";
                case Operator.Less:
                    return "<";
                case Operator.LessEqual:
                    return "<=";
                case Operator.NotEqual:
                    return "!=";
                case Operator.Contains:
                    return " like ";
                case Operator.EndsWith:
                    return " like ";
                case Operator.StartsWith:
                    return " like ";
                default:
                    return "";
            }
        }
    }

}
