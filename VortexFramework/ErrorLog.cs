using System.Collections.Generic;


namespace Vortex
{
    public class ErrorLog: List<string>
    {
        /// <summary>
        /// Whenever anything is added to the error log, the error flag is set to true.
        /// </summary>
        public bool ErrorFlag = false;

        /// <summary>
        /// Add an item into the error log, automatically adding the date and time of the error
        /// </summary>
        /// <param name="item">The error message</param>
        public new void Add(string item)
        {
            base.Add(System.DateTime.Now.ToString("dd-MM-yy HH:mm:ss")+": "+item+"<br />");
            ErrorFlag = true;
        }

        /// <summary>
        /// Directly add in an error message to the error log (The date and time will not be stamped)
        /// </summary>
        /// <param name="item">The error message</param>
        public void AddDirect(string item)
        {
            base.Add(item);
            ErrorFlag = true;
        }

        /// <summary>
        /// At the moment this gets the command name and adds it to the error message, but in the future this could be stack trace etc.
        /// </summary>
        /// <param name="Command">The issue command</param>
        /// <param name="item">The error message</param>
        public void Add(Token Command, string item)
        {
            base.Add(System.DateTime.Now.ToString("dd-MM-yy HH:mm:ss") + ": <b>"+Command.data+"</b>:" + item + "<br />");
            ErrorFlag = true;
        }

        public new string ToString()
        {
            string result = "";
            for(int i=0; i < base.Count; i++)
            {
                result += base[i];// + "<br/>";
            }
            return result;
        }

        /// <summary>
        /// Append data from one error log to another - any errors in the new log already in this one are ignored.
        /// </summary>
        /// <param name="Errors">The error log to append from</param>
        public void Append(ErrorLog Errors)
        {
            // This only adds new items not already in the list:
            foreach (string item in Errors)
            {
                if(base.Contains(item) == false)
                    base.Add(item);
            }
        }
    }
}
