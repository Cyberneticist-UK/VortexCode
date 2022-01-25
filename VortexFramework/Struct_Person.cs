using System;
using System.Collections.Generic;


namespace Vortex
{
    public class Struct_Person
    {
        List<Comms_SystemUser> People = new List<Comms_SystemUser>();
        Dictionary<string, Comms_SystemUser> LocalMachines = new Dictionary<string, Comms_SystemUser>();
        Dictionary<string, List<Comms_SystemUser>> Group = new Dictionary<string, List<Comms_SystemUser>>();
        Dictionary<string, List<Comms_SystemUser>> Room = new Dictionary<string, List<Comms_SystemUser>>();

        public Comms_SystemUser this[int index]
        {
            get
            {
                if (index > -1 && People.Count > index)
                    return People[index];
                else
                    return null;
            }
            set
            {
                if (index > -1 && People.Count > index)
                    People[index] = value;
            }
        }

        public List<Comms_SystemUser> GetPeople
        {
            get { return People; }
        }


        public void Add(Comms_SystemUser item)
        {
            // The unique identifier for an item is the computer name or the Netelligence ID.
            if (item.Netelligence.UserID != Guid.Empty)
            {
                int index = PersonFound(item.Netelligence.UserID);
                if (index == -1)
                {
                    People.Add(item);
                    AddItem(item);
                }
                else
                {
                    // Found them, so update:
                    People[index].Machine = item.Machine;
                    People[index].Connections = item.Connections;
                    People[index].Netelligence = item.Netelligence;
                }
            }
            else if (item.Machine.ComputerName != null)
            {
                int index = PersonFound(item.Machine.ComputerName);
                if (index == -1)
                {
                    People.Add(item);
                    AddItem(item);
                }
                else
                {
                    // Found them, so update:
                    People[index].Machine = item.Machine;
                    People[index].Connections = item.Connections;
                    People[index].Netelligence = item.Netelligence;
                }
            }
            else
            {
                People.Add(item);
                AddItem(item);
            }
        }

        private int PersonFound(string MachineName)
        {
            for (int i = 0; i < People.Count; i++)
            {
                if (People[i].Machine.ComputerName == MachineName)
                {
                    return i;
                }
            }
            return -1;
        }
        private int PersonFound(Guid NetelligenceID)
        {
            for (int i = 0; i < People.Count; i++)
            {
                if (People[i].Netelligence.UserID == NetelligenceID)
                {
                    return i;
                }
            }
            return -1;
        }


        private void AddItem(Comms_SystemUser item)
        {
            AddToDictionary(ref LocalMachines, item.Machine.ComputerName, item);
            AddToDictionary(ref Room, item.Machine.Room, item);
        }

        private void UpdateSubLists()
        {
            LocalMachines.Clear();
            Room.Clear();
            Group.Clear();
            foreach(Comms_SystemUser item in People)
            {
                AddItem(item);
            }
        }

        public void RemoveAt(int index)
        {
            if (index > -1 && People.Count < index)
            {
                Comms_SystemUser temp = People[index];
                if (temp.Machine.ComputerName != null)
                    LocalMachines.Remove(temp.Machine.ComputerName);
                if (temp.Machine.Room != null)
                    Room.Remove(temp.Machine.Room);
                temp = null;
                People.RemoveAt(index);
            }
        }

        public int Count
        {
            get { return People.Count; }
        }

        public Comms_SystemUser ByMachine(string MachineName)
        {
            if (LocalMachines.ContainsKey(MachineName))
                return LocalMachines[MachineName];
            else
                return null;
        }

        public List<Comms_SystemUser> ByRoom(string RoomName)
        {
            if (Room.ContainsKey(RoomName))
                return Room[RoomName];
            else
                return null;
        }

        public List<Comms_SystemUser> ByGroup(string GroupName)
        {
            if (Group.ContainsKey(GroupName))
                return Group[GroupName];
            else
                return null;
        }

        private void AddToDictionary(ref Dictionary<string, Comms_SystemUser> dic, string Key, Comms_SystemUser Person)
        {
            if (Key != null)
            {
                if (dic.ContainsKey(Key) == false)
                    dic.Add(Key, Person);
                else
                    dic[Key] = Person;
            }
        }

        private void AddToDictionary(ref Dictionary<string, List<Comms_SystemUser>> dic, string Key, Comms_SystemUser Person)
        {
            if (Key != null)
            {
                if (dic.ContainsKey(Key) == false)
                    dic.Add(Key, new List<Comms_SystemUser>());
                else
                    dic[Key].Add(Person);
            }
        }

    }
}
