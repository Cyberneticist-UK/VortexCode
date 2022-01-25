using System;
using System.Collections.Generic;
using System.IO;

namespace Vortex
{
    public struct Struct_Netelligence
    {
        public Guid UserID;
        public Guid MemberID;
        public string Username;
        public string FullName;
        public Node UserNode;
        public Node MemberNode;
        public List<Struct_Group> Groups;
        public List<string> MyMachines;

        public void AddUser(Node Login)
        {
            UserID = Login.UserID;
            Username = Login.Title;
            UserNode = Login;
            File.Create(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\NetLocal\ClientData\" + UserID.ToString() + ".key");
        }

        public void AddMember(Node Member)
        {
            MemberID = Member.UserID;
            FullName = Member.Title+" "+Member.Content;
            MemberNode = Member;
        }
    }
}
