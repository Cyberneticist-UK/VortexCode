using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Vortex
{
    /// <summary>
    /// This is version 3.0 of NetScript. It actually uses separate loosely bound dll files for all the commands for ease!
    /// </summary>
    public class NetScript
    {
        public NetScript_Command CommandProcessor;
        public Vortex_Memory Memory = new Vortex_Memory();
        public Node_AccessLayer Access = null;
        /// <summary>
        /// Shows the details of the person that is currently logged into the scripting system
        /// </summary>
        public Node WhoAmI { get { if (Access == null) return null; return Access.CurrentlyLoggedIn; } }
        string CommandFolder = "";

        /// <summary>
        /// Initialise the NetScript System:
        /// </summary>
        /// <param name="CommandFolder">The Full Path Where to find the command DLLs</param>
        /// <param name="ContentFolder">The relative path where to find the content from the current Application</param>
        /// <param name="NodeCache">A Node Access layer to use</param>
        /// <param name="UserAgent">The User Agent string</param>
        /// <param name="ApplicationFolder">Optional Current Application Folder.</param>
        public NetScript(string CommandFolder, string ContentFolder, string SystemPasscode, Guid ApplicationID, Node_AccessLayer NodeCache, string UserAgent, string ApplicationFolder = "", string SMTP = "", string SMTPUsername = "", string SMTPPassword = "")
        {
            if (Directory.Exists(CommandFolder) == true)
                this.CommandFolder = CommandFolder;
            else
            {
                // Could be in a sub directory trying to access the folder - so go back a few steps!
                string[] Folders = CommandFolder.Split(new char[] { '\\' });
                List<string> FolderList = new List<string>();
                foreach (string item in Folders)
                {
                    if(item.Trim() != "")
                    {
                        FolderList.Add(item);
                    }
                }
                // c:\Windows\Sub\sub\sub\VortexCommand
                while (FolderList.Count > 0)
                {
                    CommandFolder = "";
                    for (int i = 0; i < FolderList.Count; i++)
                    {
                        if (i != FolderList.Count - 2)
                            CommandFolder += FolderList[i] + "\\";
                    }
                    if (Directory.Exists(CommandFolder) == true)
                    {
                        this.CommandFolder = CommandFolder;
                        FolderList.Clear();
                    }
                    else
                    {
                        FolderList.RemoveAt(FolderList.Count - 2);
                    }
                }
                    
            }
            Memory.ApplicationID = ApplicationID;
            Memory.ApplicationFolder = ApplicationFolder;
            Memory.ContentFolder = ContentFolder;
            CommandProcessor = new NetScript_Command(CommandFolder, SystemPasscode, SMTP, SMTPUsername, SMTPPassword);
            Access = NodeCache;
            Memory.UserAgent = UserAgent;
            Memory.DebugLog.Add("Start of Debug Log.");
            Memory.Error.Add("Start of Error Log.");
            Memory.Error.ErrorFlag = false;
        }

        /// <summary>
        /// Simply returns the current Error log from memory
        /// </summary>
        /// <returns></returns>
        public string GetErrorLog()
        {
            Memory.AddDebugModeMessage("Requested Error Log");
            return Memory.Error.ToString();
        }

        /// <summary>
        /// Simply returns the current Debug log from memory
        /// </summary>
        /// <returns></returns>
        public string GetDebugLog()
        {
            Memory.AddDebugModeMessage("Requested Debug Log");
            return Memory.DebugLog.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetGraphicsLog()
        {
            Memory.AddDebugModeMessage("Requested Graphic Log");
            string X = "<h2>Graphics Stored in memory (Name: Width x height)</h2>";
            foreach(string item in Memory.Graphics.Keys)
            {
                X += item + ": "+Memory.Graphics[item].ToImage().Width+"px by "+Memory.Graphics[item].ToImage().Height+"px<br/>";
            }
            return X;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetVariableLog()
        {
            Memory.AddDebugModeMessage("Requested Variable Log");
            string X = "<h2>Variables stored in memory (Name: Value)</h2>";
            foreach (string item in Memory.Variables.Keys)
            {
                X += "<div><h3>"+item + "</h3>"+Memory.Variables[item].Data.ToString().Replace("<", "&lt;").Replace(">","&gt;")+"</div>";
            }
            return X;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetArrayLog()
        {
            Memory.AddDebugModeMessage("Requested Array Log");
            string X = "<h2>Arrays Stored in Memory</h2>";
            foreach (string item in Memory.VarArrays.Keys)
            {
                X += item + ": "+ Memory.VarArrays[item].TokenSubData.Count.ToString() +" items<br/>";
            }
            return X;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetFunctionLog()
        {
            Memory.AddDebugModeMessage("Requested Function Log");
            string X = "<h2>Functions stored in Memory</h2>";
            foreach (string item in CommandProcessor.Functions.Keys)
            {
                X += item + "<br/>";
            }
            return X;
        }

        /// <summary>
        /// Disconnects the Command Processor and restarts it. I think. Not sure it's been used much
        /// </summary>
        public void RefreshCommands()
        {
            Memory.AddDebugModeMessage("Refresh Commands Requested at NetScript Level");
            CommandProcessor = new NetScript_Command(CommandFolder, CommandProcessor.EmailServer.client);
        }

        /// <summary>
        /// Clears the current output document
        /// </summary>
        public void ClearOutput()
        {
            Memory.AddDebugModeMessage("Clear Output Requested at NetScript Level");
            Memory.ClearMemory();
        }

        /// <summary>
        /// Clears all the variables currently held in memory
        /// </summary>
        public void ClearVariables()
        {
            Memory.AddDebugModeMessage("Clear Variables Requested at NetScript Level");
            Memory.Variables.Clear();
        }

        /// <summary>
        /// Clears a specific variable from memory by name
        /// </summary>
        /// <param name="Name">The variable to remove</param>
        public void ClearVariable(string Name)
        {
            Memory.AddDebugModeMessage("Clear Variable named " +Name+" Requested at NetScript Level");
            if (Memory.Variables.ContainsKey(Name))
                Memory.Variables.Remove(Name);
        }

        public string GetVariable(string Name)
        {
            Memory.AddDebugModeMessage("Get Variable named " + Name + " Requested at NetScript Level");
            if (Memory.Variables.ContainsKey(Name))
                return Memory.Variables[Name].Data;
            return null;
        }

        public void SetVariable(string Name, string Data, VariableFrom WhereFrom)
        {
            Memory.AddDebugModeMessage("Set Variable named " + Name + " with "+Data+" Requested at NetScript Level");
            CommandProcessor.Tools.AddVariable(Name, Data, Memory, new Token(), true, WhereFrom);
        }

        public void SetArray(string Name, Token Data)
        {
            Memory.AddDebugModeMessage("Set Array named " + Name + " Requested at NetScript Level");
            CommandProcessor.Tools.AddVarArray(Name, Data, Memory, new Token(), true);
        }

        /// <summary>
        /// Clears a specific Array from memory by name
        /// </summary>
        /// <param name="Name">The variable to remove</param>
        public void ClearArray(string Name)
        {
            Memory.AddDebugModeMessage("Clear Variable named " + Name + " Requested at NetScript Level");
            if (Memory.VarArrays.ContainsKey(Name))
                Memory.VarArrays.Remove(Name);
        }

        public bool LogMeInCookie(string Cookie)
        {
            Memory.AddDebugModeMessage("Login with Cookie Requested at NetScript Level");
            bool result = Access.LogMeIn(Cookie, Memory.UserAgent);
            if (result == true)
                Memory.Session = Cookie;
            return result;
        }

        public bool LogMeIn(string Username, string Password, out Node Session)
        {
            Memory.AddDebugModeMessage("Login with Username and Password Requested at NetScript Level");
            bool result = Access.LogMeIn(Username, Password, Memory.UserAgent, out Session);
            if (result == true)
                Memory.Session = Session;
            return result;
        }

        public void Logout(Guid UserID, Guid SessionID)
        {
            Memory.AddDebugModeMessage("Logout Requested at NetScript Level");
            Memory.Session = null;
            Access.Logout(UserID, SessionID);
        }

        /// <summary>
        /// Directly send through the script and run it
        /// </summary>
        /// <param name="Script">The Script file to run</param>
        /// <param name="Nodes">Optional List of nodes to apply to the Script</param>
        /// <returns></returns>
        public string RunScriptFile(string ScriptFile)
        {
            Memory.AddDebugModeMessage("RunScriptFile Requested at NetScript Level - Filename " +ScriptFile);
            Token CommandRunner = new Token(TokenInfo.Command, "RunScriptFile");
            CommandRunner.TokenSubData = new List<Token>();
            CommandRunner.TokenSubData.Add(new Token(TokenInfo.Text, ScriptFile));

            // Run this script:
            CommandRunner = CommandProcessor.RunCommand(CommandRunner, Memory, Access);

            // Final stage:
            return FinalOutputProcess();
        }

        public string FinalOutputProcess()
        {
            Memory.AddDebugModeMessage("Starting Final Output Process");
            string Output = Memory.OutputDocument;
            Memory.StopScript = false;
            // Before we run the final output document back out, we need to add in any javascript if necessary:-
            if (Memory.GuidForJavascriptPlaceholder != "")
            {
                Memory.AddDebugModeMessage("GUID Found for JavaScript Placeholder - Adding in Javascript");
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<script type=\"text/javascript\">");
                foreach (string item in Memory.JSFunctionTitles.Values)
                {
                    sb.AppendLine(item);
                }
                sb.AppendLine("</script>");
                Output = Output.Replace(Memory.GuidForJavascriptPlaceholder, sb.ToString());
                Memory.JSFunctionTitles.Clear();
            }
            if (Memory.GuidForCSSPlaceholder != "")
            {
                
                // I still need to do a lot more work on this - checking that the content is proper css
                // and making sure that overridden items are not included eg if color has been used twice, the first one
                // should be removed
                Memory.AddDebugModeMessage("GUID Found for CSS Placeholder - Adding in CSS");
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<style type=\"text/css\">");
                // Redesigned 10/11/2021 to include media queries:
                if (Memory.CSSItems2.TokenSubData != null)
                {
                    foreach (Token item in Memory.CSSItems2.TokenSubData)
                    {
                        if (item.data != "Standard")
                        {
                            sb.Append(item.data);
                            sb.AppendLine("{");
                        }
                        for (int i = 0; i < item.TokenTestSubData.Count; i++)
                        {
                            sb.Append(item.TokenTestSubData[i].data);
                            sb.AppendLine("{");
                            sb.AppendLine(item.TokenSubData[i].data);
                            sb.AppendLine("}");
                            sb.AppendLine(" ");
                        }
                        if (item.data != "Standard")
                        {
                            sb.AppendLine("}");
                        }
                    }
                }
                //foreach (string item in Memory.CSSItems.Keys)
                //{
                //    sb.AppendLine(item);
                //    sb.AppendLine("{");
                //    sb.AppendLine(Memory.CSSItems[item]);
                //    sb.AppendLine("}");
                //    sb.AppendLine(" ");
                //}

                sb.AppendLine("</style>");
                Output = Output.Replace(Memory.GuidForCSSPlaceholder, sb.ToString());
                // Memory.CSSItems.Clear();
                Memory.CSSItems2 = new Token();
            }

            // We need to just check to see if we are replacing all of this with Cache:
            if (Memory.CacheFile != "")
            {
                // Ah! we are doing Cache Stuff!
                if (Memory.UseCache == true)
                {
                    Memory.AddDebugModeMessage("Cache File Requested");
                    // Replace the output with the cache file instead!
                    if (File.Exists(CommandProcessor.Folder + "\\Cache\\" + Memory.CacheFile) == true)
                    {
                        Memory.AddDebugModeMessage("Output replaced with Cache File");
                        Output = File.ReadAllText(CommandProcessor.Folder + "\\Cache\\" + Memory.CacheFile);
                    }
                    else
                    {
                        Memory.Error.Add("Error - Cache File requested but not Found - "+ CommandProcessor.Folder + "\\Cache\\" + Memory.CacheFile);
                    }
                }
                else
                {
                    // We are writing to the Cache instead!
                    Memory.AddDebugModeMessage("Output Written to Cache File " + CommandProcessor.Folder + "\\Cache\\" + Memory.CacheFile);
                    if (Directory.Exists(CommandProcessor.Folder + "\\Cache\\") == false)
                        Directory.CreateDirectory(CommandProcessor.Folder + "\\Cache\\");
                    File.WriteAllText(CommandProcessor.Folder + "\\Cache\\" + Memory.CacheFile, Output);
                }
                Memory.CacheFile = "";
            }
            // This is to keep the archive out of memory while it is not being used - at the end of an output, just put it out of memory for now, it should be
            // Brought back into memory as needed!
            Memory.Archive = null;
            return Output;
        }

        /// <summary>
        /// Directly send through the script and run it
        /// </summary>
        /// <param name="Script">The Script data to run</param>
        /// <returns></returns>
        public string RunScript(string Script)
        {
            Memory.AddDebugModeMessage("RunScript Requested at NetScript Level");
            Token CommandRunner = new Token(TokenInfo.Command, "RunScript");
            CommandRunner.TokenSubData = new List<Token>();
            CommandRunner.TokenSubData.Add(new Token(TokenInfo.Text, Script));

            // Run this script:
            CommandRunner = CommandProcessor.RunCommand(CommandRunner, Memory, Access);

            // Final stage:
            return FinalOutputProcess();
        }
        
    }

}
