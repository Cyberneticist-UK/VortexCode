using System;
using System.Data;

namespace Vortex
{
    public enum Field
    {
        Title, SubContent, Content, Value1, Value2, Value3, Date1, Date2, Date3,
        Value1X, Value1Y, Value2X, Value2Y, Value3X, Value3Y,
        BooleanItem, nullItem, ItemID, UserID, TypeID, ApplicationID, Created, LastUpdated, Active, Item1ID, Item2ID, LinkTypeID, ExtraInfo, Confirmed, ValueInfo, PathID, LinkCreated, LinkLastUpdated, JUSTUSEVALUE, LINKDATA,
        ToString
    }

    public enum LinkField
    {
        Confirmed,
        ExtraInfo,
        LinkTypeID,
        PathID,
        ValueInfo,
        LinkCreated,
        LinkLastUpdated
    }

    public enum Operator
    {
        nullItem, Equal, NotEqual, Less, LessEqual, Greater, GreaterEqual, StartsWith, EndsWith, Contains
    }

    public enum Logic
    {
        nullItem, And, Or, Not
    }

    public enum ContentType
    {
        Text, HTML, HumanVector, CompiledVector, List, BlowfishEncrypted, File, AESEncrypted, NetScript,
        TextOverflowFile, HTMLOverflowFile, HumanVectorOverflowFile, CompiledVectorOverflowFile, ListOverflowFile, NetScriptOverflowFile,
        TextFile, HTMLFile, HumanVectorFile, CompiledVectorFile, ListFile, NetScriptFile
    }
    
    public class WhereClauseItem
    {
        public Logic LogicToPrevious = Logic.nullItem;
        public bool StartBracket = false;
        public Field Field = Field.nullItem;
        public Operator Operator = Operator.nullItem;
        public object Value;
        public bool EndBracket = false;

        public WhereClauseItem() { }
        public WhereClauseItem(Field Field, Operator Operator, object Value)
        {
            this.Field = Field;
            this.Operator = Operator;
            this.Value = Value;
        }

        public WhereClauseItem(Logic LogicToPrevious, Field Field, Operator Operator, object Value)
        {
            this.LogicToPrevious = LogicToPrevious;
            this.Field = Field;
            this.Operator = Operator;
            this.Value = Value;
        }

        public WhereClauseItem(Logic LogicToPrevious, bool StartBracket, Field Field, Operator Operator, object Value, bool EndBracket)
        {
            this.StartBracket = StartBracket;
            this.LogicToPrevious = LogicToPrevious;
            this.Field = Field;
            this.Operator = Operator;
            this.Value = Value;
            this.EndBracket = EndBracket;
        }
    }

    public struct FieldValuePair
    {
        public Field Field;
        public object Value;
        public FieldValuePair(Field Field, object Value)
        {
            this.Field = Field;
            this.Value = Value;
        }
    }

    public struct OrderBy
    {
        public Field Field;
        public bool Ascending;
        public OrderBy(Field Field)
        {
            this.Field = Field;
            Ascending = false;
        }

        public OrderBy(Field Field, bool Ascending)
        {
            this.Field = Field;
            this.Ascending = Ascending;
        }

        public override string ToString()
        {
            if (Ascending)
                return Field.ToString() + " asc";
            return Field.ToString();
        }
    }

    static class NodeDataStructures
    {
        /// <summary>
        /// This is the Node table, and contains columns for links as well.
        /// Hopefully this shouldn't effect queries that don't use this.
        /// </summary>
        public static DataTable GetNodeTable
        {
            get
            {
                DataTable dt = new DataTable("Node");
                dt.Columns.Add("ItemID", typeof(Guid));
                dt.Columns.Add("UserID", typeof(Guid));
                dt.Columns.Add("TypeID", typeof(Guid));
                dt.Columns.Add("ApplicationID", typeof(Guid));
                dt.Columns.Add("Title", typeof(String));
                dt.Columns.Add("SubContent", typeof(String));
                dt.Columns.Add("Content", typeof(String));
                dt.Columns.Add("Value1", typeof(decimal));
                dt.Columns.Add("Value2", typeof(decimal));
                dt.Columns.Add("Value3", typeof(decimal));
                dt.Columns.Add("Date1", typeof(DateTime));
                dt.Columns.Add("Date2", typeof(DateTime));
                dt.Columns.Add("Date3", typeof(DateTime));
                dt.Columns.Add("BooleanItem", typeof(String));
                dt.Columns.Add("Created", typeof(DateTime));
                dt.Columns.Add("LastUpdated", typeof(DateTime));
                dt.Columns.Add("Active", typeof(String));
                // This is the link fields
                dt.Columns.Add("Item1ID", typeof(Guid));
                dt.Columns.Add("Item2ID", typeof(Guid));
                dt.Columns.Add("LinkTypeID", typeof(Guid));
                dt.Columns.Add("ExtraInfo", typeof(String));
                dt.Columns.Add("Confirmed", typeof(String));
                dt.Columns.Add("ValueInfo", typeof(decimal));
                dt.Columns.Add("PathID", typeof(Guid));
                dt.Columns.Add("LinkCreated", typeof(DateTime));
                dt.Columns.Add("LinkLastUpdated", typeof(DateTime));
                return dt;
            }
        }

        /// <summary>
        /// This is the Link table for use in queries where you are only returning the link itself.
        /// </summary>
        public static DataTable GetNodeLinkTable
        {
            get
            {
                DataTable dt = new DataTable("Link");
                dt.Columns.Add("Item1ID", typeof(Guid));
                dt.Columns.Add("Item2ID", typeof(Guid));
                dt.Columns.Add("LinkTypeID", typeof(Guid));
                dt.Columns.Add("ExtraInfo", typeof(String));
                dt.Columns.Add("Confirmed", typeof(String));
                dt.Columns.Add("ValueInfo", typeof(decimal));
                dt.Columns.Add("PathID", typeof(Guid));
                dt.Columns.Add("LinkCreated", typeof(DateTime));
                dt.Columns.Add("LinkLastUpdated", typeof(DateTime));
                return dt;
            }
        }

        /// <summary>
        /// This is the Node table with an extra "Extended field" attached, combined with the fields for a link.
        /// If Extended Field contains data,
        /// The content is longer than a single node and it should be recombined before being passed to the
        /// main system.
        /// </summary>
        public static DataTable GetNodeAndLinkTable
        {
            get
            {
                DataTable dt = new DataTable("NodeAndLink");
                dt.Columns.Add("ItemID", typeof(Guid));
                dt.Columns.Add("UserID", typeof(Guid));
                dt.Columns.Add("TypeID", typeof(Guid));
                dt.Columns.Add("ApplicationID", typeof(Guid));
                dt.Columns.Add("Title", typeof(String));
                dt.Columns.Add("SubContent", typeof(String));
                dt.Columns.Add("Content", typeof(String));
                dt.Columns.Add("Value1", typeof(decimal));
                dt.Columns.Add("Value2", typeof(decimal));
                dt.Columns.Add("Value3", typeof(decimal));
                dt.Columns.Add("Date1", typeof(DateTime));
                dt.Columns.Add("Date2", typeof(DateTime));
                dt.Columns.Add("Date3", typeof(DateTime));
                dt.Columns.Add("BooleanItem", typeof(String));
                dt.Columns.Add("Created", typeof(DateTime));
                dt.Columns.Add("LastUpdated", typeof(DateTime));
                dt.Columns.Add("Active", typeof(String));
                dt.Columns.Add("ExtendedField", typeof(String));
                dt.Columns.Add("Item1ID", typeof(Guid));
                dt.Columns.Add("Item2ID", typeof(Guid));
                dt.Columns.Add("LinkTypeID", typeof(Guid));
                dt.Columns.Add("ExtraInfo", typeof(String));
                dt.Columns.Add("Confirmed", typeof(bool));
                dt.Columns.Add("ValueInfo", typeof(decimal));
                dt.Columns.Add("PathID", typeof(Guid));
                dt.Columns.Add("LinkCreated", typeof(DateTime));
                dt.Columns.Add("LinkLastUpdated", typeof(DateTime));
                return dt;
            }
        }
    }

}
