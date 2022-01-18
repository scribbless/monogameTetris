using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MonogameTetris.TetrisLib
{
    public static class AiFunctions
    {
        private static MovePermutations _permutations = new MovePermutations();

        public static IEnumerable<int> AStar1(int activePieceType, int heldPieceType, int[,] boardArray, PiecePosition start, PiecePosition goal)
        {
            //      GET POSSIBLE MOVES

            //1 = move right
            //2 = move left
            //3 = move down
            //4 = rotate R
            //5 = rotate L

            var pieceDictionary = new PieceDictionary();
            var validMoves = new List<int>();
            var activePieceTemp = new ActivePiece(new IntVector2(start.Position.X, start.Position.Y), start.Rotation, activePieceType, activePieceType == 2 ? 4 : 3);
            
            //check right
            if (activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState, new IntVector2(1, 0))) validMoves.Add(1);
            
            //check left
            if (activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState, new IntVector2(-1, 0))) validMoves.Add(2);
            
            //check up
            if (activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState, new IntVector2(0, -1))) validMoves.Add(3);

            var newRotStateR = activePieceTemp.RotState == 3 ? 0 : activePieceTemp.RotState + 1;
            var newRotStateL = activePieceTemp.RotState == 0 ? 3 : activePieceTemp.RotState - 1;

            for (var i = 0; i < 5; i++)
            {
                var wallkick = new IntVector2(pieceDictionary.GetWallKick(false, true, newRotStateR)[i, 0] * -1, pieceDictionary.GetWallKick(false, true, newRotStateR)[i, 1] * -1);

                if (!activePieceTemp.IsValidMove(boardArray, newRotStateR, wallkick)) continue;
                validMoves.Add(4);
                break;
            }

            for (var i = 0; i < 5; i++)
            {
                var wallkick = new IntVector2(pieceDictionary.GetWallKick(false, false, newRotStateR)[i, 0] * -1, pieceDictionary.GetWallKick(false, true, newRotStateR)[i, 1] * -1);

                if (!activePieceTemp.IsValidMove(boardArray, newRotStateR, wallkick)) continue;
                validMoves.Add(5);
                break;
            }

            if (validMoves.Count == 0) return ImmutableArray<int>.Empty;

            var lowestH = 10000000;
            foreach (var move in validMoves)
            {
                //calculate heuristic
            }
            
            return ImmutableArray<int>.Empty;
        }
        public static List<PiecePosition> FindPossiblePositions1(int activePieceType, int heldPieceType, int[,] boardArray)
        {
            var activePieceTemp = new ActivePiece(new IntVector2(5, 15), 0, activePieceType, activePieceType == 2 ? 4 : 3);
            var possiblePositions = new List<PiecePosition>();

           
            activePieceTemp.RotState = 0;
            for (var i = 0; i < 4; i++)
            {
                while (true)
                {
                    if (!activePieceTemp.MoveLeft(boardArray)) break;
                }
                activePieceTemp.CurrentLocation.Y = 0;
                
                for (var j = 0; j < 10; j++)
                {

                    activePieceTemp.HardDrop(boardArray);
                    
                    
                    if (!possiblePositions.Contains(new PiecePosition(activePieceTemp.CurrentLocation, activePieceTemp.RotState)))
                        possiblePositions.Add(new PiecePosition(activePieceTemp.CurrentLocation, activePieceTemp.RotState));

                    var posTemp = activePieceTemp.CurrentLocation;
                    var rotTemp = activePieceTemp.RotState;
                    
                    //check double right R
                    if (activePieceTemp.IncreaseRotState(boardArray)[0])
                    {
                        if (!possiblePositions.Contains(new PiecePosition(activePieceTemp.CurrentLocation, activePieceTemp.RotState)))
                            possiblePositions.Add(new PiecePosition(activePieceTemp.CurrentLocation, activePieceTemp.RotState));
                        
                        if (activePieceTemp.IncreaseRotState(boardArray)[0])
                        {
                            if (!possiblePositions.Contains(new PiecePosition(activePieceTemp.CurrentLocation, activePieceTemp.RotState)))
                                possiblePositions.Add(new PiecePosition(activePieceTemp.CurrentLocation, activePieceTemp.RotState));
                        }
                    }

                    activePieceTemp.CurrentLocation = posTemp;
                    activePieceTemp.RotState = rotTemp;
                    
                    if (activePieceTemp.DecreaseRotState(boardArray)[0])
                    {
                        if (!possiblePositions.Contains(new PiecePosition(activePieceTemp.CurrentLocation, activePieceTemp.RotState)))
                            possiblePositions.Add(new PiecePosition(activePieceTemp.CurrentLocation, activePieceTemp.RotState));
                        
                        if (activePieceTemp.DecreaseRotState(boardArray)[0])
                        {
                            if (!possiblePositions.Contains(new PiecePosition(activePieceTemp.CurrentLocation, activePieceTemp.RotState)))
                                possiblePositions.Add(new PiecePosition(activePieceTemp.CurrentLocation, activePieceTemp.RotState));
                        }
                    }

                    activePieceTemp.CurrentLocation.Y = 0;
                    
                    if (!activePieceTemp.MoveRight(boardArray))
                        break;
                }

                if (!activePieceTemp.IncreaseRotState(boardArray)[0])
                {
                    break;
                }
            }

            return possiblePositions;
        }

        public static int FindValidMove(int activePieceType, int heldPieceType, int[,] boardArray,
            PiecePosition start)
        {
            var pieceDictionary = new PieceDictionary();
            var validMoves = new List<int>();
            var activePieceTemp = new ActivePiece(new IntVector2(start.Position.X, start.Position.Y), start.Rotation, activePieceType, activePieceType == 2 ? 4 : 3);


            if (activePieceTemp.CanSeeRoof(boardArray))
            {
                //go to roof and then to middle
            }
            else
            {
                var newRotStateR = activePieceTemp.RotState == 3 ? 0 : activePieceTemp.RotState + 1;
                var newRotStateL = activePieceTemp.RotState == 0 ? 3 : activePieceTemp.RotState - 1;

                foreach (var permutation in _permutations._movePermutations)
                {
                    //reset position
                    activePieceTemp.CurrentLocation = start.Position;
                    activePieceTemp.RotState = start.Rotation;
                    
                    for (var i = 0; i < 3; i++)
                    {
                        switch (int.Parse(permutation.ToString().Split()[i]))
                        {
                            case 0:
                                //check right
                                if (activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState,
                                        new IntVector2(1, 0))) activePieceTemp.CurrentLocation.X += 1;
                                break;
                            case 1:
                                //check left
                                if (activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState, new IntVector2(-1, 0))) activePieceTemp.CurrentLocation.X -= 1;
                                break;
                            case 2:
                                //check up
                                if (activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState, new IntVector2(0, -1))) activePieceTemp.CurrentLocation.Y -= 1;
                                break;
                            case 3:
                                //check rot R
                                for (var j = 0; j < 5; j++)
                                {
                                    var wallkick = new IntVector2(pieceDictionary.GetWallKick(false, true, newRotStateR)[j, 0] * -1, pieceDictionary.GetWallKick(false, true, newRotStateR)[j, 1] * -1);

                                    if (!activePieceTemp.IsValidMove(boardArray, newRotStateR, wallkick)) continue;
                                    activePieceTemp.CurrentLocation.X += wallkick.X;
                                    activePieceTemp.CurrentLocation.Y += wallkick.Y;
                                    activePieceTemp.RotState = newRotStateR;
                                    break;
                                }
                                break;
                            case 4:
                                //check rot L
                                for (var j = 0; j < 5; j++)
                                {
                                    var wallkick = new IntVector2(pieceDictionary.GetWallKick(false, false, newRotStateL)[j, 0] * -1, pieceDictionary.GetWallKick(false, false, newRotStateL)[j, 1] * -1);

                                    if (!activePieceTemp.IsValidMove(boardArray, newRotStateL, wallkick)) continue;
                                    activePieceTemp.CurrentLocation.X += wallkick.X;
                                    activePieceTemp.CurrentLocation.Y += wallkick.Y;
                                    activePieceTemp.RotState = newRotStateL;
                                    break;
                                }
                                break;
                        }
                    }

                    if (activePieceTemp.CanSeeRoof(boardArray))
                    {
                        return permutation;
                    }
                }
            }

            return 999;
        }
        
        public static IEnumerable<PiecePosition> FindPossiblePositions(int activePieceType, int heldPieceType,
            int[,] boardArray)
        {
            var activePieceTemp = new ActivePiece(new IntVector2(4, 0), 0, activePieceType, activePieceType == 2 ? 4 : 3);
            var possiblePositions = new List<PiecePosition>();

           
            activePieceTemp.RotState = 0;

            if (activePieceType != 2 && activePieceType != 5)
            {
                if (activePieceTemp.RotState == 1)
                {
                    for (var i = -1; i <= 8; i++)
                    {
                        for (var j = 1; j <= 18 ; j++)
                        {
                            activePieceTemp.CurrentLocation = new IntVector2(i, j);
                            if (!activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState,
                                    new IntVector2(0, 0)))
                                continue;
                            if (!activePieceTemp.IsTouchingBlock(boardArray))
                                continue;

                            var validMove = FindValidMove(activePieceType, heldPieceType, boardArray,
                                new PiecePosition(new IntVector2(j, i), activePieceTemp.RotState));
                            if (validMove != 999)
                            {
                                
                            }
                        }
                    }
                }
            }

            return possiblePositions;
        }
    }
}