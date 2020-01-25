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
            int pivotX = 0;
            int pivotY = 0;
            int halfWidth = matrixWidth / 2;
            int halfHeight = matrixHeight / 2;
            int newHalfWidth = newWidth / 2;
            int newHalfHeight = newHeight / 2;
            if (normalizedPivot.x == 1)
            {
                pivotX = halfWidth - newHalfWidth;
            }
            if(normalizedPivot.x == 2)
            {
                pivotX = matrixWidth - newWidth;
            }
            if (normalizedPivot.y == 1)
            {
                pivotY = halfHeight - newHalfHeight;
            }
            if (normalizedPivot.y == 2)
            {
                pivotY = matrixHeight - newHeight;
            }
            for (int i = 0; i < dest.Length; i++)
            {
                int newX = i % newWidth;
                int newY = i / newWidth;

                int oldX = pivotX + newX;
                if (oldX < 0 || oldX >= matrixWidth)
                    continue;
                int oldY = pivotY + newY;
                if (oldY < 0 || oldY >= matrixHeight)
                    continue;
                dest[newY * newWidth + newX] = matrix[oldY * matrixWidth + oldX];                
            }
            return dest;
        }
    }
}
