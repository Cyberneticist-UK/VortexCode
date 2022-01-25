using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vortex
{
    public static class StringToEnum
    {
        public static T GetEnumFromName<T>(string name) => (T)Enum.Parse(typeof(T), name);
    }
}
