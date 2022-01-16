using System.Collections.Generic;
using System.Linq;

namespace MonogameTetris.TetrisLib
{
    public static class AiFunctions
    {
        public static List<PiecePosition> FindPossiblePositions(int activePieceType, int heldPieceType, int[,] boardArray)
        {
            var activePieceTemp = new ActivePiece(new IntVector2(5, 15), 0, activePieceType, activePieceType == 2 ? 4 : 3);
            var possiblePositions = new List<PiecePosition>();

           
                
            for (var i = 0; i < 4; i++)
            {
                activePieceTemp.RotState = i;
                while (activePieceTemp.MoveLeft(boardArray))
                {
                    activePieceTemp.MoveLeft(boardArray);
                }
                activePieceTemp.CurrentLocation.Y = 15;
                
                for (var j = 0; j < 10; j++)
                {

                    activePieceTemp.HardDrop(boardArray);
                    
                    if (!possiblePositions.Contains(new PiecePosition(activePieceTemp.CurrentLocation, activePieceTemp.RotState)))
                        possiblePositions.Add(new PiecePosition(activePieceTemp.CurrentLocation, activePieceTemp.RotState));

                    activePieceTemp.CurrentLocation.Y = 15;
                    
                    if (!activePieceTemp.MoveRight(boardArray))
                        break;
                }
            }

            return possiblePositions;
        }
    }
}