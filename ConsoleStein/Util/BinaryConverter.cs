using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace ConsoleStein.Util
{
    sealed class BinaryConverter : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (assemblyName.Contains("SpriteEditor"))
            {
                if(typeName.Contains("ConsoleSprite"))
                {
                    typeName = "ConsoleStein.Rendering.ConsoleSprite";
                }                                
            }
            assemblyName = Assembly.GetExecutingAssembly().FullName;
            return Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
        }
    }
}
