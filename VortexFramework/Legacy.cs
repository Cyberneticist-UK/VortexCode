using System.Collections.Generic;
using System.IO;

namespace Vortex
{
    /// <summary>
    /// While creating NetScript I made some breaking changes to how I named by commands. This keeps a record of the old commands and replaces them with the new 
    /// names in situ, while adding a note to the debug log that the script needs updating with the new command name.
    /// </summary>
    public class Legacy
    {
        Dictionary<string, string> LegacyCommands = new Dictionary<string, string>();
        public Legacy(string Folder)
        {
            FileInfo[] Maps = new DirectoryInfo(Folder).GetFiles("*.map");
            foreach(FileInfo file in Maps)
            {
                string[] lines = File.ReadAllLines(file.FullName);
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] items = lines[i].Split(new char[] { '\t' });
                    LegacyCommands.Add(items[0], items[1]);
                }
            }
        }
        /// <summary>
        /// Check to see if the command name appears in the legacy list
        /// </summary>
        /// <param name="CommandName">Command to check</param>
        /// <returns>true if command is a legacy one, false if not</returns>
        public bool LegacyCommand(string CommandName)
        {
            return LegacyCommands.ContainsKey(CommandName);
            // return (ProcessLegacy(CommandName) != "Legacy Command Not Found");
        }

        /// <summary>
        /// Replaces the legacy command name with the new name
        /// </summary>
        /// <param name="CommandName">Command name to check</param>
        /// <returns>The new command name</returns>
        public string ProcessLegacy(string CommandName)
        {
            return LegacyCommands[CommandName];
            //switch (CommandName)
            //{
            //    case "SortNodeGroup":
            //        return "NodeGroup.Sort";
            //    case "CreateNodeGroup":
            //        return "NodeGroup.Create";
            //    case "Inc":
            //        return "Var.Inc";
            //    case "VarExist":
            //        return "Var.Exists";
            //    case "Var.Exist":
            //        return "Var.Exists";
            //    case "ClearDebugLog":
            //        return "Debug.Clear";
            //    case "ClearDebugMode":
            //        return "Debug.Off";
            //    case "SetDebugMode":
            //        return "Debug.On";
            //    case "Dropdown":
            //        return "HTML.Dropdown";
            //    default:
            //        return "Legacy Command Not Found";
            //}
        }
    }
}
