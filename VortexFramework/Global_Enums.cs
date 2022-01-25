using System;

namespace Vortex
{
    //public enum Command
    //{
    //    nullItem,
    //    Node, GetNow,
    //    CreateWhere,
    //    CreateOrder,
    //    CreateGroup,
    //    OrderGroup,
    //    UniqueGroup,
    //    AddTo, AddSetting,
    //    ChangeSetting,
    //    ListSettings,
    //    Clear,
    //    AddNode,
    //    GetNode,
    //    GetNodesByType,
    //    GetParentsByType,
    //    GetParents,
    //    GetParentsTwoChildren,
    //    GetChildrenByType,
    //    GetChildren,
    //    GetChildrenTwoParents,
    //    GetNodesBetweenParentChild,
    //    ForEach,
    //    Write,
    //    WriteTemplate,
    //    NodeLink,
    //    GetNodeLink,
    //    WriteFile,
    //    WriteFileBase64,
    //    SetFilePath,
    //    SetFilePathServer,
    //    Render,
    //    If, ElseIf, Else,
    //    SubScript, EndSubScript, Run,
    //    MatrixTable,
    //    MathsAddTo, MathsSubtractFrom, MathsDivide, MathsMultiply,
    //    Var
    //}

    public enum SpecialItems
    {
        Node,
        Setting,
        NodeLink,
        MatrixNodeX,
        MatrixNodeY,
        MatrixNodeLink,
        LoopItem
    }

    public enum StringFormat
    {
        nullItem,
        CSListBL, CSListBLR, CSListLB, CSListLBR, CSListOL, CSListOLR,
        FieldName,
        FileContent, FileContentBase64,
        TxtToHTML, P, B, T, I
    }

    public enum LogCode
    {
        WrongFormat, CommandNotTerminated, VariableError
    }

    /// <summary>
    /// All the different types a Token can be! Go to VortexTools/CheckData(string temp) if you make changes here!
    /// </summary>
    public enum TokenInfo
    {
        HTML, Command, Text, Numeric, Quote, StringFormat, Operator, SpecialItems, Field, Integer, Decimal, NodeData, Variable, InBrackets, Date, Boolean, GUID
    }
    public delegate void SendToLog(LogCode Code, string Description);


    /// <summary>
    /// List of all commands supported by Netelligence
    /// </summary>
    public enum CommandList
    {
        ActivateMesh, DeactivateMesh, UpdateMesh, InternalError, YouAreSlave, YouAreMaster,
        Text, EncryptedText, Chat, EncryptedChat, NetelligenceLogin, GetGroupList, GetPersonList, GetMyMachineList,
        SendWindow, RefreshSavedProjects,
        InterWindowComms
        //Close,
        //, RequestEnvironment, SendingEnvironment,  // , 
        //OK, ActivateComputer, DeactivateComputer,
        //SetGroup, ActivateCursor, DeactivateCursor, MoveCursor, Shutdown, ActivateVirtualWindow, DeactivateVirtualWindow, MoveVirtualWindow,
        //DropVirtualWindow, StillAlive, PacketOverflowData, EndPacketOverflow, CloseGroup, SendDebugLog, HubBack
        //, MoveComputer, ChangeColour, ChangeSize, ChangeName, 
    }



    /// <summary>
    /// List of the supported Connection Protocols:
    /// </summary>
    public enum ConnectionProtocol { NotSet, Self, TCP, TCPv6, UDP, UDPv6, WebClient, WebServer, WebService }


    /// <summary>
    /// The different formats a message can take
    /// </summary>
    public enum MessageFormat { ASCII, Unicode, Node, Bitmap, Binary }

    /// <summary>
    /// List of all commands supported by Netelligence
    /// </summary>
    public enum ErrorCode
    {
        No_Error, Message_Is_Unicode, Message_Is_Node, Packet_Missing, GUID_Wrong_In_List, EncryptionNotSet, OutOfBounds
    }



    public static class Constants
    {
        public static Guid BlankID = new Guid("00000000-0000-0000-0000-000000000000");
        public static Guid ExtendedContentID = new Guid("FFFFFFFF-0000-FFFF-FFFF-FFFFFFFFFFFF");
        public static Guid TypeID = new Guid("00000000-0000-0000-0000-000000000000");
        public static Guid LinkTypeID = new Guid("00000000-0000-0000-0000-000000000001");
        public static Guid ApplicationTypeID = new Guid("00000000-0000-0000-0000-000000000002");
        public static DateTime NullDateTime = new DateTime(1, 1, 1);
        public static string NodeContentCommaInList = "~:~";
    }

}
