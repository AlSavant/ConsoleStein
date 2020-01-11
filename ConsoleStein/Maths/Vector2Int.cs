namespace ConsoleStein.Maths
{
    public struct Vector2Int
    {
        public int x;
        public int y;

        public static Vector2Int forward { get; private set; } = new Vector2Int(0, 1);
        public static Vector2Int back { get; private set; } = new Vector2Int(0, -1);
        public static Vector2Int right { get; private set; } = new Vector2Int(1, 0);
        public static Vector2Int left { get; private set; } = new Vector2Int(-1, 0);

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2Int operator +(Vector2Int v1, Vector2Int v2)
        {
            return new Vector2Int(v1.x + v2.x, v1.y + v2.y);
        }

        public static Vector2Int operator -(Vector2Int v1, Vector2Int v2)
        {
            return new Vector2Int(v1.x - v2.x, v1.y - v2.y);
        }

        public static Vector2 operator *(Vector2Int v1, float f)
        {
            return new Vector2(v1.x * f, v1.y * f);
        }

        public static Vector2Int operator *(Vector2Int v1, int f)
        {
            return new Vector2Int(v1.x * f, v1.y * f);
        }

        public static Vector2 operator /(Vector2Int v1, float f)
        {
            return new Vector2(v1.x / f, v1.y / f);
        }
    }
}
