using System;
using ConsoleStein.Maths;
using ConsoleStein.Assets;
using System.Xml.Serialization;
using ConsoleStein.Util;

namespace ConsoleStein.Rendering
{
    [Serializable]
    public class ConsoleMaterial
    {
        public string texturePath;
        public Vector2 tiling;
        public Vector2 offset;

        [XmlIgnore]
        public ConsoleSprite texture;

        public byte[] SamplePixel(Vector2 uv)
        {
            if(texture == null)
            {
                return new byte[] { 0, 0 };
            }
            Vector2 newUV = new Vector2(uv.x * tiling.x + offset.x, uv.y * tiling.y + offset.y);
            newUV.x = (float)(newUV.x - Math.Truncate(newUV.x));
            newUV.y = (float)(newUV.y - Math.Truncate(newUV.y));
            return texture.SamplePixel(newUV);

        }
    }
}
