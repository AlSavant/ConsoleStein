using ConsoleStein.Maths;

namespace ConsoleStein.Components
{
    public interface ITransformComponent : IComponent
    {
        Vector2 Position { get; set; }        
        float Rotation { get; set; }                
        Vector2 Forward { get; }
        Vector2 Back { get; }
        Vector2 Right { get; }
        Vector2 Left { get; }
        void Translate(Vector2 direction, float speed);
    }
}
