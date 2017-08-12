using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game2048.Objects
{
    public class TilesMovePlan
    {
        public int[,] SourceMatrix { get; set; } // picture before movement start
        public int[,] MovesMap { get; set; } // total number of moves
        public int[,] DoublingMap { get; set; } // indicates, if tile on the place will be doubling
        public int[,] DeletionMap { get; set; } // indicates, if tile on the place will be delete
        public int[,] DestinationMatrix { get; set; } // final picture after movement

        public MovesHandling.MoveDirection Direction { get; set; } // relevant direction for this move plan
        public Coordinates[,] MoveVectorsMatrix { get; set; } // matrix of move vectors
        public Coordinates[,] TileDistinationCoordinates { get; set; } // final position of tile on grid after move

        public bool IsBlindAlley()
        {
            if (MovesMap != null)
                return MovesMap.OfType<int>().Sum() == 0; // if sum of all elements is zero, no moves needed
            
            return true; // if MovesMap isn't defined, no moves needed
        }

        public bool IsBlindAlleyForAnyDirection() // check all possible directions
        {
            TilesMovePlan[] mps = new TilesMovePlan[4];
            for (int i = 0; i < 4; i++)
                mps[i] = MovesHandling.GetTilesMovePlan((MovesHandling.MoveDirection)i, SourceMatrix);
            foreach (TilesMovePlan mp in mps)
            {
                if (!mp.IsBlindAlley())
                    return false;
            }
                return true;
        }
    }
}
