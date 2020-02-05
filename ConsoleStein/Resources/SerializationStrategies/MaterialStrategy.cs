using ConsoleStein.Rendering;
using System.IO;
using System.Xml.Serialization;
using ConsoleStein.Assets;

namespace ConsoleStein.Resources.SerializationStrategies
{
    public class MaterialStrategy : ISerializationStrategy
    {
        private ResourcesSystem resourcesSystem;

        public MaterialStrategy(ResourcesSystem resourcesSystem)
        {
            this.resourcesSystem = resourcesSystem;
        }

        public object Deserialize(string path)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ConsoleMaterial));
                using (Stream reader = new FileStream(path, FileMode.Open))
                {
                    var mat = (ConsoleMaterial)serializer.Deserialize(reader);
                    if(mat != null)
                    {
                        mat.texture = resourcesSystem.Load<ConsoleSprite>(mat.texturePath);
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
