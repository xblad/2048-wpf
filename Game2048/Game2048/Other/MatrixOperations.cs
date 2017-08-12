using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

using Game2048.Objects;

namespace Game2048.Other
{
    class MatrixOperations
    {
        #region RotateMatrix

        /// <summary>
        /// CW - Clockwise rotation direction
        /// ACW - Anticlockwise rotation direction
        /// </summary>
        public enum RotationDirection
        {
            CW = 1, //Clockwise
            ACW = -1, //Anticlockwise
        }

        /// <summary>
        /// Rotates specified matrix
        /// </summary>
        /// <param name="matrix">specified 2D array of integers</param>
        /// <param name="numberOfRotations">integer number of rotations</param>
        /// <returns></returns>
        public static int[,] RotateMatrix(int[,] matrix, int numberOfRotations)
        {
            return RotateMatrix(matrix, (uint)Math.Abs(numberOfRotations), (RotationDirection)Math.Sign(numberOfRotations));
        }

        /// <summary>
        /// Rotates specified matrix
        /// </summary>
        /// <param name="matrix">specified 2D array of integers</param>
        /// <param name="numberOfRotations">positive integer number of rotations</param>
        /// <param name="direction">One of two possible rotation direction (clockwise and anticlockwise)</param>
        /// <returns></returns>
        public static int[,] RotateMatrix(int[,] matrix, uint numberOfRotations, RotationDirection direction)
        {  
            if (direction == RotationDirection.ACW)
                numberOfRotations = 4 - numberOfRotations % 4; // get number of clockwise rotations form number of anticlockwise rotations
            return RotateMatrix(matrix, numberOfRotations);
        }

        /// <summary>
        /// Rotates specified matrix clockwise
        /// </summary>
        /// <param name="matrix">specified 2D array of integers</param>
        /// <param name="numberOfRotations">positive integer number of clockwise rotations</param>
        /// <returns></returns>
        public static int[,] RotateMatrix(int[,] matrix, uint numberOfRotations)
        {
            numberOfRotations = numberOfRotations % 4; // reduce useless rotations

            for (int i = 0; i < numberOfRotations; i++)
                matrix = RotateMatrix(matrix);

            return matrix;
        }

        /// <summary>
        /// One rotation of specified matrix clockwise
        /// </summary>
        /// <param name="matrix">specified 2D array of integers</param>
        /// <returns></returns>
        public static int[,] RotateMatrix(int[,] matrix)
        {
            int n = matrix.GetLength(0);
            int m = matrix.GetLength(1);
            int[,] newMatrix = new int[m, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    newMatrix[i, j] = matrix[(m - 1) - j, i];
                }
            }

            return newMatrix;
        }
        #endregion

        #region ConvertMatrix functions
        public static object[] ConvertToXElementsArray(int[,] matrix) // get XElements array from multidimentional array
        {
            
            int n = matrix.GetLength(0);
            int m = matrix.GetLength(1);
            object[] XElementArray = new object[n * m];

            int index = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    XElementArray[index] = new XElement("cell", new XAttribute("row", i), new XAttribute("column", j), matrix[i, j]);
                    index++;
                }
            }

            return XElementArray;
        }
        #endregion

        #region Debug:PrintMatrix Functions
        public static void PrintMatrix(int[,] matrix)
        {
            int n = matrix.GetLength(0);
            int m = matrix.GetLength(1);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    Debug.Write(matrix[i, j] + (j != m - 1 ? "\t" : ""));
                }
                Debug.WriteLine("");
            }
        }

        public static void PrintMatrix(Coordinates[,] matrix)
        {
            int n = matrix.GetLength(0);
            int m = matrix.GetLength(1);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    Debug.Write(matrix[i, j] + (j != m - 1 ? "\t" : ""));
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("");
        }
        #endregion
    }
}
