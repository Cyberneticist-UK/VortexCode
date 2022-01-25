using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

namespace Vortex
{
    public static class Shared_Data
    {
        public static string GetScriptFileData(Token temp, Vortex_Memory CurrentMemory)
        {
            string Filename = Vortex_PreProcess.PreProcessFile(temp.data, CurrentMemory);
            string Script = "";
            if (Filename == "")
                Script = "<? WriteLn \"There was an error in loading a script file. Please See Error Log.\"; ?>";
            else
            {
                CurrentMemory.ScriptTreeName = Filename;
                Script = File.ReadAllText(Filename);
                // Replace non-breaking white space with normal space.
                Script = Script.Replace(" ", " ");
            }
            return Script;
        }

        /// <summary>
        /// This takes the curly bracket data from an HTML component and formats it into a string to add into the tag:
        /// </summary>
        /// <param name="data">The string containing all the tags and data to add into it</param>
        /// <returns>the string ready for use</returns>
        public static string ProcessHTMLTags(string data)
        {
            // first, split down the text into a string:-
            string[] Tags = data.Split(new char[] { ';' });
            string processed = "";
            foreach (string item in Tags)
            {
                if (item != "")
                {
                    // "style" "background-color:black" "item:x"
                    string p = item.Trim().Replace("\" \"", ";");
                    p = p.Replace("\"", "");
                    string[] SubTags = p.Split(new char[] { ';' });
                    p = "";
                    for (int i = 1; i < SubTags.Length; i++)
                    {
                        if (SubTags.Length == 2)
                            p += SubTags[i];
                        else
                            p += SubTags[i] + ";";
                    }
                    p = SubTags[0] + "=\"" + p + "\" ";
                    processed += p;
                }
            }
            return processed;
        }


        public static bool RunTest(Token LeftData, Token Operator, Token RightData, VortexTools t)
        {
            //LeftData = t.CheckData(LeftData.data);
            //RightData = t.CheckData(RightData.data);
            Enum.TryParse<Operator>(Operator.data, out Operator tempOperator);
            return RunTest(LeftData, tempOperator, RightData, t);
        }

        public enum Logic { And, Or}; 

        public static bool RunTest(Token Data, VortexTools t)
        {
            bool currentResult = true;
            Logic current = Logic.And;
            for (int i = 0; i < Data.TokenSubData.Count; i+=4)
            {
                Enum.TryParse<Operator>(Data.TokenSubData[i+1].data, out Operator tempOperator);
                switch (current)
                {
                    case Logic.And:
                        currentResult = currentResult && RunTest(Data.TokenSubData[i], tempOperator, Data.TokenSubData[i + 2], t);
                        break;
                    case Logic.Or:
                        currentResult = currentResult || RunTest(Data.TokenSubData[i], tempOperator, Data.TokenSubData[i + 2], t);
                        break;

                }
                if (Data.TokenSubData.Count > i+3)
                    current = (Logic)Enum.Parse(typeof(Logic), Data.TokenSubData[i + 3].data);
            }
            return currentResult;
        }

        public static Node RunNodeGroupTest(List<Node> NodeGroup, Field TestField, Token Operator, Token RightData, VortexTools t)
        {
            Token LeftData = new Token(TokenInfo.Text, "");
            Enum.TryParse<Operator>(Operator.data, out Operator tempOperator);
            foreach (Node item in NodeGroup)
            {
                LeftData = t.CheckData(item[TestField].ToString());
                if (RunTest(LeftData, tempOperator, RightData, t) == true)
                    return item;
            }
            return null;
        }

        public static bool RunTest(Token LeftData, Operator tempOperator, Token RightData, VortexTools t)
        {
            LeftData = t.CheckData(LeftData.data);
            RightData = t.CheckData(RightData.data);
            switch (tempOperator)
            {
                case Operator.Equal:
                    if (LeftData.IsOfType(TokenInfo.Date) && RightData.IsOfType(TokenInfo.Date))
                    {
                        return (Convert.ToDateTime(LeftData.data) == Convert.ToDateTime(RightData.data));
                    }
                    else if (LeftData.IsOfType(TokenInfo.Numeric) && RightData.IsOfType(TokenInfo.Numeric))
                        return (Convert.ToDecimal(LeftData.data) == Convert.ToDecimal(RightData.data));
                    else
                        return (LeftData.data == RightData.data);
                case Operator.NotEqual:
                    if (LeftData.IsOfType(TokenInfo.Date) && RightData.IsOfType(TokenInfo.Date))
                    {
                        return (Convert.ToDateTime(LeftData.data) != Convert.ToDateTime(RightData.data));
                    }
                    else if (LeftData.IsOfType(TokenInfo.Numeric) && RightData.IsOfType(TokenInfo.Numeric))
                        return (Convert.ToDecimal(LeftData.data) != Convert.ToDecimal(RightData.data));
                    else
                        return (LeftData.data != RightData.data);
                case Operator.Less:
                    if (LeftData.IsOfType(TokenInfo.Date) && RightData.IsOfType(TokenInfo.Date))
                    {
                        return (Convert.ToDateTime(LeftData.data) < Convert.ToDateTime(RightData.data));
                    }
                    else if (LeftData.IsOfType(TokenInfo.Numeric) && RightData.IsOfType(TokenInfo.Numeric))
                        return (Convert.ToDecimal(LeftData.data) < Convert.ToDecimal(RightData.data));
                    else
                        return false;
                case Operator.LessEqual:
                    if (LeftData.IsOfType(TokenInfo.Date) && RightData.IsOfType(TokenInfo.Date))
                    {
                        return (Convert.ToDateTime(LeftData.data) <= Convert.ToDateTime(RightData.data));
                    }
                    else if (LeftData.IsOfType(TokenInfo.Numeric) && RightData.IsOfType(TokenInfo.Numeric))
                        return (Convert.ToDecimal(LeftData.data) <= Convert.ToDecimal(RightData.data));
                    else
                        return false;
                case Operator.Greater:
                    if (LeftData.IsOfType(TokenInfo.Date) && RightData.IsOfType(TokenInfo.Date))
                    {
                        return (Convert.ToDateTime(LeftData.data) > Convert.ToDateTime(RightData.data));
                    }
                    else if (LeftData.IsOfType(TokenInfo.Numeric) && RightData.IsOfType(TokenInfo.Numeric))
                        return (Convert.ToDecimal(LeftData.data) > Convert.ToDecimal(RightData.data));
                    else
                        return false;
                case Operator.GreaterEqual:
                    if (LeftData.IsOfType(TokenInfo.Date) && RightData.IsOfType(TokenInfo.Date))
                    {
                        return (Convert.ToDateTime(LeftData.data) >= Convert.ToDateTime(RightData.data));
                    }
                    else if (LeftData.IsOfType(TokenInfo.Numeric) && RightData.IsOfType(TokenInfo.Numeric))
                        return (Convert.ToDecimal(LeftData.data) >= Convert.ToDecimal(RightData.data));
                    else
                        return false;
                case Operator.Contains:
                    return LeftData.data.Contains(RightData.data);
                case Operator.EndsWith:
                    return LeftData.data.EndsWith(RightData.data);
                case Operator.StartsWith:
                    return LeftData.data.StartsWith(RightData.data);
                default:
                    return false;
            }
        }
        
        public static void ScriptRunner(string Script, Vortex_Memory CurrentMemory, Node_AccessLayer NodeCache, NetScript_Command CommandProcessor)
        {
            Token CommandRunner = new Token(TokenInfo.Command, "RunScript");
            CommandRunner.TokenSubData = new List<Token>();
            CommandRunner.TokenSubData.Add(new Token(TokenInfo.Text, Script));
            CommandRunner = CommandProcessor.RunCommand(CommandRunner, CurrentMemory, NodeCache);
        }

        public static string ProcessArray(string Script, Token Array)
        {
            for (int i = 0; i < Array.TokenSubData.Count; i++)
            {
                Script = Script.Replace("{" + i.ToString() + "}", Array.TokenSubData[i].data);
            }
            return Script;
        }

        public static string ProcessArray(string Script, List<string> Array)
        {
            for (int i = 0; i < Array.Count; i++)
            {
                Script = Script.Replace("{" + i.ToString() + "}", Array[i]);
            }
            return Script;
        }


        /// <summary>
        /// This is the generic code for running the script file, and is currently used by RunScriptFile and RunScriptIntoVar
        /// </summary>
        /// <param name="Filename">The filename to use</param>
        /// <param name="CurrentMemory">The memory to put the output into - note you may want to create a new memory object before running this code!</param>
        /// <param name="CommandProcessor">A link to the command processor</param>
        /// <param name="NodeCache">A link to the current Node Cache</param>
        /// <param name="Password">If the file is encrypted, this should be the decryption key. Null or blank if not</param>
        /// <param name="TemplateItems">Replaces {0}... {i} with this list. Only use this or Array items, never both!</param>
        /// <param name="ArrayItems">Replaces {0}... {i} with values from this array. Only use this or template items, never both!</param>
        public static void RunScriptFile(Token Filename, ref Vortex_Memory CurrentMemory, ref NetScript_Command CommandProcessor, ref Node_AccessLayer NodeCache, string Password, List<string> TemplateItems, Token ArrayItems)
        {
            // Process the filename:
            if (Filename.IsOfType(TokenInfo.Command))
                Filename = CommandProcessor.RunCommand(Filename, CurrentMemory, NodeCache);
            string Script = GetScriptFileData(Filename, CurrentMemory);
            // Process the file if encrypted:
            if(Password != null && Password != "" && Script != "<? WriteLn \"There was an error in loading a script file. Please See Error Log.\"; ?>")
                Script = Sec_Crypt.AESDecrypt(Script, Password);
            // Add in the template items if they are present:
            if (TemplateItems!= null && TemplateItems.Count > 0)
                Script = ProcessArray(Script, TemplateItems);
            // And the array items if they are present:
            if (ArrayItems.TokenSubData != null && ArrayItems.TokenSubData.Count > 0)
                Script = ProcessArray(Script, ArrayItems);
            // Now run the script in the current memory:
            Shared_Data.ScriptRunner(Script, CurrentMemory, NodeCache, CommandProcessor);
            // And we are done!
        }

        /// <summary>
        /// This is the generic code for running the script file, and is currently used by RunScriptFile and RunScriptIntoVar
        /// </summary>
        /// <param name="Filename">The filename to use</param>
        /// <param name="CurrentMemory">The memory to put the output into - note you may want to create a new memory object before running this code!</param>
        /// <param name="CommandProcessor">A link to the command processor</param>
        /// <param name="NodeCache">A link to the current Node Cache</param>
        /// <param name="Password">If the file is encrypted, this should be the decryption key. Null or blank if not</param>
        /// <param name="TemplateItems">Replaces {0}... {i} with this list. Only use this or Array items, never both!</param>
        /// <param name="ArrayItems">Replaces {0}... {i} with values from this array. Only use this or template items, never both!</param>
        public static void RunScript(string Script, ref Vortex_Memory CurrentMemory, ref NetScript_Command CommandProcessor, ref Node_AccessLayer NodeCache)
        {
            
            // Now run the script in the current memory:
            Shared_Data.ScriptRunner(Script, CurrentMemory, NodeCache, CommandProcessor);
            // And we are done!
        }

    }
}
