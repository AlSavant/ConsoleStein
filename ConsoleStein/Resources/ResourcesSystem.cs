using System;
using System.Collections.Generic;
using System.IO;
using ConsoleStein.Resources.SerializationStrategies;

namespace ConsoleStein.Resources
{
    public sealed class ResourcesSystem
    {
        private Dictionary<string, object> ResourcesLookup { get; set; }
        private Dictionary<string, ISerializationStrategy> Deserializers { get; set; }
        private string Root { get; set; }
        private bool PathExists { get; set; }

        public void Setup()
        {
            Root = AppDomain.CurrentDomain.BaseDirectory;
            Root += "/Resources/";
            if(!Directory.Exists(Root))
            {
                PathExists = false;
                return;
            }
            PathExists = true;

            ResourcesLookup = new Dictionary<string, object>();
            Deserializers = new Dictionary<string, ISerializationStrategy>();
            Deserializers.Add(".csp", new BinaryStrategy());
            Deserializers.Add(".mat", new MaterialStrategy(this));
        }

        public T Load<T>(string path)
        {
            if(!PathExists)
                return default;
            if(ResourcesLookup.ContainsKey(path))
            {
                return (T)ResourcesLookup[path];
            }
            string directory = Path.GetDirectoryName(Root + path);
            string fileName = Path.GetFileName(Root + path);

            DirectoryInfo dir = new DirectoryInfo(directory);
            FileInfo[] files = dir.GetFiles(fileName + ".*");
            if (files.Length < 0)
                return default;

            FileInfo file = files[0];
            var ext = file.Extension;
            if (!Deserializers.ContainsKey(ext))
                return default;
            var val = (T)Deserializers[ext].Deserialize(file.FullName);
            if (val == null)
                return default;
            ResourcesLookup.Add(path, val);
            return val;
        }

        public void Unload(object asset)
        {
            var keys = ResourcesLookup.Keys;
            string assetKey = string.Empty;
            foreach(var key in keys)
            {
                if(ResourcesLookup[key] == asset)
                {
                    assetKey = key;
                    break;
                }
            }
            ResourcesLookup.Remove(assetKey);
        }

        public void UnloadAll()
        {
            ResourcesLookup.Clear();
        }        
    }
}
