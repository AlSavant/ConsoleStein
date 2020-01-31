using System;
using System.Collections.Generic;
using ConsoleStein.Util;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ConsoleStein.Resources
{
    internal sealed class ResourcesSystem
    {
        private Dictionary<string, object> ResourcesLookup { get; set; }
        private Dictionary<string, Func<string, object>> Deserializers { get; set; }
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
            Deserializers = new Dictionary<string, Func<string, object>>();
            Deserializers.Add(".csp", BinaryStrategy);
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
            var val = (T)Deserializers[ext](file.FullName);
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

        private object BinaryStrategy(string path)
        {
            try
            {
                var formatter = new BinaryFormatter();
                formatter.Binder = new BinaryConverter();
                var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                var val = formatter.Deserialize(stream);
                stream.Close();
                return val;
            }
            catch
            {
                return null;
            }
        }
    }
}
