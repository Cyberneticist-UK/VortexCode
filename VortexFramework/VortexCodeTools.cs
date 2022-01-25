using System;
using System.Collections.Generic;
using System.Drawing;

namespace Vortex
{
    /// <summary>
    /// Version 1.0 - Standardised 11th Jan 2019
    /// Last Updated - 18/10/2021
    /// Last Updated - 21/1/2019
    /// Tools to process raw files and data into a suitable data structure for use in NetScript.
    /// Tools to deal with errors in the script that can be picked up and reported on.
    /// Updated to work with issues where you could have brackets inside quotes being picked up as part of the command
    /// </summary>
    public class VortexTools
    {
        /// <summary>
        /// The Command Processor to use for the Scripting Engine
        /// </summary>
        NetScript_Command CommandProcessor;

        /// <summary>
        /// Create a new instance of VortexTools
        /// </summary>
        /// <param name="CommandFolder">Where to find the Commands being used by this Script Engine</param>
        public VortexTools(string CommandFolder, string Passcode, string SMTP = "", string SMTPUsername = "", string SMTPPassword = "")
        {
            CommandProcessor = new NetScript_Command(CommandFolder, Passcode, SMTP, SMTPUsername, SMTPPassword);
        }

        /// <summary>
        /// Create a new instance of VortexTools
        /// </summary>
        /// <param name="CommandProcessor">The CommandProcessor being used by this Script Engine</param>
        public VortexTools(NetScript_Command CommandProcessor)
        {
            this.CommandProcessor = CommandProcessor;
        }

        #region Processing Script Files
        
        /// <summary>
        /// A script is made up of HTML and NetScript Code, with the NetScript Code separated from the
        /// HTML through the use of <? and ?> to signify the start and end of code. This code separates the script into
        /// HTML and single-line Commands to run.
        /// </summary>
        /// <param name="Script">The Script Data</param>
        /// <param name="InScriptCode">Whether to assume you start in code - default false</param>
        /// <returns>The HTML | Comments version of the script</returns>
        public List<Token> BreakdownScript(string Script, bool InScriptCode = false)
        {
            List<Token> Commands = new List<Token>();
            // Scan the script, dividing by semi colons:
            bool InQuote = false;
            Script = CleanScript(Script);
            string temp = "";
            bool DontAddCharacter = false;
            int CurlyBrackets = 0;

            // This currently checks each single character in the script to divide into HTML and Commands.
            for (int i = 0; i < Script.Length; i++)
            {
                // We are looking for special characters only in scriptcode:
                if (InScriptCode == false)
                {
                    // We are basically within the HTML Section here:
                    if (Script[i] == '?' && Script[i - 1] == '<')
                    {
                        // This is a start of a command, so save what is there in temp and start again as a command:
                        if (temp != "" && temp != "<")
                            Commands.Add(new Token(TokenInfo.HTML, temp.Trim().TrimEnd('<')));
                        temp = "";
                        InScriptCode = true;
                        // We don't want to add the ? into the start of our command string, so tell
                        // the system not to add it:
                        DontAddCharacter = true;
                    }
                }
                else if (InScriptCode == true)
                {
                    switch (Script[i])
                    {
                        // Entering / Exiting a quote
                        case '\"':
                            // Are we already in a quote?
                            if (InQuote == false)
                            {
                                // We are now going to be inquote in code:
                                InQuote = true;
                            }
                            // If we are in a quote and the previous item wasn't a slash, eg a \" in a quote,
                            // We have come to the end:
                            else if (InQuote == true && Script[i - 1] != '\\')
                            {
                                // We have come to the end of a quote in code!
                                InQuote = false;
                            }
                            break;
                        case '{':
                            if (InQuote == false)
                            {
                                CurlyBrackets++;
                            }
                            break;
                        // End of a command line
                        case ';':
                            if (CurlyBrackets == 0)
                            {
                                if (InQuote == false)
                                {
                                    // This is a command line:
                                    if (temp != "")
                                        Commands.Add(new Token(TokenInfo.Command, temp.Trim()));
                                    temp = "";
                                    // We don't want to add the ; into the start of our next command string, so tell
                                    // the system not to add it:
                                    DontAddCharacter = true;
                                }
                            }
                            break;
                        // End of a command line - bracket
                        case '}':
                            if (InQuote == false)
                            {
                                CurlyBrackets--;
                            }
                            break;
                        // End of a command block:
                        case '>':
                            if (InQuote == false && CurlyBrackets==0 && Script[i - 1] == '?')
                            {
                                if (temp != "" && temp != "?")
                                {
                                    Commands.Add(new Token(TokenInfo.Command, temp.Trim().TrimEnd('?')));
                                }
                                temp = "";
                                InScriptCode = false;
                                // We don't want to add the > into the start of our HTML string, so tell
                                // the system not to add it:
                                DontAddCharacter = true;
                            }
                            break;
                    }

                }

                // If we should, add the character into our temporary holding string!
                if (DontAddCharacter == false)
                    temp += Script[i];
                else
                    // Next character we will add!
                    DontAddCharacter = false;

            }
            // We should now have all the commands from temp! There should never be a time when the end is not a 
            // closed ?>, this will need to be checked for by seeing if we are still inscriptcode == true by end of script:

            if (temp != "")
            {
                Commands.Add(new Token(TokenInfo.HTML, temp));
            }
            return Commands;
        }

        /// <summary>
        /// Takes the Script tree processed by Breakdown script and properly Tokenises it
        /// </summary>
        /// <param name="ScriptTree">The Script tree from Breakdown script</param>
        /// <returns>A fully runnable Tokenised script</returns>
        public List<Token> TokeniseScriptTree(List<Token> ScriptTree)
        {
            // Tokenise the Stage 1 script:
            Token temp = new Token();
            for (int i = 0; i < ScriptTree.Count; i++)
            {
                if (ScriptTree[i].IsOfType(TokenInfo.HTML) == false)
                {
                    temp = ScriptTree[i];
                    TokeniseCommand(ref temp);
                    ScriptTree[i] = temp;
                }
            }
            for (int i = 0; i < ScriptTree.Count; i++)
            {
                if (ScriptTree[i].data == "")
                {
                    ScriptTree.RemoveAt(i);
                    i--;
                }
            }
            return ScriptTree;
        }

        private void InsertToken(ref Token CommandToken, string temp, bool TrimString = true)
        {
            if (TrimString)
                temp = temp.Trim();
            if (CommandToken.TokenSubData == null)
            {
                // This is the first time we have reached a breakpoint - end of first word! 
                CommandToken.TokenSubData = new List<Token>();
                // Set this item as our "outer token" if you like!
                CommandToken.data = temp;
                CommandToken.TypeInfo = CheckTokenValues(CommandToken.data);
            }
            else
            {
                // Something that is not a command or a quote, hopefully!
                if (TrimString == false)
                {
                    if (temp.Trim() == "{empty}")
                        CommandToken.TokenSubData.Add(CheckData(""));
                    else if (temp.Trim() == "{space}")
                        CommandToken.TokenSubData.Add(CheckData(" "));
                    else
                    {
                        CommandToken.TokenSubData.Add(CheckData(temp));
                    }
                }
                else
                {
                    if (temp.Trim() == "{empty}")
                        CommandToken.TokenSubData.Add(CheckData(""));
                    else if (temp.Trim() == "{space}")
                        CommandToken.TokenSubData.Add(CheckData(" "));
                    else if (temp.Trim() != "")
                    {
                        CommandToken.TokenSubData.Add(CheckData(temp));
                    }
                }
            }
        }

        private void InsertToken(ref Token CommandToken, Token NewToken)
        {
            if (CommandToken.TokenSubData == null)
            {
                // This is the first time we have reached a breakpoint - end of first word! 
                CommandToken.TokenSubData = new List<Token>();
            }
            // Something that is not a command or a quote, hopefully!
            CommandToken.TokenSubData.Add(NewToken);
        }

        private void InsertTestToken(ref Token CommandToken, string temp)
        {
            if (CommandToken.TokenTestSubData == null)
            {
                // This is the first time we have reached a breakpoint - end of first word! 
                CommandToken.TokenTestSubData = new List<Token>();
            }
            // Something that is not a command or a quote, hopefully!
            if (temp.Trim() != "")
                CommandToken.TokenTestSubData.Add(CheckData(temp.Trim()));
        }

        private void InsertTestToken(ref Token CommandToken, Token NewToken)
        {
            if (CommandToken.TokenTestSubData == null)
            {
                // This is the first time we have reached a breakpoint - end of first word! 
                CommandToken.TokenTestSubData = new List<Token>();
            }
            // Something that is not a command or a quote, hopefully!
            CommandToken.TokenTestSubData.Add(NewToken);
        }

        /// <summary>
        /// Takes a command token and separates it out into the command/variable (returned in data) and 
        /// The pattern of data that went with this instruction (inside the TokenSubData array in the token)
        /// </summary>
        /// <param name="CommandToken">The first-level command (When a script is first processed)</param>
        public void TokeniseCommand(ref Token CommandToken)
        {
            // Takes a command token and set and breaks it down into it's constituent parts:
            string Command = CommandToken.data;
            string temp = "";
            bool InQuote = false;
            bool DontAddCharacter = false;
            // New code - if something is in square brackets and in a quote, it gets taken as a part of the code which isn't necessarily the case
            bool Noted = false;
            int InSquareBlock = 0;
            string PreviousChar = "";
            for (int i = 0; i < Command.Length; i++)
            {
                if (InQuote == true)
                {
                    if (Command[i] == '\"') //  && Command[i - 1] != '\\'
                    {
                        if (i == 0 || (i > 0 && Command[i - 1] != '\\'))
                        {
                            // We have come to the end of a quote! add it to the breakdown:
                            InQuote = false;
                            // Replace the backslash before a quote with just a quote:
                            temp = temp.Replace("\\\"", "\"");
                            temp = temp.Replace("\\r", Convert.ToString('\r'));
                            temp = temp.Replace("\\n", Convert.ToString('\n'));
                            temp = temp.Replace("\\t", Convert.ToString('\t'));
                            if (temp == "")
                                temp = "{empty}";
                            if (temp == " ")
                                temp = "{space}";
                            InsertToken(ref CommandToken, temp, false);
                            if (CommandToken.TokenSubData.Count > 0)
                                CommandToken.TokenSubData[CommandToken.TokenSubData.Count - 1].SetType(TokenInfo.Quote);
                            temp = "";
                            DontAddCharacter = true;
                        }
                    }
                }
                else
                {
                    switch (Command[i])
                    {
                        // Test Blocks (rounded brackets) can be nested:
                        case '(':
                            if (InSquareBlock == 0 && Noted == false)
                            {
                                // check there isnt some data to add in first:
                                if (temp.Trim() != "")
                                {
                                    InsertToken(ref CommandToken, temp);
                                    temp = "";
                                }
                                // Have moved this here because rounded brackets shouldn't always be deleted!
                                DontAddCharacter = true;
                            }
                            if (Noted == false)
                                InSquareBlock++;
                            break;
                        case ')':
                            if (InSquareBlock > 1 && Noted == false)
                                InSquareBlock--;
                            else if (InSquareBlock == 1 && Noted == false)
                            {
                                InSquareBlock = 0;
                                // Need to process this test block:
                                Token squareItem = new Token(TokenInfo.Text, temp.Trim());
                                squareItem.TokenSubData = new List<Token>();
                                TokeniseCommand(ref squareItem);
                                //foreach (Token item in squareItem.TokenSubData)
                                //    InsertTestToken(ref CommandToken, item);
                                // NEW BIT - Make the test token contain sub items so can have multiple tests eg If Else if
                                InsertTestToken(ref CommandToken, squareItem);
                                temp = "";
                                // Now it's processed we can continue! Don't add the final round bracket:
                                DontAddCharacter = true;
                            }
                            break;
                        // Square blocks can be nested:
                        case '[':
                            if (InSquareBlock == 0 && Noted == false)
                            {
                                // check there isnt some data to add in first:
                                if (temp.Trim() != "")
                                {
                                    InsertToken(ref CommandToken, temp);
                                    temp = "";
                                }
                                DontAddCharacter = true;
                            }
                           if(Noted == false)
                                InSquareBlock++;
                            break;
                        case ']':
                            if (InSquareBlock > 1 && Noted == false)
                                InSquareBlock--;
                            else if (InSquareBlock == 1 && Noted == false)
                            {
                                InSquareBlock = 0;
                                // Need to process this square block:
                                Token squareItem = new Token(TokenInfo.Command, temp.Trim());
                                TokeniseCommand(ref squareItem);
                                InsertToken(ref CommandToken, squareItem);
                                temp = "";
                                // Now it's processed we can continue! Don't add the final square bracket:
                                DontAddCharacter = true;
                            }
                            break;
                        // curly blocks can be nested:
                        case '{':
                            if (InSquareBlock == 0 && Noted == false)
                            {
                                // check there isnt some data to add in first:
                                if (temp.Trim() != "")
                                {
                                    InsertToken(ref CommandToken, temp);
                                    temp = "";
                                }
                                DontAddCharacter = true;
                            }
                            if (Noted == false)
                                InSquareBlock++;
                            break;
                        case '}':
                            // For this system now, the curly brackets are used to keep all the code into a single component together
                            if (InSquareBlock > 1 && Noted == false)
                                InSquareBlock--;
                            else if (InSquareBlock == 1 && Noted == false)
                            {
                                InSquareBlock = 0;
                                // Need to process this code block:
                                Token Block = new Token(TokenInfo.InBrackets, temp.Trim());
                                InsertToken(ref CommandToken, Block);
                                temp = "";
                                // Now it's processed we can continue! Don't add the final curly bracket:
                                DontAddCharacter = true;
                            }
                            break;
                        case '\"':
                            if (i == 0 || (i > 0 && Command[i - 1] != '\\'))
                            {
                                if (InSquareBlock == 0)
                                {
                                    // We are now going to be inquote, but this quote will not be added to the string:
                                    InQuote = true;
                                    DontAddCharacter = true;
                                }
                                else
                                {
                                    Noted = !Noted;
                                }
                            }
                            break;
                        case ' ':
                            // Only if not in a square block!
                            if (InSquareBlock == 0)
                            {
                                // As we arent in a quote, this should be a new element as spaces delimit:
                                InsertToken(ref CommandToken, temp);
                                temp = "";

                                DontAddCharacter = true;
                            }
                            break;
                    }
                }

                // If we should, add the character into our temporary holding string!
                if (DontAddCharacter == false)
                    temp += Command[i];
                else
                    // Next character we will add!
                    DontAddCharacter = false;
                if (Command[i] != ' ')
                    PreviousChar = Command[i].ToString();
            }
            // At the end, add whatever is left:
            if (temp != "")
            {
                InsertToken(ref CommandToken, temp);
            }
        }

        /// <summary>
        /// This cleans any unnecessary white space and comments from an HTML script
        /// </summary>
        /// <param name="Script">The HTML and NetScript script</param>
        /// <returns>A nice shiny clean script!</returns>
        public string CleanScript(string Script)
        {
            // Quickly go through the script and only remove characters that are not in quotes:
            Script = RemoveComments(Script);
            char[] ScriptArray = Script.ToCharArray();
            bool inQuote = false;
            for (int i = 0; i < ScriptArray.Length; i++)
            {
                if(ScriptArray[i] == '\"' && inQuote == false)
                {
                    inQuote = true;
                }
                else if (ScriptArray[i] == '\"' && inQuote == true)
                {
                    inQuote = false;
                }
                if ((ScriptArray[i] == '\r' || ScriptArray[i] == '\n' || ScriptArray[i] == '\t') && inQuote == false)
                {
                    ScriptArray[i] = ' ';
                    // Script = Script.Remove(i, 1);
                    i--;
                }
            }
            
            return new string(ScriptArray);
        }

        /// <summary>
        /// Remove any comments there may be in the script
        /// </summary>
        /// <param name="Script">The script data with comments</param>
        /// <returns>The script data without comments!</returns>
        protected string RemoveComments(string Script)
        {
            // In this, we try and get any sub nodes that may be necessary to process before the rest of the command!
            Script = Script.Trim();
            while (Script.IndexOf("<!--") != -1)
            {
                int Start = Script.IndexOf("<!--");
                int End = Script.IndexOf("-->", Start);
                Script = Script.Substring(0, Start) +
                    Script.Substring(End + 3);
            }
            return Script;
        }

        #endregion


        #region Processing a Token - Checking it's types etc

        /// <summary>
        /// Check which types the data matches and return the token with those set to true
        /// </summary>
        /// <param name="temp">the data string to check</param>
        /// <returns>A token with all of the matching types set to true</returns>
        public Token CheckData(string temp, bool RunSymbolic = true)
        {
            // This needs a more permanent fix, where the operator tests are only run if necessary, not on every run!
            decimal tempValue = 0;
            int tempValueInt = 0;
            StringFormat tempFormat;
            Operator tempOperator;
            SpecialItems tempSpecial;
            Field tempField;
            string temp2 = temp;
            // Firstly, if temp equals any symbolic version of equal etc, change it into its correct item:
            if (RunSymbolic)
            {
                switch (temp)
                {
                    case "==":
                        temp2 = Operator.Equal.ToString();
                        break;
                    case "=":
                        temp2 = Operator.Equal.ToString();
                        break;
                    case "!=":
                        temp2 = Operator.NotEqual.ToString();
                        break;
                    case ">":
                        temp2 = Operator.Greater.ToString();
                        break;
                    case "<":
                        temp2 = Operator.Less.ToString();
                        break;
                    case ">=":
                        temp2 = Operator.GreaterEqual.ToString();
                        break;
                    case "<=":
                        temp2 = Operator.LessEqual.ToString();
                        break;
                    case "=>":
                        temp2 = Operator.GreaterEqual.ToString();
                        break;
                    case "=<":
                        temp2 = Operator.LessEqual.ToString();
                        break;
                }
            }

            Token Returner = new Token(TokenInfo.Text, temp2);
            DateTime tempDate;
            if (temp != temp2)
            {
                Returner.TokenSubData = new List<Token>();
                Returner.TokenSubData.Add(new Token(TokenInfo.Text, temp));
                if (DateTime.TryParse(temp, out tempDate) == true)
                {
                    Returner.TokenSubData[0].SetType(TokenInfo.Date);
                }
                // Can it be a number:
                if (Decimal.TryParse(temp, out tempValue) == true)
                {
                    Returner.TokenSubData[0].SetType(TokenInfo.Numeric);
                    Returner.TokenSubData[0].SetType(TokenInfo.Decimal);
                }
                if (Int32.TryParse(temp, out tempValueInt) == true)
                {
                    Returner.TokenSubData[0].SetType(TokenInfo.Numeric);
                    Returner.TokenSubData[0].SetType(TokenInfo.Integer);
                }
                // Can it be a boolean?
                if (Boolean.TryParse(temp, out bool tempBool2) == true)
                {
                    Returner.TokenSubData[0].SetType(TokenInfo.Boolean);
                }
                // Can it be a GUID?
                if (System.Guid.TryParse(temp, out Guid tempGuid2) == true)
                {
                    Returner.TokenSubData[0].SetType(TokenInfo.GUID);
                }

                // Can it be a command:
                if (CommandProcessor.CheckCommand(temp) == true)
                // if (Enum.TryParse<Command>(temp, out tempCommand) == true)
                {
                    Returner.TokenSubData[0].SetType(TokenInfo.Command);
                }
                // Can it be Node Data? Does it say Node?
                if (temp == "Node")
                {
                    Returner.TokenSubData[0].SetType(TokenInfo.NodeData);
                }
                // Can it be a special item:
                if (Enum.TryParse<SpecialItems>(temp, out tempSpecial) == true)
                {
                    Returner.TokenSubData[0].SetType(TokenInfo.SpecialItems);
                }
                // can it be a string format:
                if (Enum.TryParse<StringFormat>(temp, out tempFormat) == true)
                {
                    Returner.TokenSubData[0].SetType(TokenInfo.StringFormat);
                }
                // Can it be an operator:
                if (Enum.TryParse<Operator>(temp, out tempOperator) == true)
                {
                    Returner.TokenSubData[0].SetType(TokenInfo.Operator);
                }
                // Can it be a Field:
                if (Enum.TryParse<Field>(temp, out tempField) == true)
                {
                    Returner.TokenSubData[0].SetType(TokenInfo.Field);
                }
            }
            // Set the flags for this token:
            // Can it be a date:
            
            if (DateTime.TryParse(temp2, out tempDate) == true)
            {
                Returner.SetType(TokenInfo.Date);
            }
            // Can it be a number:
            if (Decimal.TryParse(temp2, out tempValue) == true)
            {
                Returner.SetType(TokenInfo.Numeric);
                Returner.SetType(TokenInfo.Decimal);
            }
            if (Int32.TryParse(temp2, out tempValueInt) == true)
            {
                Returner.SetType(TokenInfo.Numeric);
                Returner.SetType(TokenInfo.Integer);
            }
            // Can it be a boolean?
            if (Boolean.TryParse(temp2, out bool tempBool) == true)
            {
                Returner.SetType(TokenInfo.Boolean);
            }
            // Can it be a GUID?
            if (System.Guid.TryParse(temp2, out Guid tempGuid) == true)
            {
                Returner.SetType(TokenInfo.GUID);
            }

            // Can it be a command:
            if (CommandProcessor.CheckCommand(temp2) == true)
            // if (Enum.TryParse<Command>(temp, out tempCommand) == true)
            {
                Returner.SetType(TokenInfo.Command);
            }
            // Can it be Node Data? Does it say Node?
            if (temp2 == "Node")
            {
                Returner.SetType(TokenInfo.NodeData);
            }
            // Can it be a special item:
            if (Enum.TryParse<SpecialItems>(temp2, out tempSpecial) == true)
            {
                Returner.SetType(TokenInfo.SpecialItems);
            }
            // can it be a string format:
            if (Enum.TryParse<StringFormat>(temp2, out tempFormat) == true)
            {
                Returner.SetType(TokenInfo.StringFormat);
            }
            // Can it be an operator:
            if (Enum.TryParse<Operator>(temp2, out tempOperator) == true)
            {
                Returner.SetType(TokenInfo.Operator);
            }
            // Can it be a Field:
            if (Enum.TryParse<Field>(temp2, out tempField) == true)
            {
                Returner.SetType(TokenInfo.Field);
            }
            return Returner;
        }

        public Int64 CheckTokenValues(string temp)
        {
            Token Returner = CheckData(temp);
            return Returner.TypeInfo;
        }

        public Variable SetVariableTypes(Variable item)
        {
            item.ClearAllTypes();
            string temp2 = item.Data;
            item.SetType(TokenInfo.Text);
            if (DateTime.TryParse(temp2, out DateTime tempDate) == true)
            {
                item.SetType(TokenInfo.Date);
            }
            // Can it be a number:
            if (Decimal.TryParse(temp2, out Decimal tempValue) == true)
            {
                item.SetType(TokenInfo.Numeric);
                item.SetType(TokenInfo.Decimal);
            }
            if (Int32.TryParse(temp2, out int tempValueInt) == true)
            {
                item.SetType(TokenInfo.Numeric);
                item.SetType(TokenInfo.Integer);
            }
            // Can it be a boolean?
            if (Boolean.TryParse(temp2, out bool tempBool) == true)
            {
                item.SetType(TokenInfo.Boolean);
            }
            // Can it be a GUID?
            if (System.Guid.TryParse(temp2, out Guid tempGuid) == true)
            {
                item.SetType(TokenInfo.GUID);
            }

            // Can it be a command:
            if (CommandProcessor.CheckCommand(temp2) == true)
            // if (Enum.TryParse<Command>(temp, out tempCommand) == true)
            {
                item.SetType(TokenInfo.Command);
            }
            // Can it be Node Data? Does it say Node?
            if (temp2 == "Node")
            {
                item.SetType(TokenInfo.NodeData);
            }
            // Can it be a special item:
            if (Enum.TryParse<SpecialItems>(temp2, out SpecialItems tempSpecial) == true)
            {
                item.SetType(TokenInfo.SpecialItems);
            }
            // can it be a string format:
            if (Enum.TryParse<StringFormat>(temp2, out StringFormat tempFormat) == true)
            {
                item.SetType(TokenInfo.StringFormat);
            }
            // Can it be an operator:
            if (Enum.TryParse<Operator>(temp2, out Operator tempOperator) == true)
            {
                item.SetType(TokenInfo.Operator);
            }
            // Can it be a Field:
            if (Enum.TryParse<Field>(temp2, out Field tempField) == true)
            {
                item.SetType(TokenInfo.Field);
            }

            return item;
        }

        #endregion

        #region Standard Error Checking

        /// <summary>
        /// This object can be used to make a list of what the function needs for checking it through:
        /// </summary>
        public class RequiredElements
        {
            public bool RequiresDatabase = false;
            public bool RequiresLogin = false;
            public bool RequiresEmail = false;
            public List<string> VariableNames = new List<string>();
            public List<string> NodeGroups = new List<string>();
            public int MinimumSubDataItems = 0;
            public int MinimumTestSubDataItems = 0;
        }

        /// <summary>
        /// A Command Pre-Processor, checks some basics the command may need is in place before it is run:
        /// </summary>
        /// <param name="Command">Command Name</param>
        /// <param name="Data">The Full Command Structure being tested</param>
        /// <param name="CurrentMemory">Link to the memory being used</param>
        /// <param name="NodeCache">The Node Database Connection</param>
        /// <param name="RequiredItems">What this command needs to run</param>
        /// <returns>True if command passes, false if errors occur</returns>
        public bool CheckForErrors(string Command, ref Token Data, ref Vortex_Memory CurrentMemory, ref Node_AccessLayer NodeCache, RequiredElements RequiredItems)
        {
            bool Result = true;
            if (RequiredItems.RequiresDatabase == true && NodeCache == null)
            {
                CurrentMemory.Error.Add(Command+" Failed - No connection to the database found");
                CurrentMemory.Error.ErrorFlag = true;
                Result = false;
            }
            if (RequiredItems.RequiresLogin == true && CurrentMemory.Session == null)
            {
                CurrentMemory.Error.Add(Command + " Failed - You must be logged in to use this");
                CurrentMemory.Error.ErrorFlag = true;
                Result = false;
            }
            if (RequiredItems.RequiresEmail == true && CommandProcessor.EmailServer == null)
            {
                CurrentMemory.Error.Add(Command + " Failed - You must set up the SMTP Server to use this");
                CurrentMemory.Error.ErrorFlag = true;
                Result = false;
            }
            if ((Data.TokenSubData == null && RequiredItems.MinimumSubDataItems > 0) || (Data.TokenSubData != null && RequiredItems.MinimumSubDataItems > Data.TokenSubData.Count))
            {
                CurrentMemory.Error.Add(Command + " Failed - Parameters for command not correct");
                CurrentMemory.Error.ErrorFlag = true;
                Result = false;
            }
            if ((Data.TokenTestSubData == null && RequiredItems.MinimumTestSubDataItems > 0) || (Data.TokenTestSubData != null && RequiredItems.MinimumTestSubDataItems > Data.TokenTestSubData.Count))
            {
                CurrentMemory.Error.Add(Command + " Failed - Test parameters for command not correct");
                CurrentMemory.Error.ErrorFlag = true;
                Result = false;
            }
            foreach (string item in RequiredItems.VariableNames)
            {
                if(CurrentMemory.Variables.ContainsKey(item) == false)
                {
                    CurrentMemory.Error.Add(Command + " Failed - Required Variable "+item+" not found");
                    CurrentMemory.Error.ErrorFlag = true;
                    Result = false;
                }
            }
            foreach (string item in RequiredItems.NodeGroups)
            {
                if (CurrentMemory.NodeGroups.ContainsKey(item) == false)
                {
                    CurrentMemory.Error.Add(Command + " Failed - Required NodeGroup " + item + " not found");
                    CurrentMemory.Error.ErrorFlag = true;
                    Result = false;
                }
            }
            return Result;
        }



        #endregion

        #region Variable Adding To Memory

        /// <summary>
        /// This runs when adding a variable, to make sure that there isn't an object with the same name that could confuse a script
        /// </summary>
        /// <param name="GraphicName">The name you want to use</param>
        /// <param name="Filename">The filename from the graphic</param>
        /// <param name="CurrentMemory">The memory object to check</param>
        /// <param name="command">Which command this came from</param>
        /// <param name="CanOverwriteNonLockedExistingVariable">allow an existing variable with the same name to be overwritten, if it isn't locked</param>
        /// <param name="WhereFrom">Shows where the Variable was taken from originally - can be used for security to not allow URL (Querystring) variables, for example</param>
        /// <returns>True if variable written to memory, false if not. Reason given in error log.</returns>
        public bool AddGraphic(string GraphicName, string Filename, Vortex_Memory CurrentMemory, Token command, bool CanOverwriteExistingGraphic, bool AutoRotate = true)
        {
            if (CurrentMemory.NodeGroups.ContainsKey(GraphicName) == true)
                CurrentMemory.Error.Add(command, "Name " + GraphicName + " already used in NodeGroups");
            else if (CurrentMemory.VarArrays.ContainsKey(GraphicName))
                CurrentMemory.Error.Add(command, "Name " + GraphicName + " already used in VarArrays");
            else if (CurrentMemory.Variables.ContainsKey(GraphicName) == true)
                CurrentMemory.Error.Add(command, "Name " + GraphicName + " already used in Variables");
            else if (CurrentMemory.Graphics.ContainsKey(GraphicName) == false)
            {
                CurrentMemory.Graphics.Add(GraphicName, new VortexLibrary.NetBitmap(Filename, AutoRotate));
                return true;
            }
            else if (CurrentMemory.Graphics.ContainsKey(GraphicName) == true && CanOverwriteExistingGraphic == true)
            {
                CurrentMemory.Graphics[GraphicName].Dispose();
                CurrentMemory.Graphics.Remove(GraphicName);
                CurrentMemory.Graphics.Add(GraphicName, new VortexLibrary.NetBitmap(Filename));
                return true;
            }
            return false;
        }

        public bool AddGraphic(string GraphicName, byte[] Filedata, Vortex_Memory CurrentMemory, Token command, bool CanOverwriteExistingGraphic)
        {
            if (CurrentMemory.NodeGroups.ContainsKey(GraphicName) == true)
                CurrentMemory.Error.Add(command, "Name " + GraphicName + " already used in NodeGroups");
            else if (CurrentMemory.VarArrays.ContainsKey(GraphicName))
                CurrentMemory.Error.Add(command, "Name " + GraphicName + " already used in VarArrays");
            else if (CurrentMemory.Variables.ContainsKey(GraphicName) == true)
                CurrentMemory.Error.Add(command, "Name " + GraphicName + " already used in Variables");
            else if (CurrentMemory.Graphics.ContainsKey(GraphicName) == false)
            {
                using (var ms = new System.IO.MemoryStream(Filedata))
                {
                    CurrentMemory.Graphics.Add(GraphicName, new VortexLibrary.NetBitmap(new Bitmap(ms)));
                }
                return true;
            }
            else if (CurrentMemory.Graphics.ContainsKey(GraphicName) == true && CanOverwriteExistingGraphic == true)
            {
                CurrentMemory.Graphics[GraphicName].Dispose();
                CurrentMemory.Graphics.Remove(GraphicName);
                using (var ms = new System.IO.MemoryStream(Filedata))
                {
                    CurrentMemory.Graphics.Add(GraphicName, new VortexLibrary.NetBitmap(new Bitmap(ms)));
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// This runs when adding a variable, to make sure that there isn't an object with the same name that could confuse a script
        /// </summary>
        /// <param name="GraphicName">The name you want to use</param>
        /// <param name="Filename">The filename from the graphic</param>
        /// <param name="CurrentMemory">The memory object to check</param>
        /// <param name="command">Which command this came from</param>
        /// <param name="CanOverwriteNonLockedExistingVariable">allow an existing variable with the same name to be overwritten, if it isn't locked</param>
        /// <param name="WhereFrom">Shows where the Variable was taken from originally - can be used for security to not allow URL (Querystring) variables, for example</param>
        /// <returns>True if variable written to memory, false if not. Reason given in error log.</returns>
        public bool AddGraphic(string GraphicName, System.Drawing.Bitmap Graphic, Vortex_Memory CurrentMemory, Token command, bool CanOverwriteExistingGraphic)
        {
            if (CurrentMemory.NodeGroups.ContainsKey(GraphicName) == true)
                CurrentMemory.Error.Add(command, "Name " + GraphicName + " already used in NodeGroups");
            else if (CurrentMemory.VarArrays.ContainsKey(GraphicName))
                CurrentMemory.Error.Add(command, "Name " + GraphicName + " already used in VarArrays");
            else if (CurrentMemory.Variables.ContainsKey(GraphicName) == true)
                CurrentMemory.Error.Add(command, "Name " + GraphicName + " already used in Variables");
            else if (CurrentMemory.Graphics.ContainsKey(GraphicName) == false)
            {
                CurrentMemory.Graphics.Add(GraphicName, new VortexLibrary.NetBitmap(Graphic));
                return true;
            }
            else if (CurrentMemory.Graphics.ContainsKey(GraphicName) == true && CanOverwriteExistingGraphic == true)
            {
                CurrentMemory.Graphics[GraphicName] = new VortexLibrary.NetBitmap(Graphic);
                return true;
            }
            return false;
        }
        /// <summary>
        /// This runs when adding a variable, to make sure that there isn't an object with the same name that could confuse a script
        /// </summary>
        /// <param name="VariableName">The name you want to use</param>
        /// <param name="Value">The value for the variable</param>
        /// <param name="CurrentMemory">The memory object to check</param>
        /// <param name="command">Which command this came from</param>
        /// <param name="CanOverwriteNonLockedExistingVariable">allow an existing variable with the same name to be overwritten, if it isn't locked</param>
        /// <param name="WhereFrom">Shows where the Variable was taken from originally - can be used for security to not allow URL (Querystring) variables, for example</param>
        /// <returns>True if variable written to memory, false if not. Reason given in error log.</returns>
        public bool AddVariable(string VariableName, string Value, Vortex_Memory CurrentMemory, Token command, bool CanOverwriteNonLockedExistingVariable, VariableFrom WhereFrom)
        {
            if (CurrentMemory.NodeGroups.ContainsKey(VariableName) == true)
                CurrentMemory.Error.Add(command, "Name " + VariableName + " already used in NodeGroups");
            else if (CurrentMemory.VarArrays.ContainsKey(VariableName))
                CurrentMemory.Error.Add(command, "Name " + VariableName + " already used in VarArrays");
            else if (CurrentMemory.Graphics.ContainsKey(VariableName) == true)
                CurrentMemory.Error.Add(command, "Name " + VariableName + " already used in Graphics");
            else if (CurrentMemory.Variables.ContainsKey(VariableName) == false)
            {
                CurrentMemory.Variables.Add(VariableName, new Variable(Value, CheckTokenValues(Value), WhereFrom));
                return true;
            }
            else if((CanOverwriteNonLockedExistingVariable == true && CurrentMemory.Variables[VariableName].Locked == false))
            {
                CurrentMemory.Variables[VariableName] = new Variable(Value, CheckTokenValues(Value), WhereFrom);
                return true;
            }
            return false;
        }


        /// <summary>
        /// This runs when adding a NodeGroup, to make sure that there isn't an object with the same name that could confuse a script
        /// </summary>
        /// <param name="GroupName">The name you want to use</param>
        /// <param name="Value">The List of Nodes for the NodeGroup</param>
        /// <param name="CurrentMemory">The memory object to check</param>
        /// <param name="command">Which command this came from</param>
        /// <param name="CanOverwriteExistingNodeGroup">allow an existing NodeGroup with the same name to be overwritten</param>
        /// <returns>True if NodeGroup written to memory, false if not. Reason given in error log.</returns>
        public bool AddNodeGroup(string GroupName, List<Node> Value, Vortex_Memory CurrentMemory, Token command, bool CanOverwriteExistingNodeGroup)
        {
            if (CurrentMemory.Variables.ContainsKey(GroupName) == true)
                CurrentMemory.Error.Add(command, "Name " + GroupName + " already used in Variables");
            else if (CurrentMemory.VarArrays.ContainsKey(GroupName))
                CurrentMemory.Error.Add(command, "Name " + GroupName + " already used in VarArrays");
            else if (CurrentMemory.Graphics.ContainsKey(GroupName) == true)
                CurrentMemory.Error.Add(command, "Name " + GroupName + " already used in Graphics");
            else if (CurrentMemory.NodeGroups.ContainsKey(GroupName) == false)
            {
                CurrentMemory.NodeGroups.Add(GroupName, Value);
                return true;
            }
            else if (CanOverwriteExistingNodeGroup == true)
            {
                CurrentMemory.NodeGroups[GroupName] =  Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// This runs when adding a VarArray, to make sure that there isn't an object with the same name that could confuse a script
        /// </summary>
        /// <param name="ArrayName">The name you want to use</param>
        /// <param name="Value">The values for the Array</param>
        /// <param name="CurrentMemory">The memory object to check</param>
        /// <param name="command">Which command this came from</param>
        /// <param name="CanOverwriteExistingArray">allow an existing Array with the same name to be overwritten</param>
        /// <returns>True if VarArray written to memory, false if not. Reason given in error log.</returns>
        public bool AddVarArray(string ArrayName, Token Value, Vortex_Memory CurrentMemory, Token command, bool CanOverwriteExistingArray)
        {
            if (CurrentMemory.Variables.ContainsKey(ArrayName) == true)
                CurrentMemory.Error.Add(command, "Name " + ArrayName + " already used in Variables");
            else if (CurrentMemory.NodeGroups.ContainsKey(ArrayName))
                CurrentMemory.Error.Add(command, "Name " + ArrayName + " already used in NodeGroups");
            else if (CurrentMemory.Graphics.ContainsKey(ArrayName) == true)
                CurrentMemory.Error.Add(command, "Name " + ArrayName + " already used in Graphics");
            else if (CurrentMemory.VarArrays.ContainsKey(ArrayName) == false)
            {
                CurrentMemory.VarArrays.Add(ArrayName, Value);
                return true;
            }
            else if (CanOverwriteExistingArray == true)
            {
                CurrentMemory.VarArrays[ArrayName] = Value;
                return true;
            }
            return false;
        }

        #endregion

        #region DataConversions

        public Token ConvertScriptToToken(List<Token> TokenisedScript)
        {
            Token X = new Token();
            X.TokenSubData = TokenisedScript;
            return X;
        }

        public List<Token> ConvertScriptToToken(Token TokenisedScript)
        {
            return TokenisedScript.TokenSubData;
        }

        public bool ConvertToBoolean(string Command, ref Vortex_Memory CurrentMemory, string value)
        {
            if (value.ToLower() == "y" || value == "1" || value == "true")
                return true;
            else if (value.ToLower() == "n" || value == "0" || value == "false")
                return false;
            else
            {
                try
                {
                    return Convert.ToBoolean(value);
                }
                catch
                {
                    CurrentMemory.Error.Add(Command + " - Error in converting value " + value + " to Boolean.");
                    return false;
                }
            }
        }

        public Guid ConvertToGuid(string Command, ref Vortex_Memory CurrentMemory, string value)
        {
            if(Guid.TryParse(value, out Guid result) == false)
            {
                CurrentMemory.Error.Add(Command + " - Error in converting value " + value + " to Guid.");
            }
            return result;
        }

        public DateTime ConvertToDateTime(string Command, ref Vortex_Memory CurrentMemory, string value)
        {
            if (DateTime.TryParse(value, out DateTime result) == false)
            {
                CurrentMemory.Error.Add(Command + " - Error in converting value " + value + " to DateTime.");
            }
            return result;
        }

        public Decimal ConvertToDecimal(string Command, ref Vortex_Memory CurrentMemory, string value)
        {
            if (Decimal.TryParse(value, out Decimal result) == false)
            {
                CurrentMemory.Error.Add(Command + " - Error in converting value " + value + " to Decimal.");
            }
            return result;
        }

        public double ConvertToDouble(string Command, ref Vortex_Memory CurrentMemory, string value)
        {
            if (double.TryParse(value, out double result) == false)
            {
                CurrentMemory.Error.Add(Command + " - Error in converting value " + value + " to double.");
            }
            return result;
        }

        public int ConvertToInteger(string Command, ref Vortex_Memory CurrentMemory, string value)
        {
            if (Int32.TryParse(value, out int result) == false)
            {
                CurrentMemory.Error.Add(Command + " - Error in converting value " + value + " to Integer.");
            }
            return result;
        }

        public Int64 ConvertToInt64(string Command, ref Vortex_Memory CurrentMemory, ref Node_AccessLayer NodeCache, string value)
        {
            if (Int64.TryParse(value, out Int64 result) == false)
            {
                CurrentMemory.Error.Add(Command + " - Error in converting value " + value + " to Int 64.");
            }
            return result;
        }

        public NodeValue ConvertToNodeValue(string Command, ref Vortex_Memory CurrentMemory, string value)
        {
            if (Decimal.TryParse(value, out Decimal result) == false)
            {
                CurrentMemory.Error.Add(Command + " - Error in converting value " + value + " to Decimal.");
            }
            return new NodeValue(result);
        }

        public Field ConvertToNodeField(string Command, ref Vortex_Memory CurrentMemory, string value)
        {
            if (Enum.TryParse<Field>(value, out Field result) == false)
            {
                CurrentMemory.Error.Add(Command + " - Error in converting value " + value + " to a Node Field.");
            }
            return result;
        }

        public LinkField ConvertToLinkField(string Command, ref Vortex_Memory CurrentMemory, string value)
        {
            if (Enum.TryParse<LinkField>(value, out LinkField result) == false)
            {
                CurrentMemory.Error.Add(Command + " - Error in converting value " + value + " to a Node Link Field.");
            }
            return result;
        }

        public Logic ConvertToLogic(string Command, ref Vortex_Memory CurrentMemory, string value)
        {
            if (value.ToLower() == "and")
                return Logic.And;
            else if (value.ToLower() == "or")
                return Logic.Or;
            else if (value.ToLower() == "not")
                return Logic.Not;
            else
            {
                if (Enum.TryParse<Logic>(value, out Logic result) == false)
                {
                    CurrentMemory.Error.Add(Command + " - Error in converting value " + value + " to a Logical connector.");
                }
                return result;
            }
        }

        public Operator ConvertToOperator(string Command, ref Vortex_Memory CurrentMemory, string value)
        {
            if (Enum.TryParse<Operator>(value, out Operator result) == false)
            {
                CurrentMemory.Error.Add(Command + " - Error in converting value " + value + " to an operator.");
            }
            return result;
        }

        public object ConvertToCorrectFormat(Field item, string Command, ref Vortex_Memory CurrentMemory, string value)
        {
            switch(item)
            {
                case Field.Active:
                    return ConvertToBoolean(Command, ref CurrentMemory, value);
                case Field.ApplicationID:
                    return ConvertToGuid(Command, ref CurrentMemory, value);
                case Field.BooleanItem:
                    return ConvertToBoolean(Command, ref CurrentMemory, value);
                case Field.Confirmed:
                    return ConvertToBoolean(Command, ref CurrentMemory, value);
                case Field.Content:
                    return value;
                case Field.Created:
                    return ConvertToDateTime(Command, ref CurrentMemory, value);
                case Field.Date1:
                    return ConvertToDateTime(Command, ref CurrentMemory, value);
                case Field.Date2:
                    return ConvertToDateTime(Command, ref CurrentMemory, value);
                case Field.Date3:
                    return ConvertToDateTime(Command, ref CurrentMemory, value);
                case Field.ExtraInfo:
                    return value;
                case Field.Item1ID:
                    return ConvertToGuid(Command, ref CurrentMemory, value);
                case Field.Item2ID:
                    return ConvertToGuid(Command, ref CurrentMemory, value);
                case Field.ItemID:
                    return ConvertToGuid(Command, ref CurrentMemory, value);
                case Field.LastUpdated:
                    return ConvertToDateTime(Command, ref CurrentMemory, value);
                case Field.LinkCreated:
                    return ConvertToDateTime(Command, ref CurrentMemory, value);
                case Field.LinkLastUpdated:
                    return ConvertToDateTime(Command, ref CurrentMemory, value);
                case Field.LinkTypeID:
                    return ConvertToGuid(Command, ref CurrentMemory, value);
                case Field.PathID:
                    return ConvertToGuid(Command, ref CurrentMemory, value);
                case Field.SubContent:
                    return value;
                case Field.Title:
                    return value;
                case Field.TypeID:
                    return ConvertToGuid(Command, ref CurrentMemory, value);
                case Field.UserID:
                    return ConvertToGuid(Command, ref CurrentMemory, value);
                case Field.Value1:
                    return ConvertToNodeValue(Command, ref CurrentMemory, value);
                case Field.Value2:
                    return ConvertToNodeValue(Command, ref CurrentMemory, value);
                case Field.Value3:
                    return ConvertToNodeValue(Command, ref CurrentMemory, value);
                case Field.ValueInfo:
                    return ConvertToNodeValue(Command, ref CurrentMemory, value);
                default:
                    return value;
            }
        }
        #endregion
    }
}
