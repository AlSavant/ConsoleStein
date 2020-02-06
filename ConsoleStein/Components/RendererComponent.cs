using ConsoleStein.Rendering;

namespace ConsoleStein.Components
{
    public class RendererComponent : IRendererComponent
    {
        public IEntity Entity { get; set; }
        public ConsoleMaterial Material { get; set; }
    }
}
