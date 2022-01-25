using System;
using System.Collections.Generic;


namespace Vortex
{
    /// <summary>
    /// Tokens are the heart of NetScript - a Script File is chopped up into HTML and Command Tokens, then
    /// Each command is chopped up to the semantics of the statement to allow for error detection and prevention
    /// and Running on the Netelligence NetScript System!
    /// </summary>
    [Serializable]
    public class Token
    {
        public string data;
        public NodeValue TypeInfo;
        public List<Token> TokenSubData;
        public List<Token> TokenTestSubData;
        /// <summary>
        /// Create a Token of a specific type
        /// </summary>
        /// <param name="info">The Type (In Scripts, this is either HTML or Command)</param>
        /// <param name="data">The Data for the statement in full</param>
        public Token(TokenInfo info, string data)
        {
            this.data = data;
            TypeInfo = 0;
            TypeInfo.SetSwitch((int)TokenInfo.Text, true);
            TypeInfo.SetSwitch((int)info, true);
            TokenSubData = null;
            TokenTestSubData = null;
        }

        public Token()
        {
            this.data = "";
            TypeInfo = 0;
            TokenSubData = null;
            TokenTestSubData = null;
        }

        /// <summary>
        /// Hopefully creates a clone in separate memory of the token and it's structure!
        /// </summary>
        /// <returns></returns>
        public Token Clone()
        {
            Token temp = new Token();
            temp.data = this.data;
            temp.TypeInfo = this.TypeInfo;
            if(TokenSubData!= null)
            {
                temp.TokenSubData = new List<Token>();
                foreach(Token x in TokenSubData)
                {
                    temp.TokenSubData.Add(x.Clone());
                }
            }
            if(TokenTestSubData != null)
            {
                temp.TokenTestSubData = new List<Token>();
                foreach (Token x in TokenTestSubData)
                {
                    temp.TokenTestSubData.Add(x.Clone());
                }
            }
            return temp;
        }

        /// <summary>
        /// Allows you to say that this token can be seen as a particular type
        /// </summary>
        /// <param name="info">What Type to set it to</param>
        public void SetType(TokenInfo info)
        {
            TypeInfo.SetSwitch((int)info, true);
        }

        /// <summary>
        /// Sets a type to remove from this token
        /// </summary>
        /// <param name="info">What type to clear and set to "not being of this type"</param>
        public void ClearType(TokenInfo info)
        {
            TypeInfo.SetSwitch((int)info, false);
        }

        /// <summary>
        /// Used in reset code, basically say that this data is none of the types
        /// </summary>
        public void ClearAllTypes()
        {
            TypeInfo = 0;
        }

        /// <summary>
        /// Check to see if this token matches a particular type. A token can be multiple types at the same time
        /// </summary>
        /// <param name="info">Which type you wish to check</param>
        /// <returns>True if it can be this type, false if not</returns>
        public bool IsOfType(TokenInfo info)//, vList Variables = null)
        {
            if (info == TokenInfo.Variable)
            {
                //if (Variables != null)
                    // Need to check against variable list
                //    return Variables.CheckVariable(data);
                return false;
            }

            return TypeInfo.CheckSwitch((int)info);
        }

        public override string ToString()
        {
            string Result = data + " ";
            if (TokenSubData != null)
                foreach (Token item in TokenSubData)
                {
                    Result += item.data + " ";
                }
            return Result.Trim();
        }

        public int ToInt()
        {
            if (IsOfType(TokenInfo.Integer))
            {
                return Convert.ToInt32(data);
            }
            return -1;
        }
    }
}
