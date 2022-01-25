using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Vortex.OrganiseEngine
{
    public class Engine
    {
        public List<DirectoryNode> Folders = new List<DirectoryNode>();
        public FileList Files = new FileList();
        public long NumberDirectories = 0;
        public long NumberFiles = 0;
        long DirectoriesScanned = 0;
        public int Percent {get { if (NumberDirectories == 0 || DirectoriesScanned == 0) return 0; else return Convert.ToInt32(Convert.ToDouble(DirectoriesScanned) / Convert.ToDouble(NumberDirectories) * 100); } }
        public string UpdateDeleted = "";
        public string UpdateAdded = "";
        public void catalogueDrive(string Drive)
        {
            DirectoryNode thisDir = new DirectoryNode(Drive);
            Folders.Add(thisDir);
            try
            {
                CatalogueDirectory(thisDir, "", Drive);
            }
            catch
            {

            }
        }

        public void catalogueFiles()
        {
            NumberFiles = 0;
            Files.Files.Clear();
            foreach(DirectoryNode Node in Folders)
            {
                CatalogueFiles(Node, "");
            }
        }

        public void SaveCurrent(string Folder)
        {
            foreach (DirectoryNode item in Folders)
            {
                SerializeObject<DirectoryNode>(item, Folder + item.Name.Replace("\\", "").Replace(":", "") + ".cat");
            }
        }

        public void SaveFiles(string Folder)
        {
            SerializeObject<FileList>(Files, Folder + "Data.files");
        }

        public void LoadFiles(string Folder)
        {
            Files.Files.Clear();
            Files = DeSerializeObject<FileList>(Folder + "Data.files");
            NumberFiles = Files.Files.Count();
        }

        public string TracePath(Guid DirectoryID)
        {
            return PathFinder("", Folders[0], DirectoryID);
        }

        public string PathFinder(string CurrentPath, DirectoryNode CurrentNode, Guid DirectoryID)
        {
            string NewPath = "";
            if(CurrentNode.NodeID == DirectoryID)
            {
                return CurrentPath+CurrentNode.Name;
            }
            if(CurrentPath == "")
                CurrentPath = CurrentNode.Name;
            foreach (DirectoryNode Node in CurrentNode.Children)
            {
                if (NewPath != "")
                    break;
                if (Node.NodeID == DirectoryID)
                    return CurrentPath + Node.Name + "\\";
                else
                    NewPath = PathFinder(CurrentPath + Node.Name + "\\", Node, DirectoryID);
            }
            return NewPath;
        }

        public void LoadCurrent(string Folder)
        {
            FileInfo[] Files = new DirectoryInfo(Folder).GetFiles("*.cat");
            Folders.Clear();
            foreach(FileInfo file in Files)
            {
                Folders.Add(DeSerializeObject<DirectoryNode>(file.FullName));
            }
            // Need to count how many nodes there are:-
            foreach (DirectoryNode item in Folders)
            {
                CountDirectories(item);
            }

        }

        public void UpdateCurrent()
        {
            NumberDirectories = 0;
            foreach (DirectoryNode current in Folders)
            {
                UpdateDirectory(current, "");
            }
            // Need to count how many nodes there are:-
            foreach (DirectoryNode item in Folders)
            {
                CountDirectories(item);
            }
        }

        protected void CountDirectories(DirectoryNode CurrentNode)
        {
            if (CurrentNode.Children != null)
            {
                foreach (DirectoryNode dir in CurrentNode.Children)
                {
                    CountDirectories(dir);
                }
                NumberDirectories += CurrentNode.Children.Count;
            }
        }


        protected void CatalogueDirectory(DirectoryNode CurrentNode, string current, string path)
        {
            DirectoryInfo[] SubDir = new DirectoryInfo(path + current).GetDirectories();
            if (current != "")
            {
                DirectoryNode thisDir = new DirectoryNode(current);
                CurrentNode.Children.Add(thisDir);
                NumberDirectories++;
                foreach (DirectoryInfo dir in SubDir)
                {
                    try
                    {
                        CatalogueDirectory(thisDir, dir.Name, path + current + "\\");
                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                foreach (DirectoryInfo dir in SubDir)
                {
                    try
                    {
                        CatalogueDirectory(CurrentNode, dir.Name, path + current);
                    }
                    catch
                    {

                    }
                }
            }
        }

        protected void CatalogueFiles(DirectoryNode CurrentNode, string path)
        {
            FileInfo[] SubFiles = null;

            try
            {
                if (path == "")
                {
                    SubFiles = new DirectoryInfo(CurrentNode.Name).GetFiles();
                    path = CurrentNode.Name;
                }
                else
                {
                    SubFiles = new DirectoryInfo(path).GetFiles();
                }
            }
            catch
            { 
            }
            if (SubFiles != null)
            {
                foreach (FileInfo file in SubFiles)
                {
                    Files.Add(new FileNode(file, CurrentNode.NodeID));
                    NumberFiles++;
                }  
            }
            DirectoriesScanned++;
            foreach (DirectoryNode node in CurrentNode.Children)
            {
                CatalogueFiles(node, path + node.Name + "\\");
            }
        }

        protected void UpdateDirectory(DirectoryNode CurrentNode, string path)
        {
            // To update a directory, you take all the children of the current node and see if it matches the path!
            NumberDirectories++;
            string AllDir = "";
            DirectoryInfo[] SubDir = null;
            try
            {
                SubDir = new DirectoryInfo(path + CurrentNode.Name).GetDirectories();
            }
            catch
            {

            }
            // Get a list of the current sub directories:
            if (SubDir != null)
            {
                foreach (DirectoryInfo dir in SubDir)
                {
                    AllDir += "~" + dir.Name;
                }
                // Now go through the children and see if they match:
                for (int i = 0; i < CurrentNode.Children.Count(); i++)
                {
                    if (AllDir.IndexOf("~" + CurrentNode.Children[i].Name) == -1)
                    {
                        // Need to remove this child - directory no longer exists:-
                        UpdateDeleted += path + CurrentNode.Children[i].Name + "\r\n";
                        CurrentNode.Children.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        // Remove this directory from the list
                        AllDir.Replace("~" + CurrentNode.Children[i].Name, "");
                    }
                }
                if (AllDir.Length > 0)
                {
                    // Some new directories to add in:-
                    string[] dir = AllDir.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < dir.Length; i++)
                    {
                        try
                        {
                            UpdateAdded += path + CurrentNode.Name + "\\" + dir[i] + "\r\n";
                            CatalogueDirectory(CurrentNode, dir[i], path + CurrentNode.Name + "\\");
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="fileName"></param>
        public void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                }
            }
            catch //(Exception ex)
            {
                //Log exception here
            }
        }

        /// <summary>
        /// Deserializes an xml file into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                    }
                }
            }
            catch //(Exception ex)
            {
                //Log exception here
            }

            return objectOut;
        }
    }

    
}
