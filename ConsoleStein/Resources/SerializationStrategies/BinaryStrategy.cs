using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ConsoleStein.Resources.SerializationStrategies
{
    public class BinaryStrategy : ISerializationStrategy
    {
        public object Deserialize(string path)
        {
            try
            {
                var formatter = new BinaryFormatter();
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
