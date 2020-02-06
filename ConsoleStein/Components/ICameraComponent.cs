using ConsoleStein.Maths;

namespace ConsoleStein.Components
{
    public interface ICameraComponent : IComponent
    {
        float FieldOfView { get; set; }
        float FarClippingDistance { get; set; }
        float NearClippingDistance { get; set; }
        Rect ViewPort { get; set; }
    }
}
