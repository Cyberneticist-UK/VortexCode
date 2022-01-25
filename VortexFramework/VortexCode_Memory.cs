using VortexLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Vortex
{

    public class Vortex_Memory
    {
        /// <summary>
        /// The HTML/binary Document data produced by NetScript
        /// </summary>

        /// <update>
        /// This version 2021 - a new dictionary that will allow for more flexibility in the commands (using it with Flow)
        /// and moving the public strings to this dictionary as well, hopefully making it more efficient too!
        /// </update>
        public Dictionary<string, string> StructuredData = new Dictionary<string, string>();
        public Dictionary<string, Dictionary<string, string>> StructuredDictionaries = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Update 08/2021 - the addition of an "Archive" for deduplicating files and storage:
        /// </summary>
        public Token Archive = new Token();
        public string CurrentArchive = "";
        

        // The Response type can be assigned to so that NetScript can output files as well as
        // data
        public string ResponseType = "text/html";
        public Dictionary<string, string> HeaderList = new Dictionary<string, string>();
        // If it needs to output a binary stream, this is set to true:
        public bool UseOutputStream = false;
        // This is the string if it is text:
        public string OutputDocument { get { return _output.ToString(); } }
        StringBuilder _output = new StringBuilder();
        public Variable ReturnerValue = null;
        // This is the stream if it is binary:
        public byte[] OutputStream { get { return _outputStream; } }
        byte[] _outputStream = new byte[0];
        public Random rnd = new Random();

        // Now for some of the other data!
        public List<NetScript_LoopData> LoopData = new List<NetScript_LoopData>();
        public List<string> FilesAccessed = new List<string>();
        public bool ClearWasSet = false;
        public bool ClearDebugWasSet = false;
        public string ApplicationFolder = "";
        public string ContentFolder = "";
        public string UserAgent = "";
        public Guid ApplicationID = Guid.Empty;
        public Node Session = null;
        public Node CurrentNode = null;

        /// <summary>
        /// Also new for November 2018 - when your command needs some javascript to work, we add it in here ready for the output document!
        /// Note - didnt end up using this in commands, but am bringing it back July 2020 for components! 
        /// </summary>
        public Dictionary<string, string> ComponentData = new Dictionary<string, string>();
        public Dictionary<string, string> JSFunctionTitles = new Dictionary<string, string>();
        // New for 2021 - Turning CSSItems into a Token to allow for media queries:
        public Token CSSItems2 = new Token();
        // Keeping the old one in currently while making the transition:
        // public Dictionary<string, string> CSSItems = new Dictionary<string, string>();
        public string GuidForJavascriptPlaceholder = ""; // This is where the JSFunctions will be added into the output document, if this has been set to something other than the default!
        public string GuidForCSSPlaceholder = ""; // This is where the CSS will be added into the output document, if this has been set to something other than the default!
        /// <summary>
        /// Also new for Nov 2018 - can halt the script at any point. Used in cache and when a program just has had enough!
        /// </summary>
        public bool StopScript = false;

        /// <summary>
        /// Two more items for November for Caching - if CacheFile is blank, do nothing. If it is set, UseCache
        /// is set to true to use the file in place of the output, or false means save the output to that file as a cache.
        /// </summary>
        public string CacheFile = "";
        public bool UseCache = false;

        public Dictionary<string, NetBitmap> Graphics = new Dictionary<string, NetBitmap>();

        /// <summary>
        /// The Different Node Groups held in memory
        /// </summary>
        public Dictionary<string, List<Node>> NodeGroups = new Dictionary<string, List<Node>>();

        /// <summary>
        /// The Variables used within the current script
        /// </summary>
        public Dictionary<string, Variable> Variables = new Dictionary<string, Variable>();

        /// <summary>
        /// The Arrays used within the current script, saved as tokens. New for February 2019!
        /// </summary>
        public Dictionary<string, Token> VarArrays = new Dictionary<string, Token>();

        /// <summary>
        /// The Variables, VarArrays and NodeGroups that make up a particular scope for a script. New for May 2019!
        /// </summary>
        public Dictionary<string, List<string>> Scope = new Dictionary<string, List<string>>();
        /// <summary>
        /// The Structures are objects that can be used by different dll's for storage of varying different object types:
        /// </summary>
        public Hashtable Structures = new Hashtable();

        /// <summary>
        /// The entire script being processed currently:
        /// </summary>
        public List<Token> ScriptTree = new List<Token>();
        public int ScriptTreeIndex = -1;
        public string ScriptTreeName = "";

        #region Error and Debug

        /// <summary>
        /// The Error Log. Currently just a list of items!
        /// </summary>
        public ErrorLog Error = new ErrorLog();
        /// <summary>
        /// The Debug Log - useful to debug code when in debug mode!
        /// </summary>
        public ErrorLog DebugLog = new ErrorLog();
        /// <summary>
        /// NEW for November 2018! A debug mode switch, so you can check to see if we are in debug mode or not
        /// and modify the behaviour of your command based on this knowledge!
        /// </summary>
        public bool DebugMode = false;

        /// <summary>
        /// If we are currently in debug mode, this adds the message to the debug log. Otherwise it is ignored.
        /// To always add a message to debug, use DebugLog.Add(Message) directly.
        /// </summary>
        /// <param name="Message">The message to add if we are in Debug Mode</param>
        public void AddDebugModeMessage(string Message)
        {
            if (DebugMode)
            {
                DebugLog.Add(Message);
            }
        }

        /// <summary>
        /// Appends the Error Log to the output
        /// </summary>
        public void AppendErrorLog()
        {
            AddToOutput(Error.ToString());
        }

        /// <summary>
        /// Appends the Debug Log to the output
        /// </summary>
        public void AppendDebugLog()
        {
            AddToOutput(DebugLog.ToString());
        }

        #endregion

        public void AddToOutput(string item)
        {
            _output.Append(item);
        }

        public void SetOutputStream(byte[] data)
        {
            _outputStream = data;
        }

        public void AddHeader(string Key, string Value)
        {
            HeaderList.Add(Key, Value);
        }

        public void ClearHeader()
        {
            HeaderList.Clear();
        }

        public void AddLoopData(NetScript_LoopData Data)
        {
            LoopData.Add(Data);
        }

        public void RemoveLoopData()
        {
            LoopData.RemoveAt(LoopData.Count - 1);
        }
        public Token GetLoopItem(int index)
        {
            if (LoopData.Count == 0)
                return new Token(TokenInfo.Text, "");
            return LoopData[LoopData.Count - 1].ItemDataArray[index];
        }

        public Token GetLoopData { get { if (LoopData.Count == 0) return new Token(TokenInfo.Text, ""); return LoopData[LoopData.Count - 1].ItemData; } }

        public int GetLoopIndex { get { if (LoopData.Count == 0) return 0; return LoopData[LoopData.Count - 1].ItemIndex; } }

        public void ClearMemory()
        {
            _output.Clear();
            ClearHeader();
            JSFunctionTitles.Clear();
            ComponentData.Clear();
            UseOutputStream = false;
            ResponseType = "text/html";
            _outputStream = new byte[0];
            Archive = new Token();
            ClearWasSet = true;
        }

        /// <summary>
        /// Copies all the current variable data from a source memory object into this one
        /// </summary>
        /// <param name="Source">Where the variable data is currently</param>
        public void CopyFrom(Vortex_Memory Source)
        {
            CurrentArchive = Source.CurrentArchive;
            Archive = Source.Archive;
            ResponseType = Source.ResponseType;
            HeaderList = Source.HeaderList;
            UseOutputStream = Source.UseOutputStream;
            _outputStream = Source._outputStream;

            LoopData = Source.LoopData;
            Variables = Source.Variables;
            NodeGroups = Source.NodeGroups;
            ApplicationFolder = Source.ApplicationFolder;
            ApplicationID = Source.ApplicationID;
            ContentFolder = Source.ContentFolder;
            DebugMode = Source.DebugMode;
            JSFunctionTitles = Source.JSFunctionTitles;
            // CSSItems = Source.CSSItems;
            CSSItems2 = Source.CSSItems2;
            ReturnerValue = Source.ReturnerValue;
            ComponentData = Source.ComponentData;
            GuidForJavascriptPlaceholder = Source.GuidForJavascriptPlaceholder;
            GuidForCSSPlaceholder = Source.GuidForCSSPlaceholder;
            StopScript = Source.StopScript;
            CacheFile = Source.CacheFile;
            UseCache = Source.UseCache;
            UserAgent = Source.UserAgent;
            Session = Source.Session;
            VarArrays = Source.VarArrays;
            Scope = Source.Scope;
            CurrentNode = Source.CurrentNode;
            Graphics = Source.Graphics;
            Structures = Source.Structures;
            foreach (string File in Source.FilesAccessed)
            {
                if (FilesAccessed.Contains(File) == false)
                    FilesAccessed.Add(File);
            }
            Error.Append(Source.Error);
            if (Source.ClearDebugWasSet == true)
            {
                DebugLog.Clear();
                ClearDebugWasSet = true;
                Source.ClearDebugWasSet = false;
            }
            DebugLog.Append(Source.DebugLog);
        }
    }

    public class Vortex_Memory2
    {
        /// <summary>
        /// The HTML/binary Document data produced by NetScript
        /// </summary>

        /// <update>
        /// This version 2021 - a new dictionary that will allow for more flexibility in the commands (using it with Flow)
        /// and moving the public strings to this dictionary as well, hopefully making it more efficient too!
        /// </update>
        public Dictionary<string, string> StructuredData = new Dictionary<string, string>();
        public Dictionary<string, Dictionary<string, string>> StructuredDictionaries = new Dictionary<string, Dictionary<string, string>>();

        // The Response type can be assigned to so that NetScript can output files as well as
        // data
        public string ResponseType
        {
            get {if (StructuredData.ContainsKey("ResponseType") == false){StructuredData.Add("ResponseType", "text/html");}return StructuredData["ResponseType"];}
            set {if (StructuredData.ContainsKey("ResponseType") == false){StructuredData.Add("ResponseType", value);}else{StructuredData["ResponseType"] = value;}}
        }

        public Dictionary<string, string> HeaderList
        {
            get { if (StructuredDictionaries.ContainsKey("HeaderList") == false) { StructuredDictionaries.Add("HeaderList", new Dictionary<string, string>()); } return StructuredDictionaries["HeaderList"]; }
            set { if (StructuredDictionaries.ContainsKey("HeaderList") == false) { StructuredDictionaries.Add("HeaderList", value); } else { StructuredDictionaries["HeaderList"] = value; } }
        }
        // If it needs to output a binary stream, this is set to true:
        public bool UseOutputStream = false;
        // This is the string if it is text:
        public string OutputDocument { get { return _output.ToString(); } }
        StringBuilder _output = new StringBuilder();
        public Variable ReturnerValue = null;
        // This is the stream if it is binary:
        public byte[] OutputStream { get { return _outputStream; } }
        byte[] _outputStream = new byte[0];
		public Random rnd = new Random();

        // Now for some of the other data!
        public List<NetScript_LoopData> LoopData = new List<NetScript_LoopData>();
        public List<string> FilesAccessed = new List<string>();
        public bool ClearWasSet = false;
        public bool ClearDebugWasSet = false;
        public string ApplicationFolder
        {
            get { if (StructuredData.ContainsKey("ApplicationFolder") == false) { StructuredData.Add("ApplicationFolder", ""); } return StructuredData["ApplicationFolder"]; }
            set { if (StructuredData.ContainsKey("ApplicationFolder") == false) { StructuredData.Add("ApplicationFolder", value); } else { StructuredData["ApplicationFolder"] = value; } }
        }
        
        public string ContentFolder
        {
            get { if (StructuredData.ContainsKey("ContentFolder") == false) { StructuredData.Add("ContentFolder", ""); } return StructuredData["ContentFolder"]; }
            set { if (StructuredData.ContainsKey("ContentFolder") == false) { StructuredData.Add("ContentFolder", value); } else { StructuredData["ContentFolder"] = value; } }
        }
        
        public string UserAgent
        {
            get { if (StructuredData.ContainsKey("UserAgent") == false) { StructuredData.Add("UserAgent", ""); } return StructuredData["UserAgent"]; }
            set { if (StructuredData.ContainsKey("UserAgent") == false) { StructuredData.Add("UserAgent", value); } else { StructuredData["UserAgent"] = value; } }
        }

        public Guid ApplicationID = Guid.Empty;
        public Node Session = null;
        public Node CurrentNode = null;

        /// <summary>
        /// Also new for November 2018 - when your command needs some javascript to work, we add it in here ready for the output document!
        /// Note - didnt end up using this in commands, but am bringing it back July 2020 for components! 
        /// </summary>
        public Dictionary<string, string> ComponentData
        {
            get { if (StructuredDictionaries.ContainsKey("ComponentData") == false) { StructuredDictionaries.Add("ComponentData", new Dictionary<string, string>()); } return StructuredDictionaries["ComponentData"]; }
            set { if (StructuredDictionaries.ContainsKey("ComponentData") == false) { StructuredDictionaries.Add("ComponentData", value); } else { StructuredDictionaries["ComponentData"] = value; } }
        }

        public Dictionary<string, string> JSFunctionTitles
        {
            get { if (StructuredDictionaries.ContainsKey("JSFunctionTitles") == false) { StructuredDictionaries.Add("JSFunctionTitles", new Dictionary<string, string>()); } return StructuredDictionaries["JSFunctionTitles"]; }
            set { if (StructuredDictionaries.ContainsKey("JSFunctionTitles") == false) { StructuredDictionaries.Add("JSFunctionTitles", value); } else { StructuredDictionaries["JSFunctionTitles"] = value; } }
        }
        
        public Dictionary<string, string> CSSItems
        {
            get { if (StructuredDictionaries.ContainsKey("CSSItems") == false) { StructuredDictionaries.Add("CSSItems", new Dictionary<string, string>()); } return StructuredDictionaries["CSSItems"]; }
            set { if (StructuredDictionaries.ContainsKey("CSSItems") == false) { StructuredDictionaries.Add("CSSItems", value); } else { StructuredDictionaries["CSSItems"] = value; } }
        }
        // This is where the JSFunctions will be added into the output document, if this has been set to something other than the default!
        public string GuidForJavascriptPlaceholder
        {
            get { if (StructuredData.ContainsKey("GuidForJavascriptPlaceholder") == false) { StructuredData.Add("GuidForJavascriptPlaceholder", ""); } return StructuredData["GuidForJavascriptPlaceholder"]; }
            set { if (StructuredData.ContainsKey("GuidForJavascriptPlaceholder") == false) { StructuredData.Add("GuidForJavascriptPlaceholder", value); } else { StructuredData["GuidForJavascriptPlaceholder"] = value; } }
        }
        
        // This is where the CSS will be added into the output document, if this has been set to something other than the default!
        public string GuidForCSSPlaceholder
        {
            get { if (StructuredData.ContainsKey("GuidForCSSPlaceholder") == false) { StructuredData.Add("GuidForCSSPlaceholder", ""); } return StructuredData["GuidForCSSPlaceholder"]; }
            set { if (StructuredData.ContainsKey("GuidForCSSPlaceholder") == false) { StructuredData.Add("GuidForCSSPlaceholder", value); } else { StructuredData["GuidForCSSPlaceholder"] = value; } }
        }
        /// <summary>
         /// Also new for Nov 2018 - can halt the script at any point. Used in cache and when a program just has had enough!
         /// </summary>
        public bool StopScript = false;

        /// <summary>
        /// Two more items for November for Caching - if CacheFile is blank, do nothing. If it is set, UseCache
        /// is set to true to use the file in place of the output, or false means save the output to that file as a cache.
        /// </summary>
        public string CacheFile
        {
            get { if (StructuredData.ContainsKey("CacheFile") == false) { StructuredData.Add("CacheFile", ""); } return StructuredData["CacheFile"]; }
            set { if (StructuredData.ContainsKey("CacheFile") == false) { StructuredData.Add("CacheFile", value); } else { StructuredData["CacheFile"] = value; } }
        }
        public bool UseCache = false;
        public string Domain
        {
            get { if (StructuredData.ContainsKey("Domain") == false) { StructuredData.Add("Domain", ""); } return StructuredData["Domain"]; }
            set { if (StructuredData.ContainsKey("Domain") == false) { StructuredData.Add("Domain", value); } else { StructuredData["Domain"] = value; } }
        }

        public Dictionary<string, NetBitmap> Graphics = new Dictionary<string, NetBitmap>();

        /// <summary>
        /// The Different Node Groups held in memory
        /// </summary>
        public Dictionary<string, List<Node>> NodeGroups = new Dictionary<string, List<Node>>();

        /// <summary>
        /// The Variables used within the current script
        /// </summary>
        public Dictionary<string, Variable> Variables = new Dictionary<string, Variable>();

        /// <summary>
        /// The Arrays used within the current script, saved as tokens. New for February 2019!
        /// </summary>
        public Dictionary<string, Token> VarArrays = new Dictionary<string, Token>();

        /// <summary>
        /// The Variables, VarArrays and NodeGroups that make up a particular scope for a script. New for May 2019!
        /// </summary>
        public Dictionary<string, List<string>> Scope = new Dictionary<string, List<string>>();
        /// <summary>
        /// The Structures are objects that can be used by different dll's for storage of varying different object types:
        /// </summary>
        public Hashtable Structures = new Hashtable();

        /// <summary>
        /// The entire script being processed currently:
        /// </summary>
        public List<Token> ScriptTree = new List<Token>();
        public int ScriptTreeIndex = -1;
        public string ScriptTreeName = "";

        #region Error and Debug

        /// <summary>
        /// The Error Log. Currently just a list of items!
        /// </summary>
        public ErrorLog Error = new ErrorLog();
        /// <summary>
        /// The Debug Log - useful to debug code when in debug mode!
        /// </summary>
        public ErrorLog DebugLog = new ErrorLog();
        /// <summary>
        /// NEW for November 2018! A debug mode switch, so you can check to see if we are in debug mode or not
        /// and modify the behaviour of your command based on this knowledge!
        /// </summary>
        public bool DebugMode = false;

        /// <summary>
        /// If we are currently in debug mode, this adds the message to the debug log. Otherwise it is ignored.
        /// To always add a message to debug, use DebugLog.Add(Message) directly.
        /// </summary>
        /// <param name="Message">The message to add if we are in Debug Mode</param>
        public void AddDebugModeMessage(string Message)
        {
            if (DebugMode)
            {
                DebugLog.Add(Message);
            }
        }

        /// <summary>
        /// Appends the Error Log to the output
        /// </summary>
        public void AppendErrorLog()
        {
            AddToOutput(Error.ToString());
        }

        /// <summary>
        /// Appends the Debug Log to the output
        /// </summary>
        public void AppendDebugLog()
        {
            AddToOutput(DebugLog.ToString());
        }

        #endregion

        public void AddToOutput(string item)
        {
            _output.Append(item);
        }

        public void SetOutputStream(byte[] data)
        {
            _outputStream = data;
        }

        public void AddHeader(string Key, string Value)
        {
            HeaderList.Add(Key, Value);
        }

        public void ClearHeader()
        {
            HeaderList.Clear();
        }

        public void AddLoopData(NetScript_LoopData Data)
        {
            LoopData.Add(Data);
        }

        public void RemoveLoopData()
        {
            LoopData.RemoveAt(LoopData.Count-1);
        }
        public Token GetLoopItem(int index)
        {
            if (LoopData.Count == 0)
                return new Token(TokenInfo.Text, "");
            return LoopData[LoopData.Count - 1].ItemDataArray[index];
        }

        public Token GetLoopData { get { if (LoopData.Count == 0) return new Token(TokenInfo.Text,""); return LoopData[LoopData.Count - 1].ItemData; } }

        public int GetLoopIndex { get { if (LoopData.Count == 0) return 0; return LoopData[LoopData.Count - 1].ItemIndex; } }

        public void ClearMemory()
        {
            _output.Clear();
            ClearHeader();
            StructuredData.Clear();
            JSFunctionTitles.Clear();
            ComponentData.Clear();
            UseOutputStream = false;
            _outputStream = new byte[0];
            ClearWasSet = true;
        }

        /// <summary>
        /// Copies all the current variable data from a source memory object into this one
        /// </summary>
        /// <param name="Source">Where the variable data is currently</param>
        public void CopyFrom(Vortex_Memory2 Source)
        {
            StructuredDictionaries = Source.StructuredDictionaries;
            StructuredData = Source.StructuredData;
            UseOutputStream = Source.UseOutputStream;
            _outputStream = Source._outputStream;
            LoopData = Source.LoopData;
            Variables = Source.Variables;
            NodeGroups = Source.NodeGroups;
            ApplicationID = Source.ApplicationID;
            DebugMode = Source.DebugMode;
            ReturnerValue = Source.ReturnerValue;
            StopScript = Source.StopScript;
            UseCache = Source.UseCache;
            Session = Source.Session;
            VarArrays = Source.VarArrays;
            Scope = Source.Scope;
            CurrentNode = Source.CurrentNode;
            Graphics = Source.Graphics;
            Structures = Source.Structures;
            foreach (string File in Source.FilesAccessed)
            {
                if (FilesAccessed.Contains(File) == false)
                    FilesAccessed.Add(File);
            }
            Error.Append(Source.Error);
            if (Source.ClearDebugWasSet == true)
            {
                DebugLog.Clear();
                ClearDebugWasSet = true;
                Source.ClearDebugWasSet = false;
            }
            DebugLog.Append(Source.DebugLog);
        }
    }
}
