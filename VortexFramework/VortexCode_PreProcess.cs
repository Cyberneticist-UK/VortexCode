using System;
using System.IO;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Vortex
{
    public static class Vortex_PreProcess
    {
        public static void PreProcessElements(Token item, Vortex_Memory CurrentMemory, Vortex.Node_AccessLayer NodeCache, NetScript_Command CommandProcessor)
        {
            if (item.TokenSubData != null)
            {
                for (int i = 0; i < item.TokenSubData.Count; i++)
                // Do we not need to check each sub item first for whether it is a function that needs to be run?
                {
                    if (item.TokenSubData[i].IsOfType(TokenInfo.Quote) == false && CommandProcessor.Functions.ContainsKey(item.TokenSubData[i].data) == true)
                    {
                        // Its a function - run it!
                        item.TokenSubData[i] = CommandProcessor.RunCommand(item.TokenSubData[i], CurrentMemory, NodeCache, false);
                        // Did the function return a value?
                        if (CurrentMemory.ReturnerValue != null)
                        {
                            Token temp = item.TokenSubData[i];
                            temp.data = CurrentMemory.ReturnerValue;
                            item.TokenSubData[i] = temp;
                        }
                        CurrentMemory.ReturnerValue = null;
                    }
                    if (item.TokenSubData[i].IsOfType(TokenInfo.Quote) == false && CurrentMemory.Variables.ContainsKey(item.TokenSubData[i].data) == true)
                    {
                        Token temp = item.TokenSubData[i];
                        temp.ClearAllTypes();
                        temp.SetType(TokenInfo.Variable);
                        temp.SetType(TokenInfo.Quote);
                        temp.data = CurrentMemory.Variables[item.TokenSubData[i].data].Data;
                        item.TokenSubData[i] = temp;
                    }
                    // New code - use VarArrays! As these are tokens, they slip straight in!
                    else if (item.TokenSubData[i].IsOfType(TokenInfo.Quote) == false && CurrentMemory.VarArrays.ContainsKey(item.TokenSubData[i].data) == true)
                    {
                        item.TokenSubData[i] = CurrentMemory.VarArrays[item.TokenSubData[i].data];
                    }
                    else if (item.TokenSubData[i].IsOfType(TokenInfo.Command) == true && item.TokenSubData[i].IsOfType(TokenInfo.Quote) == false && item.TokenSubData[i].IsOfType(TokenInfo.InBrackets) == false)
                        item.TokenSubData[i] = CommandProcessor.RunCommand(item.TokenSubData[i], CurrentMemory, NodeCache);
                }
            }

            if (item.TokenTestSubData != null)
            {
                for (int i = 0; i < item.TokenTestSubData.Count; i++)
                // Do we not need to check each sub item first for whether it is a function that needs to be run?
                {
                    //if(ProcessVariables && (CurrentMemory.Variables.ContainsKey(item.TokenTestSubData[i].data) == true))
                    //{
                    //    Token temp = item.TokenTestSubData[i];
                    //    temp.ClearAllTypes();
                    //    temp.SetType(TokenInfo.Variable);
                    //    temp.data = CurrentMemory.Variables[item.TokenTestSubData[i].data].Data;
                    //    item.TokenTestSubData[i] = temp;
                    //}
                    if (item.TokenTestSubData[i].TokenSubData != null)
                    {
                        for (int j = 0; j < item.TokenTestSubData[i].TokenSubData.Count; j++)
                        // Do we not need to check each sub item first for whether it is a function that needs to be run?
                        {
                            if (CurrentMemory.Variables.ContainsKey(item.TokenTestSubData[i].TokenSubData[j].data) == true)
                            {
                                Token temp = item.TokenTestSubData[i].TokenSubData[j];
                                temp.ClearAllTypes();
                                temp.SetType(TokenInfo.Variable);
                                temp.SetType(TokenInfo.Quote);
                                temp.data = CurrentMemory.Variables[item.TokenTestSubData[i].TokenSubData[j].data].Data;
                                item.TokenTestSubData[i].TokenSubData[j] = temp;
                            }
                            //else 
                            else if (item.TokenTestSubData[i].TokenSubData[j].IsOfType(TokenInfo.Command) == true && item.TokenTestSubData[i].TokenSubData[j].IsOfType(TokenInfo.Quote) == false && item.TokenTestSubData[i].TokenSubData[j].IsOfType(TokenInfo.InBrackets) == false)
                                item.TokenTestSubData[i].TokenSubData[j] = CommandProcessor.RunCommand(item.TokenTestSubData[i].TokenSubData[j], CurrentMemory, NodeCache);
                        }
                    }
                }
            }
        }

        public static void PreProcessElementsScriptLevel(Token item, Vortex_Memory CurrentMemory, Vortex.Node_AccessLayer NodeCache, NetScript_Command CommandProcessor)
        {
            if (item.TokenSubData != null)
            {
                for (int i = 0; i < item.TokenSubData.Count; i++)
                // Do we not need to check each sub item first for whether it is a function that needs to be run?
                {
                    if (item.TokenSubData[i].IsOfType(TokenInfo.Quote) == false && CurrentMemory.Variables.ContainsKey(item.TokenSubData[i].data) == true)
                    {
                        Token temp = item.TokenSubData[i];
                        temp.ClearAllTypes();
                        temp.SetType(TokenInfo.Variable);
                        temp.SetType(TokenInfo.Quote);
                        temp.data = CurrentMemory.Variables[item.TokenSubData[i].data].Data;
                        item.TokenSubData[i] = temp;
                    }
                    // New code - use VarArrays! As these are tokens, they slip straight in!
                    else if (item.TokenSubData[i].IsOfType(TokenInfo.Quote) == false && CurrentMemory.VarArrays.ContainsKey(item.TokenSubData[i].data) == true)
                    {
                        item.TokenSubData[i] = CurrentMemory.VarArrays[item.TokenSubData[i].data];
                    }
                    else if (item.TokenSubData[i].IsOfType(TokenInfo.Command) == true && item.TokenSubData[i].IsOfType(TokenInfo.Quote) == false && item.TokenSubData[i].IsOfType(TokenInfo.InBrackets) == false)
                        item.TokenSubData[i] = CommandProcessor.RunCommand(item.TokenSubData[i], CurrentMemory, NodeCache);
                }
            }

            if (item.TokenTestSubData != null)
            {
                for (int i = 0; i < item.TokenTestSubData.Count; i++)
                // Do we not need to check each sub item first for whether it is a function that needs to be run?
                {
                    if (item.TokenTestSubData[i].IsOfType(TokenInfo.Command) == true && item.TokenTestSubData[i].IsOfType(TokenInfo.Quote) == false && item.TokenTestSubData[i].IsOfType(TokenInfo.InBrackets) == false)
                        item.TokenTestSubData[i] = CommandProcessor.RunCommand(item.TokenTestSubData[i], CurrentMemory, NodeCache);
                }
            }
        }

        public static string SaveFile(string Filename, Vortex_Memory CurrentMemory)
        {
            CurrentMemory.FilesAccessed.Add(Filename);
            return Filename;
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="fileName"></param>
        public static void SerializeObject<T>(T serializableObject, string fileName)
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
            catch // (Exception ex)
            {
                //Log exception here
                // ex.Message;
            }
        }

        /// <summary>
        /// Deserializes an xml file into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static T DeSerializeObject<T>(string fileName)
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

        /// <summary>
        /// This checks to see where the file actually exists, and when it finds the file it sends back the full filename
        /// </summary>
        /// <param name="Filename">Filename to check</param>
        /// <param name="CurrentMemory">Current Memory</param>
        /// <returns>The full filename, or blank if file not found</returns>
        public static string PreProcessFile(string Filename, Vortex_Memory CurrentMemory)
        {
            if (File.Exists(Filename) == true)
                return SaveFile(new FileInfo(Filename).FullName, CurrentMemory);
            else if (File.Exists(CurrentMemory.ApplicationFolder + Filename) == true)
                return SaveFile(new FileInfo(CurrentMemory.ApplicationFolder + Filename).FullName, CurrentMemory);
            else if (File.Exists(CurrentMemory.ApplicationFolder + CurrentMemory.ContentFolder+ "\\" +Filename) == true)
                return SaveFile(new FileInfo(CurrentMemory.ApplicationFolder + CurrentMemory.ContentFolder + "\\" +Filename).FullName, CurrentMemory);
            else
            {
                try
                {
                    if (File.Exists(CurrentMemory.ContentFolder + "/" + Filename) == true)
                        return SaveFile(CurrentMemory.ContentFolder + "/" + Filename, CurrentMemory);
                    else
                    {
                        if (HttpContext.Current != null)
                        {
                            if (File.Exists(HttpContext.Current.Server.MapPath(CurrentMemory.ContentFolder + "/" + Filename)) == true)
                                return SaveFile(HttpContext.Current.Server.MapPath(CurrentMemory.ContentFolder + "/" + Filename), CurrentMemory);
                            else if (File.Exists(HttpContext.Current.Server.MapPath(Filename)) == true)
                                return SaveFile(HttpContext.Current.Server.MapPath(Filename), CurrentMemory);
                        }
                    }
                    CurrentMemory.Error.Add("Pre process file "+ Filename + " - File Not found");
                    return "";
                }
                catch
                {
                    CurrentMemory.Error.Add("Pre process file " + Filename + " - File Not found");
                    return "";
                }
            }
        }

        /// <summary>
        /// This checks to see whether the directory actually exists, and sends back the full directory
        /// </summary>
        /// <param name="DirectoryName">Directory to check</param>
        /// <param name="CurrentMemory">Current memory</param>
        /// <returns>The full directory, or blank if file not found</returns>
        public static string PreProcessDir(string DirectoryName, Vortex_Memory CurrentMemory)
        {
            if (CurrentMemory.ApplicationFolder.LastIndexOf("\\") < CurrentMemory.ApplicationFolder.Length - 1)
                CurrentMemory.ApplicationFolder += "\\";
            if (Directory.Exists(DirectoryName) == true)
                return new DirectoryInfo(DirectoryName).FullName;
            if (Directory.Exists(CurrentMemory.ApplicationFolder + DirectoryName) == true)
                return new DirectoryInfo(CurrentMemory.ApplicationFolder + DirectoryName).FullName;
            if (Directory.Exists(CurrentMemory.ContentFolder + "\\" + DirectoryName) == true)
                return new DirectoryInfo(CurrentMemory.ContentFolder + "\\" + DirectoryName).FullName;
            else if (Directory.Exists(CurrentMemory.ApplicationFolder + CurrentMemory.ContentFolder + "\\" + DirectoryName) == true)
                return new DirectoryInfo(CurrentMemory.ApplicationFolder + CurrentMemory.ContentFolder + "\\" + DirectoryName).FullName;
            else if (DirectoryName == "application")
                return new DirectoryInfo(CurrentMemory.ApplicationFolder).FullName;
            else
            {
                try
                {
                    if (HttpContext.Current != null)
                    {
                        if (Directory.Exists(HttpContext.Current.Server.MapPath(CurrentMemory.ContentFolder + "/" + DirectoryName)) == true)
                            return HttpContext.Current.Server.MapPath(CurrentMemory.ContentFolder + "/" + DirectoryName);
                        else if (Directory.Exists(HttpContext.Current.Server.MapPath(DirectoryName)) == true)
                            return HttpContext.Current.Server.MapPath(DirectoryName);
                    }
                    CurrentMemory.Error.Add("Pre process Directory " + DirectoryName + " - Dir Not found");
                    return "";
                }
                catch
                {
                    CurrentMemory.Error.Add("Pre process Directory " + DirectoryName + " - Dir Not found");
                    return "";
                }
            }
        }

        /// <summary>
        /// This checks to see where the file actually exists, and when it finds the file it sends back the full file data
        /// </summary>
        /// <param name="Filename">Filename to check</param>
        /// <param name="CurrentMemory">Current Memory</param>
        /// <returns>Data found for the file</returns>
        public static string PreProcessDataFile(string Filename, Vortex_Memory CurrentMemory)
        {
            if(File.Exists(Filename) == true)
            {
                SaveFile(new FileInfo(Filename).FullName, CurrentMemory);
                return File.ReadAllText(Filename);
            }
            if (File.Exists(CurrentMemory.ApplicationFolder + Filename) == true)
            {
                SaveFile(new FileInfo(CurrentMemory.ApplicationFolder + Filename).FullName, CurrentMemory);
                return File.ReadAllText(CurrentMemory.ApplicationFolder + Filename);
            }
            else
            {
                try
                {
                    if (File.Exists(HttpContext.Current.Server.MapPath(Filename)) == true)
                    {
                        SaveFile(HttpContext.Current.Server.MapPath(Filename), CurrentMemory);
                        return File.ReadAllText(HttpContext.Current.Server.MapPath(Filename));
                    }
                    else if (File.Exists(HttpContext.Current.Server.MapPath(CurrentMemory.ContentFolder + "\\" + Filename)) == true)
                    {
                        SaveFile(HttpContext.Current.Server.MapPath(CurrentMemory.ContentFolder + "\\" + Filename), CurrentMemory);
                        return File.ReadAllText(HttpContext.Current.Server.MapPath(CurrentMemory.ContentFolder + "\\" + Filename));
                    }
                    CurrentMemory.Error.Add("Pre process Data File " + Filename + " - File Not found");
                    return "";
                }
                catch
                {
                    CurrentMemory.Error.Add("Pre process Data File " + Filename + " - File Not found");
                    return "";
                }
            }
        }

        /// <summary>
        /// This checks to see where the file actually exists, and when it finds the file it sends back the full file data in Base64 format
        /// </summary>
        /// <param name="Filename">Filename to check</param>
        /// <param name="CurrentMemory">Current Memory</param>
        /// <returns>Data found for the file</returns>
        public static string PreProcessDataFileBase64(string Filename, Vortex_Memory CurrentMemory)
        {
            if (File.Exists(Filename) == true)
            {
                SaveFile(new FileInfo(Filename).FullName, CurrentMemory);
                return Convert.ToBase64String(File.ReadAllBytes(Filename));
            }
            if (File.Exists(CurrentMemory.ApplicationFolder + Filename) == true)
            {
                SaveFile(new FileInfo(CurrentMemory.ApplicationFolder + Filename).FullName, CurrentMemory);
                return Convert.ToBase64String(File.ReadAllBytes(CurrentMemory.ApplicationFolder + Filename));
            }
            else
            {
                try
                {
                    if (File.Exists(HttpContext.Current.Server.MapPath(Filename)) == true)
                    {
                        SaveFile(HttpContext.Current.Server.MapPath(Filename), CurrentMemory);
                        return Convert.ToBase64String(File.ReadAllBytes(HttpContext.Current.Server.MapPath(Filename)));
                    }
                    else if (File.Exists(HttpContext.Current.Server.MapPath(CurrentMemory.ContentFolder + "\\" + Filename)) == true)
                    {
                        SaveFile(HttpContext.Current.Server.MapPath(CurrentMemory.ContentFolder + "\\" + Filename), CurrentMemory);
                        return Convert.ToBase64String(File.ReadAllBytes(HttpContext.Current.Server.MapPath(CurrentMemory.ContentFolder + "\\" + Filename)));
                    }
                    CurrentMemory.Error.Add("Pre process Data File " + Filename + " - File Not found");
                    return "";
                }
                catch
                {
                    CurrentMemory.Error.Add("Pre process Data File " + Filename + " - File Not found");
                    return "";
                }
            }
        }

        public static string CreateNewFile(string Filename, Vortex_Memory CurrentMemory)
        {
            Filename = Filename.Replace("/", "\\");
            string[] Parts = Filename.Split(new char[] { '\\' });
            int SubDirToRemove = 0;
            for (int i = Parts.Length - 2; i > 0; i--)
            {
                if (Parts[i] == "..")
                {
                    SubDirToRemove++;
                    Parts[i] = "";
                }
                if (SubDirToRemove > 0)
                {
                    SubDirToRemove--;
                    Parts[i] = "";
                }
            }
            Filename = "";
            for (int i = 0; i < Parts.Length - 1; i++)
            {
                if (Parts[i] != "")
                    Filename += Parts[i] + "\\";
            }
            // That should be the folder?
            string Name = PreProcessDir(Filename, CurrentMemory);
            if(Name != "")
            {
                File.WriteAllText(Name+ Parts[Parts.Length - 1], "");
            }
            return Name;
        }
    }
}
