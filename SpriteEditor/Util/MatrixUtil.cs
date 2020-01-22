namespace SpriteEditor.Util
{
    public static class MatrixUtil
    {
        public static T[] FlipMatrixHorizontally<T>(T[] matrix, int matrixWidth, int matrixHeight)
        {
            T[] dest = new T[matrixWidth * matrixHeight];

            for (int i = 0; i < matrix.Length; i++)
            {
                int oldX = i % matrixWidth;
                int oldY = i / matrixWidth;

                int newX = matrixWidth - oldX - 1;
                int newY = oldY;

                dest[newY * matrixWidth + newX] = matrix[oldY * matrixWidth + oldX];
            }
            return dest;
        }

        public static T[] FlipMatrixVertically<T>(T[] matrix, int matrixWidth, int matrixHeight)
        {
            T[] dest = new T[matrixWidth * matrixHeight];

            for (int i = 0; i < matrix.Length; i++)
            {
                int oldX = i % matrixWidth;
                int oldY = i / matrixWidth;

                int newX = oldX;
                int newY = matrixHeight - oldY - 1;

                dest[newY * matrixWidth + newX] = matrix[oldY * matrixWidth + oldX];
            }
            return dest;
        }

        public static T[] RotateMatrix180<T>(T[] matrix, int matrixWidth, int matrixHeight)
        {
            T[] dest = new T[matrixWidth * matrixHeight];

            for (int i = 0; i < matrix.Length; i++)
            {
                int oldX = i % matrixWidth;
                int oldY = i / matrixWidth;

                int newX = matrixWidth - oldX - 1;
                int newY = matrixHeight - oldY - 1;

                dest[newY * matrixWidth + newX] = matrix[oldY * matrixWidth + oldX];
            }
            return dest;
        }

        public static T[] RotateMatrix90CW<T>(T[] matrix, int matrixWidth, int matrixHeight)
        {
            T[] dest = new T[matrixWidth * matrixHeight];
            for(int i = 0; i < matrix.Length; i++)
            {
                int oldX = i % matrixWidth;
                int oldY = i / matrixWidth;

                int newX = matrixHeight - oldY - 1;
                int newY = oldX;

                dest[newY * matrixHeight + newX] = matrix[oldY * matrixWidth + oldX];
                
            }
            return dest;
        }

        public static T[] RotateMatrix90CCW<T>(T[] matrix, int matrixWidth, int matrixHeight)
        {
            T[] dest = new T[matrixWidth * matrixHeight];
            for (int i = 0; i < matrix.Length; i++)
            {
                int oldX = i % matrixWidth;
                int oldY = i / matrixWidth;

                int newY = matrixWidth - oldX - 1;
                int newX = oldY;

                dest[newY * matrixHeight + newX] = matrix[oldY * matrixWidth + oldX];

            }
            return dest;
        }       
    }
}
