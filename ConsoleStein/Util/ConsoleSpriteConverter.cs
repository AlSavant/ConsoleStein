using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace ConsoleStein.Util
{
    sealed class ConsoleSpriteConverter : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type returntype = null;
            Console.Write(typeName);
            if (typeName.Contains("ConsoleSprite"))
            {
                typeName = "ConsoleStein.Rendering.ConsoleSprite";
                assemblyName = Assembly.GetExecutingAssembly().FullName;
                returntype =
                    Type.GetType(String.Format("{0}, {1}",
                    typeName, assemblyName));
                return returntype;
            }
            return returntype;
        }
    }
}
