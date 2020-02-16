using ConsoleStein.Rendering;
using System.IO;
using System.Xml.Serialization;
using ConsoleStein.Assets;

namespace ConsoleStein.Resources.SerializationStrategies
{
    public class SkyboxStrategy : ISerializationStrategy
    {
        private ResourcesSystem resourcesSystem;

        public SkyboxStrategy(ResourcesSystem resourcesSystem)
        {
            this.resourcesSystem = resourcesSystem;
        }

        public object Deserialize(string path)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(SkyboxMaterial));
                using (Stream reader = new FileStream(path, FileMode.Open))
                {
                    var mat = (SkyboxMaterial)serializer.Deserialize(reader);
                    if (mat != null)
                    {
                        foreach(var layer in mat.layers)
                        {
                            layer.texture = resourcesSystem.Load<ConsoleSprite>(layer.texturePath);
                        }
                        
                    }
                    return mat;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
