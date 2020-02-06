using ConsoleStein.Rendering;

namespace ConsoleStein.Components
{
    public interface IRendererComponent : IComponent
    {
        ConsoleMaterial Material { get; set; }
    }
}
