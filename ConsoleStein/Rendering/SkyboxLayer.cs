using System;
using ConsoleStein.Assets;
using System.Xml.Serialization;

namespace ConsoleStein.Rendering
{
    [Serializable]
    public class SkyboxLayer
    {
        public float rotation;
        public string texturePath;

        [XmlIgnore]
        public ConsoleSprite texture;        
    }
}