using System;
using System.IO;
using System.Collections.Generic;

namespace Vortex.OrganiseEngine
{
    [Serializable]
    public class FileList
    {
        public List<FileNode> Files = new List<FileNode>();
        public void Add(FileNode item)
        {
            Files.Add(item);
        }
    }

    [Serializable]
    public class FileNode
    {
        public Guid NodeID;
        public Guid DirectoryID;
        public string Name;
        public DateTime Created;
        public DateTime LastUpdated;
        public string Hash;
        public long FileSize;
        public FileNode(FileInfo File, Guid DirectoryID)
        {
            NodeID = Guid.NewGuid();
            this.Name = File.Name;
            this.Created = File.CreationTime;
            this.LastUpdated = File.LastWriteTime;
            this.FileSize = File.Length;
            this.DirectoryID = DirectoryID;
        }

        public FileNode()
        {

        }
    }
}
