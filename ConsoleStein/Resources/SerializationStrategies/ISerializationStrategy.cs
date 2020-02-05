namespace ConsoleStein.Resources.SerializationStrategies
{
    public interface ISerializationStrategy
    {
        object Deserialize(string path);
    }
}
