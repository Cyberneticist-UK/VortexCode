using System;
using System.Collections.Generic;
using System.Drawing;

namespace Vortex
{
    public class Node
    {
        public NodeValue Changed = 0;
        #region Public Fields
        /// <summary>
        /// The current Item ID. This is unique for every node.
        /// </summary>
        public Guid ItemID { get { return _ItemID; } set { Changed[(int)Field.ItemID] = true; _ItemID = value; } }
        private Guid _ItemID = System.Guid.NewGuid();
        /// <summary>
        /// UserID is the identifier for the initial creator of this object 
        /// </summary>
        public Guid UserID { get { return _UserID; } set { Changed[(int)Field.UserID] = true; _UserID = value; } }
        private Guid _UserID = Constants.BlankID;
        /// <summary>
        /// What type of object this is - a UID from the "Type" record
        /// </summary>
        public Guid TypeID { get { return _TypeID; } set { Changed[(int)Field.TypeID] = true; _TypeID = value; } }
        private Guid _TypeID = Constants.BlankID;
        /// <summary>
        /// Which Application Created this object (UID) from the "Application" record
        /// </summary>
        public Guid ApplicationID { get { return _ApplicationID; } set { Changed[(int)Field.ApplicationID] = true; _ApplicationID = value; } }
        private Guid _ApplicationID = Constants.BlankID;
        /// <summary>
        /// When this object was created
        /// </summary>
        public DateTime Created { get { return _Created; } set { Changed[(int)Field.Created] = true; _Created = value; } }
        private DateTime _Created = System.DateTime.Now;
        /// <summary>
        /// When this object was last updated
        /// </summary>
        public DateTime LastUpdated { get { return _LastUpdated; } set { Changed[(int)Field.LastUpdated] = true; _LastUpdated = value; } }
        private DateTime _LastUpdated = System.DateTime.Now;
        /// <summary>
        /// The title field for this node
        /// </summary>
        public string Title { get { return _Title; } set { Changed[(int)Field.Title] = true; _Title = value; } }
        private string _Title;
        /// <summary>
        /// A small string field that can be used for this node
        /// </summary>
        public string SubContent { get { return _SubContent; } set { Changed[(int)Field.SubContent] = true; _SubContent = value; } }
        private string _SubContent;
        /// <summary>
        /// The main content for the node
        /// </summary>
        public NodeContent Content { 
            get {
                // 24-03-21 - Updating this so that extended nodes only build if necessary and at point requested:
                if (CheckedExtended == false)
                {
                    _Content = _Content.ToString() + Access.ExtendedContentData(_ItemID);
                    CheckedExtended = true;
                }
                return _Content; 
            } 
            set { Changed[(int)Field.Content] = true; CheckedExtended = true; _Content = value; } }
        private NodeContent _Content;
        /// <summary>
        /// The first value field for the node
        /// </summary>
        public NodeValue Value1 { get { return _Value1; } set { Changed[(int)Field.Value1] = true; _Value1 = value; } }
        private NodeValue _Value1;
        /// <summary>
        /// The second value field for the node
        /// </summary>
        public NodeValue Value2 { get { return _Value2; } set { Changed[(int)Field.Value2] = true; _Value2 = value; } }
        private NodeValue _Value2;
        /// <summary>
        /// The third value field for the node
        /// </summary>
        public NodeValue Value3 { get { return _Value3; } set { Changed[(int)Field.Value3] = true; _Value3 = value; } }
        private NodeValue _Value3;
        /// <summary>
        /// The first date field for the node
        /// </summary>
        public DateTime Date1 { get { return _Date1; } set { Changed[(int)Field.Date1] = true; _Date1 = value; } }
        private DateTime _Date1;
        /// <summary>
        /// The second date field for the node
        /// </summary>
        public DateTime Date2 { get { return _Date2; } set { Changed[(int)Field.Date2] = true; _Date2 = value; } }
        private DateTime _Date2;
        /// <summary>
        /// The third date field for the node
        /// </summary>
        public DateTime Date3 { get { return _Date3; } set { Changed[(int)Field.Date3] = true; _Date3 = value; } }
        private DateTime _Date3;
        /// <summary>
        /// A boolean value for the node
        /// </summary>
        public bool BooleanItem { get { return _BooleanItem; } set { Changed[(int)Field.BooleanItem] = true; _BooleanItem = value; } }
        private bool _BooleanItem;
        /// <summary>
        /// Whether this node is active or not - default is "true". If false, this node can only be seen by the network if specifically asked for, or the request is set to also show non-active items
        /// </summary>
        public bool Active { get { return _Active; } set { Changed[(int)Field.Active] = true; _Active = value; } }
        private bool _Active = true;

        /// <summary>
        /// This is set to true when the link data has been filled in.
        /// </summary>
        public bool LINKDATA = false;
        /// <summary>
        /// IF THERE IS A LINK CONNECTION The Parent Item ID.
        /// </summary>
        public Guid Item1ID = Constants.BlankID;
        /// <summary>
        /// IF THERE IS A LINK CONNECTION The Child Item ID
        /// </summary>
        public Guid Item2ID = Constants.BlankID;
        /// <summary>
        /// IF THERE IS A LINK CONNECTION The Type for this link. The Standard is "Standard".
        /// </summary>
        public Guid LinkTypeID = Constants.BlankID;
        /// <summary>
        /// IF THERE IS A LINK CONNECTION Any extra information needed for the link
        /// </summary>
        public string ExtraInfo = null;
        /// <summary>
        /// IF THERE IS A LINK CONNECTION Can be used to confirm true or false
        /// </summary>
        public bool Confirmed = false;
        /// <summary>
        /// IF THERE IS A LINK CONNECTION Can store numerical data
        /// </summary>
        public NodeValue ValueInfo = null;
        /// <summary>
        /// IF THERE IS A LINK CONNECTION Used to join children together via a path
        /// </summary>
        public Guid PathID = Constants.BlankID;
        /// <summary>
        /// IF THERE IS A LINK CONNECTION When the link was created
        /// </summary>
        public DateTime LinkCreated = System.DateTime.Now;
        /// <summary>
        /// IF THERE IS A LINK CONNECTION When the link was last updated
        /// </summary>
        public DateTime LinkLastUpdated = System.DateTime.Now;

        #endregion

        // When a type is associated correctly to a node, these allow indexing based on the type name rather than the
        // field name
        public NodeContent Names = new NodeContent(",,,,,,,,,", ContentType.List);

        /// <summary>
        /// New for '21 - I am not going to load in extended content until content is requested. this should
        /// speed things up for loading node groups, and i am hoping this ref to the access layer won't increase
        /// the memory footprint
        /// </summary>
        public bool CheckedExtended = false;
        public Node_AccessLayer Access = null;

        /// <summary>
        /// Keep track of which fields have been updated
        /// </summary>
        public NodeValue Changes = 0;

        public object this[Field Field]    // Indexer declaration  
        {
            get { return GetValue(Field); }
            set
            {
                Changes[(int)Field] = true;
                switch (Field)
                {
                    case Field.Active:
                        Active = (bool)value;
                        break;
                    case Field.ApplicationID:
                        ApplicationID = (Guid)value;
                        break;
                    case Field.BooleanItem:
                        BooleanItem = (bool)value;
                        break;
                    case Field.Confirmed:
                        Confirmed = (bool)value;
                        break;
                    case Field.Content:
                        Content = (NodeContent)value;
                        break;
                    case Field.Created:
                        Created = (DateTime)value;
                        break;
                    case Field.Date1:
                        Date1 = (DateTime)value;
                        break;
                    case Field.Date2:
                        Date2 = (DateTime)value;
                        break;
                    case Field.Date3:
                        Date3 = (DateTime)value;
                        break;
                    case Field.ExtraInfo:
                        ExtraInfo = (string)value;
                        break;
                    case Field.Item1ID:
                        Item1ID = (Guid)value;
                        break;
                    case Field.Item2ID:
                        Item2ID = (Guid)value;
                        break;
                    case Field.ItemID:
                        ItemID = (Guid)value;
                        break;
                    case Field.LastUpdated:
                        LastUpdated = (DateTime)value;
                        break;
                    case Field.LinkCreated:
                        LinkCreated = (DateTime)value;
                        break;
                    case Field.LINKDATA:
                        LINKDATA = (bool)value;
                        break;
                    case Field.LinkLastUpdated:
                        LinkLastUpdated = (DateTime)value;
                        break;
                    case Field.LinkTypeID:
                        LinkTypeID = (Guid)value;
                        break;
                    case Field.PathID:
                        PathID = (Guid)value;
                        break;
                    case Field.SubContent:
                        SubContent = (string)value;
                        break;
                    case Field.Title:
                        Title = (string)value;
                        break;
                    case Field.TypeID:
                        TypeID = (Guid)value;
                        break;
                    case Field.UserID:
                        UserID = (Guid)value;
                        break;
                    case Field.Value1:
                        Value1 = (NodeValue)value;
                        break;
                    case Field.Value1X:
                        Value1.X = (long)value;
                        break;
                    case Field.Value1Y:
                        Value1.Y = (long)value;
                        break;
                    case Field.Value2:
                        Value2 = (NodeValue)value;
                        break;
                    case Field.Value2X:
                        Value2.X = (long)value;
                        break;
                    case Field.Value2Y:
                        Value2.X = (long)value;
                        break;
                    case Field.Value3:
                        Value3 = (NodeValue)value;
                        break;
                    case Field.Value3X:
                        Value3.X = (long)value;
                        break;
                    case Field.Value3Y:
                        Value3.X = (long)value;
                        break;
                    case Field.ValueInfo:
                        ValueInfo = (NodeValue)value;
                        break;
                }
            }
        }

        public object this[string Field]    // Indexer declaration  
        {
            get
            {
                if (Enum.TryParse(Field, out Field FieldName) == true)
                    return GetValue(FieldName);
                else
                {
                    // See if the field is the name of a field by Type:
                    for (int i = 0; i < 10; i++)
                    {
                        if (String.IsNullOrEmpty(Names[i]) == false && Field == Names[i])
                            return GetValue((Field)i);
                    }
                    return null;
                }
            }
            set
            {
                if (Enum.TryParse(Field, out Field FieldName) == true)
                    this[FieldName] = value;
                else
                {
                    // See if the field is the name of a field by Type:
                    for (int i = 0; i < 10; i++)
                    {
                        if (String.IsNullOrEmpty(Names[i]) == false && Field == Names[i])
                            this[(Field)i] = value;
                    }
                }
            }
        }

        public object this[Field Field, string Format]    // Indexer declaration  
        {
            get { return GetValue(Field, Format); }
        }



        public Node(Guid ItemID)
        {
            this.ItemID = ItemID;
            // Content.OnError += Content_OnError;
        }

        internal Node()
        {

        }

        private object GetValue(Field Field, string Format = null)
        {
            // Setup format so shows with spaces:
            if (Format != null)
                Format = Format.Replace("_", " ");
            switch (Field)
            {
                case Field.ToString:
                    return ToString();
                case Field.Active:
                    return Active;
                case Field.ApplicationID:
                    return ApplicationID;
                case Field.BooleanItem:
                    return BooleanItem;
                case Field.Content:
                    return Content;
                case Field.Created:
                    if (Format != null)
                        return Created.ToString(Format);
                    return Created;
                case Field.Date1:
                    if (Format != null)
                        return Date1.ToString(Format);
                    return Date1;
                case Field.Date2:
                    if (Format != null)
                        return Date2.ToString(Format);
                    return Date2;
                case Field.Date3:
                    if (Format != null)
                        return Date3.ToString(Format);
                    return Date3;
                case Field.ItemID:
                    return ItemID;
                case Field.LastUpdated:
                    if (Format != null)
                        return LastUpdated.ToString(Format);
                    return LastUpdated;
                case Field.SubContent:
                    return SubContent;
                case Field.Title:
                    return Title;
                case Field.TypeID:
                    return TypeID;
                case Field.UserID:
                    return UserID;
                case Field.Value1:
                    return (Decimal)Value1;
                case Field.Value1X:
                    return (Decimal)Value1.X;
                case Field.Value1Y:
                    return (Decimal)Value1.Y;
                case Field.Value2:
                    return (Decimal)Value2;
                case Field.Value2X:
                    return (Decimal)Value2.X;
                case Field.Value2Y:
                    return (Decimal)Value2.Y;
                case Field.Value3:
                    return (Decimal)Value3;
                case Field.Value3X:
                    return (Decimal)Value3.X;
                case Field.Value3Y:
                    return (Decimal)Value3.Y;
                // Now for the link items:
                case Field.Item1ID:
                    return Item1ID;
                case Field.Item2ID:
                    return Item2ID;
                case Field.LinkTypeID:
                    return LinkTypeID;
                case Field.ExtraInfo:
                    return ExtraInfo;
                case Field.Confirmed:
                    return Confirmed;
                case Field.ValueInfo:
                    return (Decimal)ValueInfo;
                case Field.PathID:
                    return PathID;
                case Field.LinkCreated:
                    if (Format != null)
                        return LinkCreated.ToString(Format);
                    return LinkCreated;
                case Field.LinkLastUpdated:
                    if (Format != null)
                        return LinkLastUpdated.ToString(Format);
                    return LinkLastUpdated;
                default:
                    return null;
            }
        }

        private string GetValueString(Field Field, string Format = null)
        {
            // Setup format so shows with spaces:
            if (Format != null)
                Format = Format.Replace("_", " ");
            switch (Field)
            {
                case Field.ToString:
                    return ToString();
                case Field.Active:
                    return Active.ToString();
                case Field.ApplicationID:
                    return ApplicationID.ToString();
                case Field.BooleanItem:
                    return BooleanItem.ToString();
                case Field.Content:
                    return Content;
                case Field.Created:
                    if (Format != null)
                        return Created.ToString(Format);
                    return Created.ToString();
                case Field.Date1:
                    if (Format != null)
                        return Date1.ToString(Format);
                    return Date1.ToString();
                case Field.Date2:
                    if (Format != null)
                        return Date2.ToString(Format);
                    return Date2.ToString();
                case Field.Date3:
                    if (Format != null)
                        return Date3.ToString(Format);
                    return Date3.ToString();
                case Field.ItemID:
                    return ItemID.ToString();
                case Field.LastUpdated:
                    if (Format != null)
                        return LastUpdated.ToString(Format);
                    return LastUpdated.ToString();
                case Field.SubContent:
                    return SubContent;
                case Field.Title:
                    return Title;
                case Field.TypeID:
                    return TypeID.ToString();
                case Field.UserID:
                    return UserID.ToString();
                case Field.Value1:
                    return ((Decimal)Value1).ToString();
                case Field.Value1X:
                    return ((Decimal)Value1.X).ToString();
                case Field.Value1Y:
                    return ((Decimal)Value1.Y).ToString();
                case Field.Value2:
                    return ((Decimal)Value2).ToString();
                case Field.Value2X:
                    return ((Decimal)Value2.X).ToString();
                case Field.Value2Y:
                    return ((Decimal)Value2.Y).ToString();
                case Field.Value3:
                    return ((Decimal)Value3).ToString();
                case Field.Value3X:
                    return ((Decimal)Value3.X).ToString();
                case Field.Value3Y:
                    return ((Decimal)Value3.Y).ToString();
                // Now for the link items:
                case Field.Item1ID:
                    return Item1ID.ToString();
                case Field.Item2ID:
                    return Item2ID.ToString();
                case Field.LinkTypeID:
                    return LinkTypeID.ToString();
                case Field.ExtraInfo:
                    return ExtraInfo;
                case Field.Confirmed:
                    return Confirmed.ToString();
                case Field.ValueInfo:
                    return ((Decimal)ValueInfo).ToString();
                case Field.PathID:
                    return PathID.ToString();
                case Field.LinkCreated:
                    if (Format != null)
                        return LinkCreated.ToString(Format);
                    return LinkCreated.ToString();
                case Field.LinkLastUpdated:
                    if (Format != null)
                        return LinkLastUpdated.ToString(Format);
                    return LinkLastUpdated.ToString();
                default:
                    return null;
            }
        }

        public static implicit operator Node(string item)
        {
            // Deserialise the Node:
            return DeserialiseNode(item);
        }

        public override string ToString()
        {
            return SerialiseNode();
        }

        public string ToString(Field field, string Format = null)
        {
            return GetValueString(field, Format);
        }

        private static Node DeserialiseNode(string data)
        {
            if (data == "")
                return null;
            Node X = new Node();
            X.ItemID = Guid.Parse(data.Substring(0, 36));
            string Decrypted = Sec_Crypt.AESDecrypt(data.Substring(36), "SerialisedNode");
            string Fixed = Decrypted.Substring(0, 36 + 36 + 36 + 14 + 14 + 1);
            X.UserID = Guid.Parse(Fixed.Substring(0, 36));
            X.TypeID = Guid.Parse(Fixed.Substring(36, 36));
            X.ApplicationID = Guid.Parse(Fixed.Substring(36 + 36, 36));
            X.Created = FromMySillyFormat(Fixed.Substring(36 + 36 + 36, 14));
            X.LastUpdated = FromMySillyFormat(Fixed.Substring(36 + 36 + 36 + 14, 14));
            if (Fixed.Substring(36 + 36 + 36 + 14 + 14) == "Y")
                X.Active = true;
            else
                X.Active = false;

            string[] Rest = Decrypted.Substring(36 + 36 + 36 + 14 + 14 + 1).Split(new string[] { "!~*~!" }, StringSplitOptions.None);

            if (Rest[0] != "")
            {
                X.Title = Rest[0];
                X.Names[0] = "X";
            }

            if (Rest[1] != "")
            {
                X.SubContent = Rest[1];
                X.Names[1] = "X";
            }

            if (Rest[2] != "")
            {
                X.Content = Rest[2];
                X.Names[2] = "X";
            }

            if (Rest[3] != "")
            {
                X.Value1 = Convert.ToDecimal(Rest[3]);
                X.Names[3] = "X";
            }

            if (Rest[4] != "")
            {
                X.Value2 = Convert.ToDecimal(Rest[4]);
                X.Names[4] = "X";
            }

            if (Rest[5] != "")
            {
                X.Value3 = Convert.ToDecimal(Rest[5]);
                X.Names[5] = "X";
            }

            if (Rest[6] != "")
            {
                X.Date1 = FromMySillyFormat(Rest[6]);
                X.Names[6] = "X";
            }

            if (Rest[7] != "")
            {
                X.Date2 = FromMySillyFormat(Rest[6]);
                X.Names[7] = "X";
            }

            if (Rest[8] != "")
            {
                X.Date3 = FromMySillyFormat(Rest[6]);
                X.Names[8] = "X";
            }

            if (Rest[9] != "")
            {
                X.BooleanItem = bool.Parse(Rest[9]);
                X.Names[9] = "X";
            }
            X.Changed = 0;
            return X;
        }

        private static DateTime FromMySillyFormat(string sillyFormattedDate)
        {
            return new DateTime(Convert.ToInt32(sillyFormattedDate.Substring(4, 4)),
                Convert.ToInt32(sillyFormattedDate.Substring(2, 2)),
                Convert.ToInt32(sillyFormattedDate.Substring(0, 2)),
                Convert.ToInt32(sillyFormattedDate.Substring(8, 2)), Convert.ToInt32(sillyFormattedDate.Substring(10, 2)), Convert.ToInt32(sillyFormattedDate.Substring(12, 2)));
        }

        private string SerialiseNode()
        {
            string Result = UserID.ToString() + TypeID.ToString() + ApplicationID.ToString() + Created.ToString("ddMMyyyyHHmmss") + LastUpdated.ToString("ddMMyyyyHHmmss");
            try
            {
                if (Active)
                    Result += "Y";
                else
                    Result += "N";

                if (string.IsNullOrEmpty(Names[0]) == false && Names[0] != "")
                    Result += Title + "!~*~!";
                else
                    Result += "!~*~!";

                if (string.IsNullOrEmpty(Names[1]) == false && Names[1] != "")
                    Result += SubContent + "!~*~!";
                else
                    Result += "!~*~!";

                if (string.IsNullOrEmpty(Names[2]) == false && Names[2] != "")
                    Result += Content.ToDataString() + "!~*~!";
                else
                    Result += "!~*~!";

                if (string.IsNullOrEmpty(Names[3]) == false && Names[3] != "" && Value1 != null)
                    Result += Value1.ToString() + "!~*~!";
                else
                    Result += "!~*~!";

                if (string.IsNullOrEmpty(Names[4]) == false && Names[4] != "" && Value2 != null)
                    Result += Value2.ToString() + "!~*~!";
                else
                    Result += "!~*~!";

                if (string.IsNullOrEmpty(Names[5]) == false && Names[5] != "" && Value3 != null)
                    Result += Value3.ToString() + "!~*~!";
                else
                    Result += "!~*~!";

                if (string.IsNullOrEmpty(Names[6]) == false && Names[6] != "" && Date1 != null)
                    Result += Date1.ToString("ddMMyyyyHHmmss") + "!~*~!";
                else
                    Result += "!~*~!";

                if (string.IsNullOrEmpty(Names[7]) == false && Names[7] != "" && Date2 != null)
                    Result += Date2.ToString("ddMMyyyyHHmmss") + "!~*~!";
                else
                    Result += "!~*~!";

                if (string.IsNullOrEmpty(Names[8]) == false && Names[8] != "" && Date3 != null)
                    Result += Date3.ToString("ddMMyyyyHHmmss") + "!~*~!";
                else
                    Result += "!~*~!";

                if (string.IsNullOrEmpty(Names[9]) == false && Names[9] != "")
                    Result += BooleanItem.ToString() + "!~*~!";
                else
                    Result += "!~*~!";

                return ItemID.ToString() + Sec_Crypt.AESEncrypt(Result.Substring(0, Result.Length - 5), "SerialisedNode");
            }
            catch(Exception err)
            {
                return Result+"<br/>"+err.Message+"<br/>"+err.Data+"<br/>"+err.StackTrace;
            }
        }
    }

    public class NodeContent
    {
        #region Variables
        public ErrorCode ErrorStatus = ErrorCode.No_Error;
        protected string Value = "";
        protected Bitmap ImageVersion = null;
        protected List<string> Items = new List<string>();
        protected int Length { get { return Value.Length; } }
        protected string AESKey = "";

        #region Content Type Code - Dealing with Changing the Type as you use it

        protected ContentType _Type = ContentType.Text;

        public ContentType Type
        {
            get { return _Type; }
            set
            {
                if (_Type != value)
                {
                    // What happens internally depends on what the new type is, and what the current type is:
                    switch (_Type)
                    {
                        case ContentType.Text:
                            ConvertFromText(value);
                            break;
                        case ContentType.HTML:
                            ConvertFromText(value);
                            break;
                        case ContentType.List:
                            ConvertFromList(value);
                            break;
                        case ContentType.AESEncrypted:
                            ConvertFromAES(value);
                            break;
                        case ContentType.BlowfishEncrypted:
                            ConvertFromBlowfish(value);
                            break;
                    }
                }
            }
        }

        #region ForFutureDevelopment
        /// <summary>
        /// This is the generic constructor for new items
        /// </summary>
        /// <param name="newValue">What Type to change this to</param>
        //private void ConvertFromText(ContentType newValue)
        //{
        //    _Type = newValue;
        //    switch (newValue)
        //    {
        //        case ContentType.AESEncrypted:
        //            break;
        //        case ContentType.BlowfishEncrypted:
        //            break;
        //        case ContentType.CompiledVector:
        //            break;
        //        case ContentType.CompiledVectorFile:
        //            break;
        //        case ContentType.CompiledVectorOverflowFile:
        //            break;
        //        case ContentType.File:
        //            break;
        //        case ContentType.HTML:
        //            break;
        //        case ContentType.HTMLFile:
        //            break;
        //        case ContentType.HTMLOverflowFile:
        //            break;
        //        case ContentType.HumanVector:
        //            break;
        //        case ContentType.HumanVectorFile:
        //            break;
        //        case ContentType.HumanVectorOverflowFile:
        //            break;
        //        case ContentType.List:
        //            break;
        //        case ContentType.ListFile:
        //            break;
        //        case ContentType.ListOverflowFile:
        //            break;
        //        case ContentType.NetScript:
        //            break;
        //        case ContentType.NetScriptFile:
        //            break;
        //        case ContentType.NetScriptOverflowFile:
        //            break;
        //        case ContentType.Text:
        //            break;
        //        case ContentType.TextFile:
        //            break;
        //        case ContentType.TextOverflowFile:
        //            break;
        //        default:
        //            break;
        //    }
        //}

        #endregion

        /// <summary>
        /// This runs if the value is currently text and it has been asked to change to another mode:
        /// </summary>
        /// <param name="newValue">What Type to Change this to</param>
        private void ConvertFromText(ContentType newValue)
        {
            _Type = newValue;
            switch (newValue)
            {
                case ContentType.BlowfishEncrypted:
                    // Is it already encrypted?
                    if (Value != null && Value != String.Empty && Value.IndexOf("$2a$") != 0)
                    {
                        // Encrypt the message:                        
                        Value = BCryptEmbed.HashPassword(Value, BCryptEmbed.GenerateSalt(12));
                    }
                    break;
                case ContentType.List:
                    // Convert to a list:
                    if (Value != null && Value != String.Empty)
                    {
                        string[] Split = Value.Split(new char[] { ',' });
                        Items = new List<string>();
                        foreach (string item in Split)
                        {
                            Items.Add(item);
                        }
                    }
                    break;
                case ContentType.AESEncrypted:
                    // Encrypt if possible:
                    if (Sec_Crypt.IsBase64(Value) == false)
                    {
                        if (AESKey != "")
                        {
                            Value = Sec_Crypt.AESEncrypt(Value, AESKey);
                        }
                        else
                        {
                            ErrorStatus = ErrorCode.EncryptionNotSet;
                        }
                    }
                    break;
            }
        }

        private void ConvertFromList(ContentType newValue)
        {
            _Type = newValue;

            // Convert the list to text first:
            Value = "";
            if (Items != null)
            {
                foreach (string item in Items)
                {
                    Value += item + ",";
                }
                Value = Value.TrimEnd(new char[] { ',' });
                Items.Clear();
                Items = null;
            }
            // Now do the switch:
            switch (newValue)
            {
                case ContentType.BlowfishEncrypted:
                    if (Value != null && Value != String.Empty && Value.IndexOf("$2a$") != 0)
                    {
                        // Encrypt the message:                        
                        Value = BCryptEmbed.HashPassword(Value, BCryptEmbed.GenerateSalt(12));
                    }
                    break;
                case ContentType.AESEncrypted:
                    // Encrypt if possible:
                    if (Sec_Crypt.IsBase64(Value) == false)
                    {
                        if (AESKey != "")
                        {
                            Value = Sec_Crypt.AESEncrypt(Value, AESKey);
                        }
                        else
                        {
                            ErrorStatus = ErrorCode.EncryptionNotSet;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// If moving away from AES, and We know the value, decrypt and change!
        /// </summary>
        /// <param name="newValue">What Type to change this to</param>
        private void ConvertFromAES(ContentType newValue)
        {
            if (AESKey != "")
            {
                Value = Sec_Crypt.AESDecrypt(Value, AESKey);
            }
            ConvertFromText(newValue);
            _Type = ContentType.Text;
        }

        /// <summary>
        /// If moving away from Blowfish, Do what we can!
        /// </summary>
        /// <param name="newValue">What Type to change this to</param>
        private void ConvertFromBlowfish(ContentType newValue)
        {
            _Type = newValue;

            switch (newValue)
            {
                case ContentType.AESEncrypted:
                    // Encrypt if possible:
                    if (Sec_Crypt.IsBase64(Value) == false)
                    {
                        if (AESKey != "")
                        {
                            Value = Sec_Crypt.AESEncrypt(Value, AESKey);
                        }
                        else
                        {
                            ErrorStatus = ErrorCode.EncryptionNotSet;
                        }
                    }
                    break;
                case ContentType.List:
                    // Convert to a list:
                    if (Value != null && Value != String.Empty)
                    {
                        string[] Split = Value.Split(new char[] { ',' });
                        Items = new List<string>();
                        foreach (string item in Split)
                        {
                            Items.Add(item);
                        }
                    }
                    break;
            }
        }

        #endregion

        #endregion

        #region Constructors

        public string this[int index]    // Indexer declaration  
        {
            get
            {
                ErrorStatus = ErrorCode.No_Error;
                if (Type == ContentType.List)
                {
                    if (index < 0 || index >= Items.Count)
                    {
                        ErrorStatus = ErrorCode.OutOfBounds;
                        return null;
                    }
                    return Items[index];
                }
                else
                {
                    if (index != 0)
                    {
                        ErrorStatus = ErrorCode.OutOfBounds;
                        return null;
                    }
                    return ToDataString();
                }
            }
            set
            {
                if (Type == ContentType.List)
                {
                    if (index >= 0 && index < Items.Count)
                        Items[index] = value;
                }
            }
        }

        public NodeContent()
        {
            // No constructor
        }

        public NodeContent(ContentType Type)
        {
            this.Type = Type;
        }

        public NodeContent(string Value, ContentType Type)
        {
            this.Value = Value;
            this.Type = Type;
        }

        public NodeContent(string Value, ContentType Type, string AESKey)
        {
            this.AESKey = AESKey;
            this.Value = Value;
            this.Type = Type;
        }

        /// <summary>
        /// Constructor with a string value
        /// </summary>
        /// <param name="Value">The value to store in NodeContent</param>
        public NodeContent(string Value)
        {
            // This piece of code has to try and determine whether this is a string, a list or something else!
            // Firstly, we are going to remove any ',' from the string and replace it with a GUID - it will help, promise!

            string Remover = System.Guid.NewGuid().ToString();
            Value = Value.Replace("','", Remover);

            if (Value.IndexOf("$2a$") == 0)
            {
                // Has all the hallmarks of being a blowfish encrypted string!
                this.Value = Value;
                _Type = ContentType.BlowfishEncrypted;
            }
            else if (Value.IndexOf(",") == -1)
            {
                // This is not considered a list. we may be wrong, but that will hopefully be fixed once it is changed again!
                _Type = ContentType.Text;
                // Put back in the commas:
                this.Value = Value.Replace(Remover, ",");
                Items = null;
            }
            else
            {
                // This is a list:
                string[] Split = Value.Split(new char[] { ',' });
                Items = new List<string>();
                foreach (string item in Split)
                {
                    // Add in the values including all the commas that there should be!
                    Items.Add(item.Replace(Remover, ","));
                }
                this.Value = "";
                _Type = ContentType.List;
            }
        }

        public NodeContent(string[] Value)
        {
            this.Value = null;
            Items = new List<string>();
            for (int i = 0; i < Value.Length; i++)
            {
                Items.Add(Value[i].Replace(Constants.NodeContentCommaInList, ","));
            }
            Type = ContentType.List;
        }

        #endregion

        #region Public Functions

        public void Reset()
        {
            this.Value = "";
            AESKey = "";
            this.Items.Clear();
            _Type = ContentType.Text;
        }

        public void SetValue(string Value)
        {
            ErrorStatus = ErrorCode.No_Error;
            switch (_Type)
            {
                case ContentType.BlowfishEncrypted:
                    if (Value == String.Empty || Value.IndexOf("$2a$") == 0)
                    {
                        this.Value = Value;
                    }
                    else
                        this.Value = BCryptEmbed.HashPassword(Value, BCryptEmbed.GenerateSalt(12));
                    break;
                case ContentType.AESEncrypted:
                    if (Sec_Crypt.IsBase64(Value) == true)
                    {
                        this.Value = Value;
                    }
                    else
                    {
                        if (Sec_Crypt.IsBase64(Value))
                            this.Value = Value;
                        else
                        {
                            if (AESKey != "")
                            {
                                this.Value = Sec_Crypt.AESEncrypt(Value, AESKey);
                            }
                            else
                            {
                                this.Value = Value;
                                ErrorStatus = ErrorCode.EncryptionNotSet;
                            }
                        }
                    }
                    break;
                case ContentType.List:
                    Items.Add(Value);
                    break;
                default:
                    this.Value = Value;
                    break;
            }

        }

        public void SetEncryption(string EncryptionKey)
        {
            if (Type == ContentType.AESEncrypted && ErrorStatus == ErrorCode.EncryptionNotSet)
                this.Value = Sec_Crypt.AESEncrypt(Value, EncryptionKey);
            else if (Type == ContentType.AESEncrypted && Sec_Crypt.IsBase64(this.Value) == false)
                this.Value = Sec_Crypt.AESEncrypt(Value, EncryptionKey);
            else if (Type == ContentType.AESEncrypted && AESKey != "")
            {
                this.Value = Sec_Crypt.AESDecrypt(Value, AESKey);
                this.Value = Sec_Crypt.AESEncrypt(Value, EncryptionKey);
            }
            this.AESKey = EncryptionKey;
            ErrorStatus = ErrorCode.No_Error;
        }

        #endregion

        #region Implicit Operators for Assignment and Mathematics

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(NodeContent Data, NodeContent Data2)
        {
            if (Data.Type == ContentType.BlowfishEncrypted && Data2.Type == ContentType.BlowfishEncrypted)
                return false;
            else if (Data.Type == ContentType.BlowfishEncrypted)
            {
                return BCryptEmbed.CheckPassword(Data2.ToString(), Data.Value);
            }
            else if (Data2.Type == ContentType.BlowfishEncrypted)
            {
                return BCryptEmbed.CheckPassword(Data.ToString(), Data2.Value);
            }
            else
                return Data.ToString() == Data2.ToString();
        }

        public static bool operator !=(NodeContent Data, NodeContent Data2)
        {
            if (Data.Type == ContentType.BlowfishEncrypted && Data2.Type == ContentType.BlowfishEncrypted)
                return true;
            else if (Data.Type == ContentType.BlowfishEncrypted)
            {
                return !BCryptEmbed.CheckPassword(Data2.ToString(), Data.Value);
            }
            else if (Data2.Type == ContentType.BlowfishEncrypted)
            {
                return !BCryptEmbed.CheckPassword(Data.ToString(), Data2.Value);
            }
            else
                return Data.ToString() != Data2.ToString();
        }

        public static bool operator ==(NodeContent Data, string Data2)
        {
            if (Data.Type == ContentType.BlowfishEncrypted)
            {
                return BCryptEmbed.CheckPassword(Data2, Data.Value);
            }
            else
                return Data.ToString() == Data2;
        }

        public static bool operator !=(NodeContent Data, string Data2)
        {
            if (Data.Type == ContentType.BlowfishEncrypted)
            {
                return !BCryptEmbed.CheckPassword(Data2, Data.Value);
            }
            else
                return Data.ToString() != Data2;
        }

        public static implicit operator string(NodeContent Data)
        {
            return Data.ToString();
        }

        public static implicit operator NodeContent(string Data)
        {
            return new NodeContent(Data);
        }

        public static implicit operator string[] (NodeContent Data)
        {
            return Data.ToStringArray();
        }

        public static implicit operator NodeContent(string[] Data)
        {
            return new NodeContent(Data);
        }
        
        public override string ToString()
        {
            if (Type == ContentType.List)
            {
                string X = "";
                for (int i = 0; i < Items.Count; i++)
                {
                    X += Items[i] + ",";
                }
                return X.TrimEnd(new char[] { ',' });
            }
            else if (Type == ContentType.HTML)
            {
                // Take out all of the HTML tags!
                string temp = Value;
                temp = temp.Replace("\r", "");
                temp = temp.Replace("\n", "");
                temp = temp.Replace("<br/>", "\r\n");
                temp = temp.Replace("</p>", "\r\n");
                temp = Text_Modifier.Compact_Tag(temp, "<", ">", "");
                temp = temp.Replace("&gt;", ">");
                temp = temp.Replace("&lt;", "<");
                temp = temp.Replace("&amp;", "&");
                temp = temp.Replace("&nbsp;", " ");
                return temp;
            }
            else if (Type == ContentType.AESEncrypted)
            {
                if (AESKey != "")
                {
                    return Sec_Crypt.AESDecrypt(Value, AESKey);
                }
                else
                {
                    ErrorStatus = ErrorCode.EncryptionNotSet;
                    return Value;
                }
            }
            else
            {
                return Value;
            }
        }

        public string[] ToStringArray()
        {
            if (Type == ContentType.List)
            {
                string[] X = new string[Items.Count];
                for (int i = 0; i < Items.Count; i++)
                {
                    X[i] = Items[i];
                }
                return X;
            }
            else
            {
                return new string[1] { ToString() };
            }
        }

        /// <summary>
        /// Use this to get the NodeContent in the format that is necessary for the database
        /// </summary>
        /// <returns>commas are in quotes</returns>
        public string ToDataString(bool PutCommasInQuotes = false)
        {
            if (Type == ContentType.List)
            {
                string X = "";
                for (int i = 0; i < Items.Count; i++)
                {
                    X += Items[i].Replace(",", "','") + ",";
                }
                return X.TrimEnd(new char[] { ',' });
            }
            else
            {
                if (PutCommasInQuotes == false)
                    return Value;
                else
                    return Value.Replace(",", "','");
            }
        }

        public string ToHTMLString()
        {
            string temp = "";
            if (Type == ContentType.List)
            {
                string X = "<ul>";
                for (int i = 0; i < Items.Count; i++)
                {
                    X += "<li>" + Items[i] + "</li>";
                }
                X += "</ul>";
                return X;
            }
            else if (Type == ContentType.Text || Type == ContentType.AESEncrypted)
            {
                if (Type == ContentType.AESEncrypted && AESKey != "")
                    temp = "<p>" + Sec_Crypt.AESDecrypt(Value, AESKey) + "</p>";
                else
                    temp = "<p>" + Value + "</p>";
                temp = temp.Replace("\r", "");
                temp = temp.Replace("\n", "</p><p>");
                temp = temp.Replace("&", "&amp;");
                temp = temp.Replace(">", "&gt;");
                temp = temp.Replace("<", "&lt;");
                temp = temp.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                temp = temp.Replace("  ", " &nbsp;");
                return temp;
            }
            else
            {
                return Value;
            }
        }

        #endregion


    }

    [Serializable]
    public class NodeValue
    {
        #region Variables
        public ErrorCode ErrorStatus = ErrorCode.No_Error;
        /// <summary>
        /// The actual value as stored in RAM
        /// </summary>
        protected decimal Value = 0;

        /// <summary>
        /// The X coordinate in an XY Pair
        /// </summary>
        public Int64 X
        {
            get
            {
                return Convert.ToInt64(Value);
            }
            set
            {
                Value = Value - Convert.ToInt64(Value) + value;
            }
        }

        /// <summary>
        /// The Y coordinate in an XY Pair
        /// </summary>
        public Int64 Y
        {
            get
            {

                if (Value.ToString().IndexOf(".") == -1)
                    return 0;
                string First = Value.ToString().Substring(Value.ToString().IndexOf(".") + 1);
                try
                {
                    // Reverse the string:
                    string Second = "";
                    int counter = First.Length - 1;
                    while (counter >= 0)
                    {
                        Second += First[counter];
                        counter--;
                    }
                    // if the first number is a 1, its positive, else it is negative:
                    if (Second[0] == '1')
                        return Convert.ToInt64(Second.Substring(1));
                    else
                        return 0 - Convert.ToInt64(Second.Substring(1));
                }
                catch
                {
                    if (Value.ToString().IndexOf(".") == -1)
                        return 0;
                    else
                        return Convert.ToInt64(First);
                }
            }
            set
            {
                string First = "";
                if (Value == 0)
                    First = "0";
                else if (value >= 0)
                    First = "1" + value.ToString();
                else
                {
                    First = "2" + (-value).ToString();
                }
                // Reverse the string:
                string Second = "";
                int counter = First.Length - 1;
                while (counter >= 0)
                {
                    Second += First[counter];
                    counter--;
                }
                Second = Convert.ToInt64(Value).ToString() + "." + Second;
                Value = Convert.ToDecimal(Second);
            }
        }

        #endregion

        #region Constructors

        public bool this[int index, bool ClearAllOthers = false]    // Indexer declaration  
        {
            get { return this.CheckSwitch(index); }
            set
            {
                if (ClearAllOthers == false)
                    this.SetSwitch(index, value);
                else
                    this.SetClearSwitch(index, value);
            }
        }

        /// <summary>
        /// Constructor with a decimal value
        /// </summary>
        /// <param name="value">The value to store in NodeValue</param>
        public NodeValue(decimal value)
        {
            Value = value;
        }

        /// <summary>
        /// Constructor with a double value
        /// </summary>
        /// <param name="value">The value to store in NodeValue</param>
        public NodeValue(double value)
        {
            Value = Convert.ToDecimal(value);
        }

        /// <summary>
        /// Constructor with an integer value
        /// </summary>
        /// <param name="value">The value to store in NodeValue</param>
        public NodeValue(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Constructor with an int 64 value
        /// </summary>
        /// <param name="value">The value to store in NodeValue</param>
        public NodeValue(Int64 value)
        {
            Value = value;
        }

        /// <summary>
        /// Constructor with an Point value
        /// </summary>
        /// <param name="value">The value to store in NodeValue</param>
        public NodeValue(Point value)
        {
            X = value.X;
            Y = value.Y;
        }

        #endregion

        #region Implicit Operators for Assignment and Mathematics

        public static implicit operator string(NodeValue Data)
        {
            if (Data == null)
                return "";
            else
                return Data.Value.ToString();
        }

        public static implicit operator NodeValue(decimal Data)
        {
            return new NodeValue(Data);
        }

        public static implicit operator NodeValue(double Data)
        {
            return new NodeValue(Data);
        }

        public static implicit operator NodeValue(int Data)
        {
            return new NodeValue(Data);
        }

        public static implicit operator NodeValue(Int64 Data)
        {
            return new NodeValue(Data);
        }

        public static implicit operator NodeValue(Point Data)
        {
            return new NodeValue(Data);
        }

        public static implicit operator decimal(NodeValue Data)
        {
            if (Data == null)
                return -1111111111;
            return Data.Value;
        }

        public static implicit operator int(NodeValue Data)
        {
            if (Data == null)
                return -1111111111;
            return Convert.ToInt32(Data.Value);
        }

        public static implicit operator Int64(NodeValue Data)
        {
            if (Data == null)
                return -1111111111;
            return Convert.ToInt64(Data.Value);
        }

        public static implicit operator double(NodeValue Data)
        {
            if (Data == null)
                return -1111111111;
            return Convert.ToDouble(Data.Value);
        }

        public static implicit operator Point(NodeValue Data)
        {
            if (Data == null)
                return new Point(-1111111111, -1111111111);
            return new Point(Convert.ToInt32(Data.X), Convert.ToInt32(Data.Y));
        }

        public static NodeValue operator +(NodeValue value1, NodeValue value2)
        {
            return new NodeValue(value1.Value + value2.Value);
        }

        public static NodeValue operator -(NodeValue value1, NodeValue value2)
        {
            return new NodeValue(value1.Value - value2.Value);
        }

        public static NodeValue operator /(NodeValue value1, NodeValue value2)
        {
            return new NodeValue(value1.Value / value2.Value);
        }

        public static NodeValue operator *(NodeValue value1, NodeValue value2)
        {
            return new NodeValue(value1.Value * value2.Value);
        }

        // Point operators

        public static NodeValue operator +(NodeValue value1, Point value2)
        {
            NodeValue temp = 0;
            temp.X = value1.X + value2.X;
            temp.Y = value1.Y + value2.Y;
            return temp;
        }

        public static NodeValue operator -(NodeValue value1, Point value2)
        {
            NodeValue temp = 0;
            temp.X = value1.X - value2.X;
            temp.Y = value1.Y - value2.Y;
            return temp;
        }

        public static NodeValue operator /(NodeValue value1, Point value2)
        {
            NodeValue temp = 0;
            temp.X = value1.X / value2.X;
            temp.Y = value1.Y / value2.Y;
            return temp;
        }

        public static NodeValue operator *(NodeValue value1, Point value2)
        {
            NodeValue temp = 0;
            temp.X = value1.X * value2.X;
            temp.Y = value1.Y * value2.Y;
            return temp;
        }

        public static NodeValue operator +(Point value1, NodeValue value2)
        {
            NodeValue temp = 0;
            temp.X = value1.X + value2.X;
            temp.Y = value1.Y + value2.Y;
            return temp;
        }

        public static NodeValue operator -(Point value1, NodeValue value2)
        {
            NodeValue temp = 0;
            temp.X = value1.X - value2.X;
            temp.Y = value1.Y - value2.Y;
            return temp;
        }

        public static NodeValue operator /(Point value1, NodeValue value2)
        {
            NodeValue temp = 0;
            temp.X = value1.X / value2.X;
            temp.Y = value1.Y / value2.Y;
            return temp;
        }

        public static NodeValue operator *(Point value1, NodeValue value2)
        {
            NodeValue temp = 0;
            temp.X = value1.X * value2.X;
            temp.Y = value1.Y * value2.Y;
            return temp;
        }

        // Integer operators

        public static NodeValue operator +(NodeValue value1, Int64 value2)
        {
            return new NodeValue(value1.Value + value2);
        }

        public static NodeValue operator -(NodeValue value1, Int64 value2)
        {
            return new NodeValue(value1.Value - value2);
        }

        public static NodeValue operator /(NodeValue value1, Int64 value2)
        {
            return new NodeValue(value1.Value / value2);
        }

        public static NodeValue operator *(NodeValue value1, Int64 value2)
        {
            return new NodeValue(value1.Value * value2);
        }

        public static NodeValue operator +(Int64 value1, NodeValue value2)
        {
            return new NodeValue(value1 + value2.Value);
        }

        public static NodeValue operator -(Int64 value1, NodeValue value2)
        {
            return new NodeValue(value1 - value2.Value);
        }

        public static NodeValue operator /(Int64 value1, NodeValue value2)
        {
            return new NodeValue(value1 / value2.Value);
        }

        public static NodeValue operator *(Int64 value1, NodeValue value2)
        {
            return new NodeValue(value1 * value2.Value);
        }

        // Decimal

        public static NodeValue operator +(NodeValue value1, decimal value2)
        {
            return new NodeValue(value1.Value + value2);
        }

        public static NodeValue operator -(NodeValue value1, decimal value2)
        {
            return new NodeValue(value1.Value - value2);
        }

        public static NodeValue operator /(NodeValue value1, decimal value2)
        {
            return new NodeValue(value1.Value / value2);
        }

        public static NodeValue operator *(NodeValue value1, decimal value2)
        {
            return new NodeValue(value1.Value * value2);
        }

        public static NodeValue operator +(decimal value1, NodeValue value2)
        {
            return new NodeValue(value1 + value2.Value);
        }

        public static NodeValue operator -(decimal value1, NodeValue value2)
        {
            return new NodeValue(value1 - value2.Value);
        }

        public static NodeValue operator /(decimal value1, NodeValue value2)
        {
            return new NodeValue(value1 / value2.Value);
        }

        public static NodeValue operator *(decimal value1, NodeValue value2)
        {
            return new NodeValue(value1 * value2.Value);
        }

        // Double
        public static NodeValue operator +(NodeValue value1, double value2)
        {
            return new NodeValue(Convert.ToDouble(value1.Value) + value2);
        }

        public static NodeValue operator -(NodeValue value1, double value2)
        {
            return new NodeValue(Convert.ToDouble(value1.Value) - value2);
        }

        public static NodeValue operator /(NodeValue value1, double value2)
        {
            return new NodeValue(Convert.ToDouble(value1.Value) / value2);
        }

        public static NodeValue operator *(NodeValue value1, double value2)
        {
            return new NodeValue(Convert.ToDouble(value1.Value) * value2);
        }

        public static NodeValue operator +(double value1, NodeValue value2)
        {
            return new NodeValue(value1 + Convert.ToDouble(value2.Value));
        }

        public static NodeValue operator -(double value1, NodeValue value2)
        {
            return new NodeValue(value1 - Convert.ToDouble(value2.Value));
        }

        public static NodeValue operator /(double value1, NodeValue value2)
        {
            return new NodeValue(value1 / Convert.ToDouble(value2.Value));
        }

        public static NodeValue operator *(double value1, NodeValue value2)
        {
            return new NodeValue(value1 * Convert.ToDouble(value2.Value));
        }

        #endregion

        #region Switches

        /// <summary>
        /// This tells us if the selected bit is true or false
        /// </summary>
        /// <param name="index">0 lowest, 31 highest</param>
        /// <returns>true if bit is 1, false if 0</returns>
        public bool CheckSwitch(int index)
        {
            ErrorStatus = ErrorCode.No_Error;
            if (index >= 0 && index < 32)
            {
                return (Convert.ToInt64(Value) & Convert.ToInt64(Math.Pow(2, index))) != 0;
            }
            else
            {
                ErrorStatus = ErrorCode.OutOfBounds;
                return false;
            }
        }

        /// <summary>
        /// Internal - does what CheckSwitch does but returns a 1 or 0
        /// </summary>
        /// <param name="index">0 lowest, 31 highest</param>
        /// <returns>a string "1" or "0" for the value</returns>
        protected string CheckSwitchValue(int index)
        {
            ErrorStatus = ErrorCode.No_Error;
            if (index >= 0 && index < 32)
            {
                if ((Convert.ToInt64(Value) & Convert.ToInt64(Math.Pow(2, index))) != 0)
                    return "1";
                else
                    return "0";
            }
            else
            {
                ErrorStatus = ErrorCode.OutOfBounds;
                return null;
            }
        }

        /// <summary>
        /// Set a single bit in the value, clearing any existing values first:
        /// </summary>
        /// <param name="index">0 lowest, 31 highest</param>
        /// <param name="value">true for 1, false for 0</param>
        public void SetClearSwitch(int index, bool value)
        {
            Value = 0;
            SetSwitch(index, value);
        }

        /// <summary>
        /// Set a single bit in the value
        /// </summary>
        /// <param name="index">0 lowest, 31 highest</param>
        /// <param name="value">true for 1, false for 0</param>
        public void SetSwitch(int index, bool value)
        {
            ErrorStatus = ErrorCode.No_Error;
            if (index >= 0 && index < 32)
            {
                decimal mask = Convert.ToDecimal(Math.Pow(2, index));
                if ((value == false) && CheckSwitch(index) == true)
                    Value -= mask;
                else if ((value == true) && CheckSwitch(index) == false)
                    Value += mask;
            }
            else
                ErrorStatus = ErrorCode.OutOfBounds;
        }

        #endregion

        /// <summary>
        /// Show the NodeValue as a Binary Representation
        /// </summary>
        /// <returns>String in binary 31st bit - 0th bit</returns>
        public string ToBinaryString()
        {
            int counter = 31;
            string X = "";
            while (counter >= 0)
            {
                X += CheckSwitchValue(counter);
                counter--;
            }
            return X;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    /// <summary>
    /// Used when updating a node to say which fields have changed:
    /// </summary>
    [Serializable]
    public class NodeChanges
    {
        #region Public Fields
        /// <summary>
        /// The title field for this node
        /// </summary>
        public bool Title = false;
        /// <summary>
        /// A small string field that can be used for this node
        /// </summary>
        public bool SubContent = false;
        /// <summary>
        /// The main content for the node
        /// </summary>
        public bool Content = false;
        /// <summary>
        /// The first value field for the node
        /// </summary>
        public bool Value1 = false;
        /// <summary>
        /// The second value field for the node
        /// </summary>
        public bool Value2 = false;
        /// <summary>
        /// The third value field for the node
        /// </summary>
        public bool Value3 = false;
        /// <summary>
        /// The first date field for the node
        /// </summary>
        public bool Date1 = false;
        /// <summary>
        /// The second date field for the node
        /// </summary>
        public bool Date2 = false;
        /// <summary>
        /// The third date field for the node
        /// </summary>
        public bool Date3 = false;
        /// <summary>
        /// A boolean value for the node
        /// </summary>
        public bool BooleanValue = false;
        /// <summary>
        /// Whether this node is active or not - default is "true". If false, this node can only be seen by the network if specifically asked for, or the request is set to also show non-active items
        /// </summary>
        public bool Active = false;

        public bool TypeID = false;
        #endregion
    }
}
