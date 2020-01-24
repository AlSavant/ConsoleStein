using SpriteEditor.Models;
using System;

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

        public static T[] ResizeMatrix<T>(T[] matrix, int matrixWidth, int matrixHeight, int newWidth, int newHeight, Vector2Int normalizedPivot)
        {
            T[] dest = new T[newWidth * newHeight];
            int oldHalfWidth = matrixWidth / 2;
            int oldHalfHeight = matrixHeight / 2;
            int newHalfWidth = newWidth / 2;
            int newHalfHeight = newHeight / 2;
            for (int i = 0; i < matrix.Length; i++)
            {
                int oldX = i % matrixWidth;
                int oldY = i / matrixWidth;

                if(newWidth < matrixWidth)
                {
                    switch (normalizedPivot.x)
                    {
                        case -1:
                            if (oldX > newWidth)
                                continue;
                            break;

                        case 0:
                            if (oldX > Math.Abs(oldHalfWidth - newHalfWidth))
                                continue;
                            break;

                        case 1:
                            if (oldX > Math.Abs(matrixWidth - newWidth))
                                continue;
                            break;
                    }
                }

                if (newHeight < matrixHeight)
                {
                    switch (normalizedPivot.y)
                    {
                        case -1:
                            if (oldY > newHeight)
                                continue;
                            break;

                        case 0:
                            if (oldY > Math.Abs(oldHalfHeight - newHalfHeight))
                                continue;
                            break;

                        case 1:
                            if (oldY > Math.Abs(matrixHeight - newHeight))
                                continue;
                            break;
                    }
                }


                //int newY = matrixWidth - oldX - 1;
                //int newX = oldY;

                //dest[newY * matrixHeight + newX] = matrix[oldY * matrixWidth + oldX];

            }
            return dest;
        }
    }
}
