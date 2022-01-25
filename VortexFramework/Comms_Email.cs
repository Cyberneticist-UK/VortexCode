using System;
using System.Net.Mail;

namespace Vortex
{
    /// <summary>
    /// This is my crude email system - you set up the email in memory, each item as required, then send.
    /// If the email sends without an issue, the email is then reset ready for the next message.
    /// </summary>
    public class Comms_Email
    {
        public SmtpClient client;
        MailAddress From;
        MailAddress ReplyTo;
        MailMessage mail = new MailMessage();
        public string ErrorMessage = "";
        bool[] ItemsSet = new bool[4] { false, false, false, false };
        enum ItemsSetting { From, Address, Subject, Message };
        enum AddressGroup { From, ReplyTo, To, CC, BCC };

        /// <summary>
        /// Create the email server - you must supply a secure connection
        /// </summary>
        /// <param name="SMTP">SMTP server to use</param>
        /// <param name="username">Username for the server</param>
        /// <param name="password">Password for the server</param>
        public Comms_Email(string SMTP, string username, string password)
        {
            client = new SmtpClient(SMTP);
            client.Credentials = new System.Net.NetworkCredential(username, password);
        }

        /// <summary>
        /// Create the email server - you must supply a secure connection
        /// </summary>
        /// <param name="SMTP">SMTP Server to use</param>
        public Comms_Email(SmtpClient SMTP)
        {
            client = SMTP;
        }

        /// <summary>
        /// Reset this object back to be ready for a new email - Note that the "From" and "Reply To" addresses
        /// Are kept if present!
        /// </summary>
        public void ResetMail()
        {
            mail = new MailMessage();
            if (From != null)
                mail.From = From;
            mail.ReplyToList.Clear();
            if(ReplyTo != null)
                mail.ReplyToList.Add(ReplyTo);
            mail.Priority = MailPriority.Normal;
            ItemsSet[(int)ItemsSetting.Address] = false;
            ItemsSet[(int)ItemsSetting.Subject] = false;
            ItemsSet[(int)ItemsSetting.Message] = false;
        }

        public void SetPriority(string Priority)
        {
            try
            {
                mail.Priority = (MailPriority)Enum.Parse(typeof(MailPriority), Priority);
            }
            catch
            {
                mail.Priority = MailPriority.Normal;
            }
        }

        /// <summary>
        /// Used to create an email address object, and returns null if there is an error (Error message set)
        /// </summary>
        /// <param name="MailAddress">The properly qualified email address</param>
        /// <param name="MailName">The name of the person to display</param>
        /// <returns>Null if error, or the address if okay</returns>
        private MailAddress CreateMailAddress(string MailAddress, string MailName)
        {
            try
            {
                return new MailAddress(MailAddress, MailName);
            }
            catch (Exception err)
            {
                ErrorMessage = err.Message;
                return null;
            }
        }

        /// <summary>
        /// This is the generic setting of the address command - used by all the others
        /// </summary>
        /// <param name="AddressAndName">The mail address created</param>
        /// <param name="Group">Where this address goes!</param>
        /// <returns>bool if worked, false if not</returns>
        private bool SetAddress(MailAddress AddressAndName, AddressGroup Group)
        {
            try
            {
                switch (Group)
                {
                    case AddressGroup.From:
                        if (AddressAndName == null)
                            ItemsSet[(int)ItemsSetting.From] = false;
                        else
                            ItemsSet[(int)ItemsSetting.From] = true;
                        From = AddressAndName;
                        mail.From = From;
                        return ItemsSet[(int)ItemsSetting.From];
                    case AddressGroup.ReplyTo:
                        ReplyTo = AddressAndName;
                        mail.ReplyToList.Clear();
                        mail.ReplyToList.Add(ReplyTo);
                        return (AddressAndName == null);
                    case AddressGroup.To:
                        if (AddressAndName == null)
                            ItemsSet[(int)ItemsSetting.Address] = false;
                        else
                        {
                            ItemsSet[(int)ItemsSetting.Address] = true;
                            mail.To.Add(AddressAndName);
                        }
                        return (AddressAndName == null);
                    case AddressGroup.CC:
                        if (AddressAndName == null)
                            ItemsSet[(int)ItemsSetting.Address] = false;
                        else
                        {
                            ItemsSet[(int)ItemsSetting.Address] = true;
                            mail.Bcc.Add(AddressAndName);
                        }
                        return (AddressAndName == null);
                    case AddressGroup.BCC:
                        if (AddressAndName == null)
                            ItemsSet[(int)ItemsSetting.Address] = false;
                        else
                        {
                            ItemsSet[(int)ItemsSetting.Address] = true;
                            mail.Bcc.Add(AddressAndName);
                        }
                        return (AddressAndName == null);
                }
                return false;
            }
            catch(Exception err)
            {
                ErrorMessage = err.Message;
                return false;
            }
        }

        /// <summary>
        /// Sets the "From" address for the email
        /// </summary>
        /// <param name="FromAddress">The properly qualified email address</param>
        /// <param name="FromName">The name of the person to display</param>
        /// <returns>True if successful, false if not</returns>
        private bool SetAddress(string Address, string Name, AddressGroup Group)
        {
            MailAddress temp = CreateMailAddress(Address, Name);
            return SetAddress(temp, Group);
        }

        /// <summary>
        /// Sets the "From" address for the email
        /// </summary>
        /// <param name="AddressAndName">2 field array of Email, Name</param>
        /// <returns>True if successful, false if not</returns>
        private bool SetAddress(string[] AddressAndName, AddressGroup Group)
        {
            if (AddressAndName.Length == 2)
                return SetAddress(AddressAndName[0], AddressAndName[1], Group);
            else
            {
                ErrorMessage = "Array for an address should be Email, Name (2 parameters)";
                return false;
            }
        }

        /// <summary>
        /// Adds an attachment to the current mail object:
        /// </summary>
        /// <param name="Filename">The file to attach</param>
        public bool AddAttachment(string Filename)
        {
            if (mail != null)
            {
                mail.Attachments.Add(new Attachment(Filename));
                return true;
            }
            ErrorMessage = "File could not be attached - Mail object not created!";
            return false;
        }


        #region From Address

        /// <summary>
        /// Sets the "From" address for the email
        /// </summary>
        /// <param name="FromAddress">The properly qualified email address</param>
        /// <param name="FromName">The name of the person to display</param>
        /// <returns>True if successful, false if not</returns>
        public bool SetFromAddress(string FromAddress, string FromName)
        {
            return SetAddress(FromAddress, FromName, AddressGroup.From);
        }

        /// <summary>
        /// Sets the "From" address for the email
        /// </summary>
        /// <param name="AddressAndName">2 field array of Email, Name</param>
        /// <returns>True if successful, false if not</returns>
        public bool SetFromAddress(string[] AddressAndName)
        {
            return SetAddress(AddressAndName, AddressGroup.From);
        }

        /// <summary>
        /// Sets the "From" address for the email
        /// </summary>
        /// <param name="AddressAndName">The Mail Address</param>
        /// <returns>True if successful, false if not</returns>
        public bool SetFromAddress(MailAddress AddressAndName)
        {
            return SetAddress(AddressAndName, AddressGroup.From);
        }

        #endregion

        #region Reply To Address

        /// <summary>
        /// Sets the "Reply To" address for the email
        /// </summary>
        /// <param name="FromAddress">Email</param>
        /// <param name="FromName">Name</param>
        /// <returns>True if successful, false if not</returns>
        public bool SetReplyToAddress(string FromAddress, string FromName)
        {
            return SetAddress(FromAddress, FromName, AddressGroup.ReplyTo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AddressAndName"></param>
        /// <returns>True if successful, false if not</returns>
        public bool SetReplyToAddress(string[] AddressAndName)
        {
            return SetAddress(AddressAndName, AddressGroup.ReplyTo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AddressAndName"></param>
        /// <returns>True if successful, false if not</returns>
        public bool SetReplyToAddress(MailAddress AddressAndName)
        {
            return SetAddress(AddressAndName, AddressGroup.From);
        }

        #endregion

        public bool SetAddressTo(string ToAddress, string ToName)
        {
            mail.To.Clear();
            return SetAddress(ToAddress, ToName, AddressGroup.To);
        }

        public bool SetAddressTo(string[] AddressAndName)
        {
            mail.To.Clear();
            return SetAddress(AddressAndName, AddressGroup.To);
        }

        public bool SetAddressTo(MailAddress AddressAndName)
        {
            mail.To.Clear();
            return SetAddress(AddressAndName, AddressGroup.To);
        }

        public bool AddAddressTo(string ToAddress, string ToName)
        {
            return SetAddress(ToAddress, ToName, AddressGroup.To);
        }

        public bool AddAddressTo(string[] AddressAndName)
        {
            return SetAddress(AddressAndName, AddressGroup.To);
        }

        public bool AddAddressTo(MailAddress AddressAndName)
        {
            return SetAddress(AddressAndName, AddressGroup.To);
        }

        public bool SetAddressCC(string ToAddress, string ToName)
        {
            mail.CC.Clear();
            return SetAddress(ToAddress, ToName, AddressGroup.CC);
        }

        public bool SetAddressCC(string[] AddressAndName)
        {
            mail.CC.Clear();
            return SetAddress(AddressAndName, AddressGroup.CC);
        }

        public bool SetAddressCC(MailAddress AddressAndName)
        {
            mail.CC.Clear();
            return SetAddress(AddressAndName, AddressGroup.CC);
        }

        public bool AddAddressCC(string ToAddress, string ToName)
        {
            return SetAddress(ToAddress, ToName, AddressGroup.CC);
        }

        public bool AddAddressCC(string[] AddressAndName)
        {
            return SetAddress(AddressAndName, AddressGroup.CC);
        }

        public bool AddAddressCC(MailAddress AddressAndName)
        {
            return SetAddress(AddressAndName, AddressGroup.CC);
        }

        public bool SetAddressBCC(string ToAddress, string ToName)
        {
            mail.Bcc.Clear();
            return SetAddress(ToAddress, ToName, AddressGroup.BCC);
        }

        public bool SetAddressBCC(string[] AddressAndName)
        {
            mail.Bcc.Clear();
            return SetAddress(AddressAndName, AddressGroup.BCC);
        }

        public bool SetAddressBCC(MailAddress AddressAndName)
        {
            mail.Bcc.Clear();
            return SetAddress(AddressAndName, AddressGroup.BCC);
        }

        public bool AddAddressBCC(string ToAddress, string ToName)
        {
            return SetAddress(ToAddress, ToName, AddressGroup.BCC);
        }

        public bool AddAddressBCC(string[] AddressAndName)
        {
            return SetAddress(AddressAndName, AddressGroup.BCC);
        }

        public bool AddAddressBCC(MailAddress AddressAndName)
        {
            return SetAddress(AddressAndName, AddressGroup.BCC);
        }

        public bool SetSubject(string Subject)
        {
            mail.Subject = Subject;
            ItemsSet[(int)ItemsSetting.Subject] = true;
            return true;
        }

        public bool SetBody(string Body, bool HTML)
        {
            ItemsSet[(int)ItemsSetting.Message] = true;
            mail.IsBodyHtml = HTML;
            mail.Body = Body;
            return true;
        }

        public bool SendMail()
        {
            if (ItemsSet[(int)ItemsSetting.From] == false)
            {
                ErrorMessage = "You need to set the From Address";
                return false;
            }
            else if (ItemsSet[(int)ItemsSetting.Address] == false)
            {
                ErrorMessage = "You need to set the To/CC/BCC Address";
                return false;
            }
            else if (ItemsSet[(int)ItemsSetting.Subject] == false)
            {
                ErrorMessage = "You need to set the Subject of the Email";
                return false;
            }
            else if (ItemsSet[(int)ItemsSetting.Message] == false)
            {
                ErrorMessage = "You need to set the Message of the Email";
                return false;
            }
            else
            {
                try
                {
                    client.Send(mail);
                    ResetMail();
                    return true;
                }
                catch(Exception err)
                {
                    ErrorMessage = err.Message;
                    return false;
                }
            }
            
        }

    }

}
