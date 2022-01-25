using System;

namespace Vortex
{
    public enum VariableFrom { URL, Form, Script, NotSet, System, Ajax }

    public class Variable
    {
        public string Data
        {
            get
            {
                return _Data;
            }
            set
            {
                if (Locked == false)
                    _Data = value;
            }
        }

        string _Data;
        public NodeValue TypeInfo;
        public bool Locked;
        public string Password;
        public VariableFrom WhereFrom = VariableFrom.NotSet;
        public Variable()
        {

        }
        public Variable(string Data, NodeValue TypeInfo, VariableFrom WhereFrom)
        {
            this._Data = Data;
            this.TypeInfo = TypeInfo;
            this.Locked = false;
            this.Password = "";
            this.WhereFrom = WhereFrom;
        }

        // Build the variable with the right values:
        public Variable(string Data, ref VortexTools Tools)
        {
            this._Data = Data;
            this.TypeInfo = Tools.CheckTokenValues(this._Data);
            this.Locked = false;
            this.Password = "";
        }

        public void Lock(string Password)
        {
            if (Locked == false)
            {
                this.Password = Password;
                this.Locked = true;
            }
        }

        public void Unlock(string Password)
        {
            if (Locked == true && this.Password == Password)
            {
                this.Password = "";
                this.Locked = false;
            }
        }

        
        /// <summary>
        /// Allows you to say that this token can be seen as a particular type
        /// </summary>
        /// <param name="info">What Type to set it to</param>
        public void SetType(TokenInfo info)
        {
            if (Locked == false)
            {
                if (TypeInfo == null)
                    TypeInfo = new NodeValue(0);
                TypeInfo.SetSwitch((int)info, true);
            }
        }

        /// <summary>
        /// Sets a type to remove from this token
        /// </summary>
        /// <param name="info">What type to clear and set to "not being of this type"</param>
        public void ClearType(TokenInfo info)
        {
            if (Locked == false)
            {
                TypeInfo.SetSwitch((int)info, false);
            }
        }

        /// <summary>
        /// Used in reset code, basically say that this data is none of the types
        /// </summary>
        public void ClearAllTypes()
        {
            if (Locked == false)
            {
                TypeInfo = 0;
            }
        }

        /// <summary>
        /// Check to see if this token matches a particular type. A token can be multiple types at the same time
        /// </summary>
        /// <param name="info">Which type you wish to check</param>
        /// <returns>True if it can be this type, false if not</returns>
        public bool IsOfType(TokenInfo info)
        {
            return TypeInfo.CheckSwitch((int)info);
        }


        public static implicit operator string(Variable Data)
        {
            return Data.ToString();
        }


        public static implicit operator decimal(Variable Data)
        {
            if (Data.IsOfType(TokenInfo.Decimal))
                return Convert.ToDecimal(Data.ToString());
            else
                return -1;
        }

        public static implicit operator int(Variable Data)
        {
            if (Data.IsOfType(TokenInfo.Integer))
                return Convert.ToInt32(Data.ToString());
            else
                return -1;
        }

        public static implicit operator Int64(Variable Data)
        {
            if (Data.IsOfType(TokenInfo.Integer))
                return Convert.ToInt64(Data.ToString());
            else
                return -1;
        }

        public static implicit operator double(Variable Data)
        {
            if (Data.IsOfType(TokenInfo.Decimal))
                return Convert.ToDouble(Data.ToString());
            else
                return -1;
        }

        public static implicit operator Variable(decimal v)
        {
            NodeValue A = new NodeValue(0);
            A.SetSwitch((int)TokenInfo.Decimal, true);
            return new Variable(v.ToString(), A, VariableFrom.Script);
        }


        public override string ToString()
        {
            return Data;
        }

        public int ToInt()
        {
            //if (IsOfType(TokenInfo.Integer))
            //{
                return Convert.ToInt32(Data);
            //}
            //return -1;
        }

        public decimal ToDecimal()
        {
            //if (IsOfType(TokenInfo.Numeric))
            //{
                return Convert.ToDecimal(Data);
            //}
            //return -1;
        }
    }
}
