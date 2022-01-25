using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Vortex
{
    /// <summary>
    /// Version 2.0
    /// New changes (currently all old code still commented out while testing!)
    /// All old code now removed!!
    /// Moved from a double dictionary system (a dictionary within a dictionary) to a single one - should be faster
    /// Utilised an indexing system to look up commands in replacement of the above
    /// Moved where the same code is being used into a separate method (loop/no loop), refresh and constructor
    /// All commands are now case sensitive, which they should have been anyway!
    /// Sped up the checking to see if something is a command or not by just looking at the index
    /// </summary>
    public class NetScript_Command
    {
        public bool IsolationMode = false;
        // This is the actual container for the commands
        public Dictionary<string, string> Functions = new Dictionary<string, string>();
        public NetScriptCommand_Container CommandList;
        // This is the folder the DLL's are all stored in
        public string Folder;
        // This is the direct access to the different tools necessary
        public VortexTools Tools;
        public Comms_Email EmailServer;

        // This is the indexer into the command list
        public Dictionary<string, int> CommandIndex = new Dictionary<string, int>();
        // Where we store the legacy commands:
        Legacy leg;
        // The system passcode - this must match in the script for the system command to run
        string SystemPasscode = "";
        /// <summary>
        /// Initialise the Command System
        /// </summary>
        /// <param name="Folder"></param>
        public NetScript_Command(string Folder, string Passcode, string SMTP = "", string SMTPUsername = "", string SMTPPassword="")
        {
            leg = new Legacy(Folder);
            this.SystemPasscode = Passcode;
            this.Folder = Folder;
            if(SMTP != "")
            {
                EmailServer = new Comms_Email(SMTP, SMTPUsername, SMTPPassword);
            }
            // New - Utilise the refresh code, as it was duplicating code before!
            Refresh();
            Tools = new VortexTools(this);
        }

        /// <summary>
        /// Initialise the Command System
        /// </summary>
        /// <param name="Folder"></param>
        public NetScript_Command(string Folder, System.Net.Mail.SmtpClient SMTP)
        {
            this.Folder = Folder;
            EmailServer = new Comms_Email(SMTP);
            // New - Utilise the refresh code, as it was duplicating code before!
            Refresh();
            Tools = new VortexTools(this);
        }



        /// <summary>
        /// This resets the command container in case of changes to the underlying commands
        /// </summary>
        public void Refresh()
        {
            // Set up the commands list:
            Functions = new Dictionary<string, string>();
            CommandList = new NetScriptCommand_Container(Folder);
            // Reset the Index
            CommandIndex.Clear();
            int i = 0;
            foreach (var Cmd in CommandList.Commands)
            {
                CommandIndex.Add(Cmd.Value.Command, i);
                i++;
            }
        }

        /// <summary>
        /// Simplified now - just see if the command is in the index
        /// </summary>
        /// <param name="CommandName">The command to check</param>
        /// <returns>true if what is being requested is a command, false if not!</returns>
        public bool CheckCommand(string CommandName)
        {
            if (CommandIndex.ContainsKey(CommandName))
                return true;
            else
            {
                return leg.LegacyCommand(CommandName);
            }
        }
        
        ///// <summary>
        ///// Not currently used
        ///// This looks to see if any Javascript parts are required for a command and if so puts the required JS into memory ready for output later
        ///// </summary>
        ///// <param name="Cmd">The Command to check JS for</param>
        ///// <param name="CurrentMemory">Which memory block to add the functions into</param>
        //private void CheckForJavaScript(Interface_NetScriptCommand Cmd, Vortex_Memory CurrentMemory)
        //{
        //    // Check to see if there is any Javascript to go with this command:
        //    if (Cmd.JSFunctionTitles.Count > 0)
        //    {
        //        foreach (string item in Cmd.JSFunctionTitles)
        //        {
        //            if (CurrentMemory.JSFunctionTitles.ContainsKey(item) == false)
        //            {
        //                if (System.IO.File.Exists(Folder + "\\" + item + ".jspart") == true)
        //                {
        //                    CurrentMemory.JSFunctionTitles.Add(item, System.IO.File.ReadAllText(Folder + "\\" + item + ".jspart"));
        //                }
        //                else
        //                {
        //                    CurrentMemory.Error.Add("Javascript Part File Required by "+Cmd.Command+" but Not Found - " + item + ".jspart");
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// This is the base for the whole command system - run the selected command with the data that has been given
        /// Updated in 2019 to allow for "Legacy" commands - these are where I have changed a command's name and a script is loaded
        /// with the old name - if that happens, it replaces the old name with the new name and adds to the debug log a note that the command
        /// should be updated!
        /// Updated again in October 2019 to add some "System" commands!
        /// </summary>
        /// <param name="Command">The Command To Run</param>
        /// <param name="CurrentMemory">The Memory being used</param>
        /// <param name="NodeCache">The Cache of Data Nodes as required</param>
        /// <returns>The completed command (It may have been modified by the running process)</returns>
        public Token RunCommand(Token Command, Vortex_Memory CurrentMemory, Vortex.Node_AccessLayer NodeCache, bool Looped=false)
        {
            // New bit to see if this means we can return things!
            //if (CurrentMemory.ReturnerValue != null)
            //{
            //    Command.data = CurrentMemory.ReturnerValue;
            //    CurrentMemory.ReturnerValue = null;
            //}
            if (Command.data.StartsWith("System.") == true && Looped == false)
            {
                // System command - let's try!
                CheckSystemCommand(Command);
            }

            else if (CommandIndex.ContainsKey(Command.data) == true && Looped == false)
            {
                // Run the command
                Command = CommandList.Commands[CommandIndex[Command.data]].Value.Process(Command, CurrentMemory, NodeCache, this);
            }
            else if (Functions.ContainsKey(Command.data) == true)
            {
                Vortex_PreProcess.PreProcessElements(Command, CurrentMemory, NodeCache, this);
                string Script = Functions[Command.data].ToString();
                if (Command.TokenTestSubData != null && Command.TokenTestSubData.Count > 0)
                {
                    for (int i = 0; i < Command.TokenTestSubData.Count; i++)
                    {
                        if (Command.TokenTestSubData[i].TokenSubData != null && Command.TokenTestSubData[i].TokenSubData.Count > 1)
                        {
                            for (int j = 0; j < Command.TokenTestSubData[i].TokenSubData.Count; j++)
                            {
                                string temp = RecursiveData(Command.TokenTestSubData[i].TokenSubData[j]);

                                // Note - just changed this so the function elements are in quotes
                                // Script = Script.Replace("{" + Convert.ToString(i + j) + "}", "\""+Command.TokenTestSubData[i].TokenSubData[j].data +"\"");
                                // Scrub that - just put the code in quotes instead!
                                Script = Script.Replace("{" + Convert.ToString(i + j) + "}", temp);
                            }
                        }
                        else
                            Script = Script.Replace("{" + i.ToString() + "}", Command.TokenTestSubData[i].data);
                    }
                }
                if (Command.TokenSubData != null && Command.TokenSubData.Count > 0)
                {
                    for (int i = 0; i < Command.TokenSubData.Count; i++)
                    {
                        if (Command.TokenSubData[i].TokenSubData != null && Command.TokenSubData[i].TokenSubData.Count > 1)
                        {
                            for (int j = 0; j < Command.TokenSubData[i].TokenSubData.Count; j++)
                            {
                                string temp = RecursiveData(Command.TokenSubData[i].TokenSubData[j]);

                                // Note - just changed this so the function elements are in quotes
                                // Script = Script.Replace("{" + Convert.ToString(i + j) + "}", "\""+Command.TokenTestSubData[i].TokenSubData[j].data +"\"");
                                // Scrub that - just put the code in quotes instead!
                                Script = Script.Replace("{" + Convert.ToString(i + j) + "}", temp);
                            }
                        }
                        else
                            Script = Script.Replace("{" + i.ToString() + "}", Command.TokenSubData[i].data);
                    }
                }

                // CurrentMemory.ReturnerValue = null;
                Token CommandRunner = new Token(TokenInfo.Command, "RunScript");
                CommandRunner.TokenSubData = new List<Token>();
                CommandRunner.TokenSubData.Add(new Token(TokenInfo.Text, Script));
                Command = RunCommand(CommandRunner, CurrentMemory, NodeCache);
               
            }
            else if ((leg.LegacyCommand(Command.data) == true) && (Looped == false))
            {
                string message = "Legacy Command Detected - Replace " + Command.data + " with ";
                Command.data = leg.ProcessLegacy(Command.data);
                CommandList.Commands[CommandIndex[Command.data]].Value.Process(Command, CurrentMemory, NodeCache, this);
                CurrentMemory.DebugLog.Add("Line " + CurrentMemory.ScriptTreeIndex.ToString() + " in " + CurrentMemory.ScriptTreeName + " " + message + Command.data);
            }
            else if (Looped == false)
            {
                // hopefully this allows variable contents to run as commands and functions!
                Vortex_PreProcess.PreProcessElements(Command, CurrentMemory, NodeCache, this);
                if (CurrentMemory.Variables.ContainsKey(Command.data) == true)
                {
                    Command.data = CurrentMemory.Variables[Command.data];
                    Tools.TokeniseCommand(ref Command);
                    Command.data = Command.TokenSubData[0].data;
                    Command.TokenSubData.RemoveAt(0);
                    // Move all the items into test items:
                    while (Command.TokenSubData.Count > 0)
                    {
                        if (Command.TokenTestSubData == null)
                            Command.TokenTestSubData = new List<Token>();
                        Command.TokenTestSubData.Add(Command.TokenSubData[0]);
                        Command.TokenSubData.RemoveAt(0);
                    }
                    return RunCommand(Command, CurrentMemory, NodeCache, true);
                }
            }
            
            // return the command
            return Command;
        }

        protected string RecursiveData(Token data)
        {
            if (data.TokenSubData == null || data.TokenSubData.Count == 0)
                return data.data;
            else
            {
                string temp = "";
                for (int i = 0; i < data.TokenSubData.Count; i++)
                {
                    temp += " \"" + RecursiveData(data.TokenSubData[i])+ "\"";
                }
                return data.data + " " + temp;
            }
        }

        protected void CheckSystemCommand(Token Command)
        {
            if (Command.TokenTestSubData[0].data == SystemPasscode)
            {
                switch (Command.data)
                {
                    case "System.Stop":
                        // Stop the system:
                        CommandList = null;
                        GC.Collect();
                        break;
                    case "System.Restart":
                        // Restart the system:
                        Refresh();
                        break;
                    case "System.Reset":
                        // Stop and restart the system:
                        CommandList = null;
                        GC.Collect();
                        Refresh();
                        break;
                    case "System.SetupBaseSystem":
                        // Creates a copy of the DLL's etc into a sub directory (e.g. to set up a new application)
                        break;
                    case "System.ClearCache":
                        // Clears all the files from the file cache
                        System.IO.Directory.Delete(Folder + "\\Cache", true);
                        break;
                    case "System.RemoveFromCache":
                        // Clears a specific file from the file cache
                        System.IO.File.Delete(Folder + "\\Cache\\"+Command.TokenSubData[0].data);
                        break;
                }
            }
        }

        /// <summary>
        /// This does the same as the other Run Command, but deals with loop data - adds in the loop data, runs then removes it
        /// </summary>
        /// <param name="Command">The Command To Run</param>
        /// <param name="CurrentMemory">The Memory being used</param>
        /// <param name="NodeCache">The Cache of Data Nodes as required</param>
        /// <param name="Loop">The Loop Data To add</param>
        /// <returns>The completed command (It may have been modified by the running process)</returns>
        public Token RunCommand(Token Command, Vortex_Memory CurrentMemory, Vortex.Node_AccessLayer NodeCache, NetScript_LoopData Loop)
        {
            CurrentMemory.AddLoopData(Loop);
            // Just call the Run Command instead of having it in two places!
            Command = RunCommand(Command, CurrentMemory, NodeCache);
            CurrentMemory.RemoveLoopData();
            return Command;
        }
    }
    
    /// <summary>
    /// This is the Lazy container. Sorted Dictionary seems to be required at the moment, as it crashes with dictionary!
    /// </summary>
    public class NetScriptCommand_Container
    {
        [ImportMany]
        public System.Lazy<Interface_NetScriptCommand, SortedDictionary<string, object>>[] Commands { get; set; }
        
        public NetScriptCommand_Container(string Folder)
        {
            DirectoryCatalog catalog = new DirectoryCatalog(Folder, "*.dll");
            CompositionContainer container = new CompositionContainer(catalog);
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(this);
            container.Compose(batch);
        }


    }
}
