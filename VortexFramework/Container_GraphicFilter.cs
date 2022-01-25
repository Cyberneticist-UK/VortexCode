using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace VortexLibrary
{
    public class Vortex_GraphicFilter
    {
        public Container_GraphicFilter FilterList;
        string Folder;

        public Vortex_GraphicFilter(string Folder)
        {
            this.Folder = Folder;
            FilterList = new Container_GraphicFilter(Folder);
        }

        public void Refresh()
        {
            FilterList = new Container_GraphicFilter(Folder);
        }

        public List<string> GetFilterList()
        {
            List<string> Result = new List<string>();
            foreach(var Filter in FilterList.Filters)
            {
                Result.Add(Filter.Value.FilterName);
            }
            Result.Sort();
            return Result;
        }

        public string GetDescription(string FilterName)
        {
            foreach (var Filter in FilterList.Filters)
            {
                if (Filter.Value.FilterName == FilterName)
                    return Filter.Value.Description;
            }
            return "Not Found";
        }

        public Dictionary<string, object> GetSettings(string FilterName)
        {
            foreach (var Filter in FilterList.Filters)
            {
                if (Filter.Value.FilterName == FilterName)
                    return Filter.Value.DefaultSettings;
            }
            return null;
        }

        public void ApplyFilter(string FilterName, NetBitmap Image, Dictionary<string, object> Settings)
        {
            foreach (var Filter in FilterList.Filters)
            {
                if (Filter.Value.FilterName == FilterName)
                {
                    Filter.Value.ApplyFilter(Image, Settings);
                    return;
                }
            }
        }
    }

    public class Container_GraphicFilter
    {
        [ImportMany]
        public System.Lazy<Interface_GraphicFilter, SortedDictionary<string, object>>[] Filters { get; set; }

        public Container_GraphicFilter(string Folder)
        {
            DirectoryCatalog catalog = new DirectoryCatalog(Folder);
            CompositionContainer container = new CompositionContainer(catalog);
            CompositionBatch batch = new CompositionBatch();
            batch.AddPart(this);
            container.Compose(batch);
        }
    }
}
