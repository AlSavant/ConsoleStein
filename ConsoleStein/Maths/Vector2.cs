using System;

namespace ConsoleStein.Maths
{
    public struct Vector2
    {
        public float x;
        public float y;

        public static Vector2 zero { get; private set; } = new Vector2(0f, 0f);
        public static Vector2 forward { get; private set; } = new Vector2(0f, 1f);
        public static Vector2 back { get; private set; } = new Vector2(0f, -1f);
        public static Vector2 right { get; private set; } = new Vector2(1f, 0f);
        public static Vector2 left { get; private set; } = new Vector2(-1f, 0f);

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float magnitude
        {
            get
            {
                return (float)Math.Sqrt(x * x + y * y);
            }
        }

        public static float Dot(Vector2 from, Vector2 toOther)
        {
            float d = toOther.magnitude;
            return (from.x * toOther.x / d) + (from.y * toOther.y / d);
        }

        public static float Distance(Vector2 pointA, Vector2 pointB)
        {
            float x = pointA.x - pointB.x;
            float y = pointA.y - pointB.y;
            return (float)Math.Sqrt(x * x + y * y);
        }

        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x + v2.x, v1.y + v2.y);
        }

        public static Vector2 operator -(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x - v2.x, v1.y - v2.y);
        }

        public static Vector2 operator *(Vector2 v1, float f)
        {
            return new Vector2(v1.x * f, v1.y * f);
        }

        public static Vector2 operator /(Vector2 v1, float f)
        {
            return new Vector2(v1.x / f, v1.y / f);
        }
    }
}
