using System;
using ConsoleStein.Maths;

namespace ConsoleStein.Components
{
    public sealed class TransformComponent : Component
    {
        public Vector2 position;

        private float m_rotation;
        public float rotation 
        {
            get
            {
                return m_rotation;
            }
            set
            {
                if(m_rotation != value)
                {
                    m_rotation = value;
                    RecalculateOrientation();
                }
            }
        }

        private void RecalculateOrientation()
        {
            forward = RotateVector(Vector2.forward, rotation);
            back = RotateVector(Vector2.back, rotation);
            right = RotateVector(Vector2.right, rotation);
            left = RotateVector(Vector2.left, rotation);
        }

        private Vector2 RotateVector(Vector2 vector, float rotation)
        {
            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);

            float tx = vector.x;
            float ty = vector.y;
            return new Vector2((cos * tx) + (ty * sin), (cos * ty) - (sin * tx));            
        }

        public Vector2 forward { get; private set; } = Vector2.forward;
        public Vector2 back { get; private set; } = Vector2.back;
        public Vector2 right { get; private set; } = Vector2.right;
        public Vector2 left { get; private set; } = Vector2.left;


        public void Translate(Vector2 direction, float speed)
        {
            position += direction * speed;
        }
    }
}
