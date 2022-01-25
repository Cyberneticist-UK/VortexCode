
using System.Collections.Generic;

namespace VortexLibrary
{
    /// <summary>
    /// A Generic interface for graphic filters
    /// </summary>
    public interface Interface_GraphicFilter
    {
        string FilterName { get; }

        string ErrorMessage { get; set; }

        string Description { get; }

        Dictionary<string,object> DefaultSettings { get; }

        void ApplyFilter(NetBitmap Image, Dictionary<string, object> Settings);
        
    }
}
