using System;
using ConsoleStein.Maths;

namespace ConsoleStein.Components
{
    public sealed class TransformComponent : ITransformComponent
    {
        public IEntity Entity { get; set; }

        public Vector2 Position { get; set; }

        private float m_rotation;
        public float Rotation 
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
            Forward = RotateVector(Vector2.forward, Rotation);
            Back = RotateVector(Vector2.back, Rotation);
            Right = RotateVector(Vector2.right, Rotation);
            Left = RotateVector(Vector2.left, Rotation);
        }

        private Vector2 RotateVector(Vector2 vector, float rotation)
        {
            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);

            float tx = vector.x;
            float ty = vector.y;
            return new Vector2((cos * tx) + (ty * sin), (cos * ty) - (sin * tx));            
        }

        public Vector2 Forward { get; private set; } = Vector2.forward;
        public Vector2 Back { get; private set; } = Vector2.back;
        public Vector2 Right { get; private set; } = Vector2.right;
        public Vector2 Left { get; private set; } = Vector2.left;


        public void Translate(Vector2 direction, float speed)
        {
            Position += direction * speed;
        }
    }
}
