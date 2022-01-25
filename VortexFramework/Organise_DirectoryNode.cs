using System;
using System.Collections.Generic;

namespace Vortex.OrganiseEngine
{

    [Serializable]
    public class DirectoryNode
    {
        public Guid NodeID;
        public string Name;
        public List<DirectoryNode> Children;
        public DirectoryNode(string Name)
        {
            NodeID = Guid.NewGuid();
            this.Name = Name;
            Children = new List<DirectoryNode>();
        }

        public DirectoryNode()
        {

        }
    }
}
