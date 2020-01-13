namespace ConsoleStein.Maths
{
    public struct Rect
    {
        public Vector2 position;
        public Vector2 size;
        public float x
        {
            get
            {
                return position.x;
            }
            set
            {
                position.x = value;
            }
        }

        public float y
        {
            get
            {
                return position.y;
            }
            set
            {
                position.y = value;
            }
        }

        public float width
        {
            get
            {
                return size.x;
            }
            set
            {
                size.x = value;
            }
        }

        public float height
        {
            get
            {
                return size.y;
            }
            set
            {
                size.y = value;
            }
        }

        public Rect(float x, float y, float width, float height)
        {
            position = new Vector2(x, y);
            size = new Vector2(width, height);
        }

        public Rect(Vector2 position, Vector2 size)
        {
            this.position = position;
            this.size = size;
        }

        public Rect(float x, float y, Vector2 size)
        {
            position = new Vector2(x, y);
            this.size = size;
        }

        public Rect(Vector2 position, float width, float height)
        {
            this.position = position;
            size = new Vector2(width, height);
        }
    }
}
