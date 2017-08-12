using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Windows.Controls;

using Game2048.Other;

namespace Game2048.Objects
{
    public class MovesHandling
    {        
        /// <summary>
        /// Move direction or number of 90 degree rotations clockwise from Left direction
        /// </summary>
        public enum MoveDirection
        {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }

        #region HorizontalMoveTransformation
        private static int GetHorizontalMoveByDirection(MoveDirection direction, int move)
        {
            return move * GetHorizontalMoveByDirection(direction); // multitiplies horizontal move n times
        }

        private static int GetHorizontalMoveByDirection(MoveDirection direction) // returns number of unit vertical steps in selected direction
        {
            return -(int)Math.Cos((int)direction * Math.PI / 2); // - cos(direction * π/2);
        }
        #endregion

        #region VerticalMoveTransformation
        private static int GetVerticalMoveByDirection(MoveDirection direction, int move)
        {
            return move * GetVerticalMoveByDirection(direction); // multitiplies vertical move n times
        }

        private static int GetVerticalMoveByDirection(MoveDirection direction) // returns number of unit vertical steps in selected direction
        {
            return -(int)Math.Sin((int)direction * Math.PI / 2); // - sin(direction * π/2);
        }
        #endregion

        public static Coordinates[,] GetTIlesDistinationCoordinates(TilesMovePlan mp)
        {
            int n = mp.MovesMap.GetLength(0);
            int m = mp.MovesMap.GetLength(1);
            int[,] sm = (int[,])mp.SourceMatrix.Clone(); // source matrix
            Coordinates[,] mv = new Coordinates[n, m]; // move vectors
            Coordinates[,] dc = new Coordinates[n, m]; // destination coordinates
            int[,] dm = new int[n, m]; // destination matrix
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    mv[i, j] = new Coordinates(
                        GetVerticalMoveByDirection(mp.Direction, mp.MovesMap[i, j]),
                        GetHorizontalMoveByDirection(mp.Direction, mp.MovesMap[i, j])
                        ); // get move vector
                    dc[i, j] = Coordinates.Sum(new Coordinates(i, j), mv[i, j]); // get coordinates of destination

                    dm[dc[i, j].Row, dc[i, j].Column] += sm[i, j]; // increase number in destination
                    sm[i, j] -= sm[i, j]; // reduce source number
                }
            }

            mp.MoveVectorsMatrix = mv;
            mp.TileDistinationCoordinates = dc;
            mp.DestinationMatrix = dm;

            return dc;
        }

        public static TilesMovePlan GetTilesMovePlan() // default move plan (to the left)
        {
            return GetTilesMovePlan(MoveDirection.Left, GameBoard.GetGridTileMatrix());
        }
        
        public static TilesMovePlan GetTilesMovePlan(MoveDirection direction) // move plan for specified direction
        {
            return GetTilesMovePlan(direction, GameBoard.GetGridTileMatrix());
        }

        public static TilesMovePlan GetTilesMovePlan(MoveDirection direction, int[,] sourceMatrix)
        {
            // rotate source matrix (Top || Right || Bottom --> Left)
            sourceMatrix = MatrixOperations.RotateMatrix(sourceMatrix, (uint)direction, MatrixOperations.RotationDirection.ACW);
            TilesMovePlan mp = GetTilesMovePlan(sourceMatrix); // perform all calculation for left direction
            // rotate all move plan matrices back (Left --> Top || Right || Bottom)
            mp.SourceMatrix = MatrixOperations.RotateMatrix(sourceMatrix, (uint)direction);
            mp.MovesMap = MatrixOperations.RotateMatrix(mp.MovesMap, (uint)direction);
            mp.DoublingMap = MatrixOperations.RotateMatrix(mp.DoublingMap, (uint)direction);
            mp.DeletionMap = MatrixOperations.RotateMatrix(mp.DeletionMap, (uint)direction);
            mp.Direction = direction;

            return mp;
        }

        private static TilesMovePlan GetTilesMovePlan(int[,] matrix) // move plan for default direction (to the left)
        {
            int n = matrix.GetLength(0);
            int m = matrix.GetLength(1);

            int[,] zeroMoves = new int[n, m]; // moves caused by empty places on grid
            int[,] doublingMoves = new int[n, m]; // moves caused by doubling of joined tiles
            int[,] moves = new int[n, m];

            int[,] doublingIndicator = new int[n, m]; 
            int[,] deletionIndicator = new int[n, m]; 

            for (int i = 0; i < n; i++) //rows
            {
                for (int j = 0; j < m; j++) //columns
                {
                    if (!(matrix[i, j] == 0 || j == 0))
                    {
                        zeroMoves[i, j] = Enumerable.Range(0, j)
                            .Where(x => matrix[i, x] == 0)
                            .Count(); // number of zeros before

                        int prevNonZeroValueRow = Enumerable.Range(0, j)
                            .Reverse()
                            .SkipWhile(x => matrix[i, x] == 0)
                            .FirstOrDefault(); // get row number of previous non-zero value
                        doublingIndicator[i, j] = (matrix[i, prevNonZeroValueRow] == matrix[i, j] && doublingIndicator[i, prevNonZeroValueRow] != 1 ? 1 : 0); // compare previous non-zero (and non-doubling) value with current value

                        deletionIndicator[i, prevNonZeroValueRow] = doublingIndicator[i, j]; // set previous non-zero value as condidate for deletion

                        doublingMoves[i, j] = Enumerable.Range(0, j + 1)
                            .Sum(x => doublingIndicator[i, x]); // cumulate moves based on indicator value

                        moves[i, j] = zeroMoves[i, j] + doublingMoves[i, j];
                    }
                }
            }
            return new TilesMovePlan() {
                MovesMap = moves,
                DoublingMap = doublingIndicator,
                DeletionMap = deletionIndicator
            };
        }
    }
}
