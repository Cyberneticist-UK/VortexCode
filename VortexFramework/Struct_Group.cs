using System;
using System.Collections.Generic;
using System.Text;

namespace Vortex
{
    public struct Struct_Group
    {
        public Node Group;
        public List<Node> Members;

        public Struct_Group(Node Group, List<Node> Members)
        {
            this.Group = Group;
            this.Members = Members;
        }

        public Struct_Group(string Code)
        {
            string[] Codebreak = Code.Split(new char[1] { '£' }, StringSplitOptions.RemoveEmptyEntries);
            if (Code != "" && Codebreak.Length > 0)
            {
                this.Group = Codebreak[0];
                Members = new List<Node>();
                for (int i = 1; i < Codebreak.Length; i++)
                {
                    if (Codebreak[i] != "")
                        Members.Add(Codebreak[i]);
                }

            }
            else
            {
                Group = null;
                Members = new List<Node>();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(Node m in Members)
            {
                sb.Append(m.ToString()+"£");
            }
            return Group.ToString() + "£" + sb.ToString();
        }

    }
    
}
