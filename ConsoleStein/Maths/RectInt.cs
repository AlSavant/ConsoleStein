namespace ConsoleStein.Maths
{
    public struct RectInt
    {
        public Vector2Int position;
        public Vector2Int size;
        public int x
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

        public int y
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

        public int width
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

        public int height
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
        
        public RectInt(int x, int y, int width, int height)
        {
            position = new Vector2Int(x, y);
            size = new Vector2Int(width, height);
        }

        public RectInt(Vector2Int position, Vector2Int size)
        {
            this.position = position;
            this.size = size;
        }

        public RectInt(int x, int y, Vector2Int size)
        {
            position = new Vector2Int(x, y);
            this.size = size;
        }

        public RectInt(Vector2Int position, int width, int height)
        {
            this.position = position;
            size = new Vector2Int(width, height);
        }
    }
}
