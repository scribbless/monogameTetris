using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonogameTetris.TetrisLib
{
    public static class AiFunctions
    {
        private const int MoveCheckAmount = 3;
        private static MovePermutations _permutations = new MovePermutations(MoveCheckAmount);
        
        //simple search for positions
        public static List<PiecePosition> FindPossiblePositionsSimple(int activePieceType, int heldPieceType, int[,] boardArray)
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

        public static double CalculateCost(int[,] boardArray, int[,] staticBoardArray, double[] heuristicWeights,
            bool backToBack, bool lastMoveIsSpin, ActivePiece activePiece, bool wasLastWallkickUsed, int comboCount)
        {
            // 1. calculate any line clears
            
            var GarbageAmount = TetrisUtil.ClearLines(ref boardArray, staticBoardArray, ref backToBack,
                lastMoveIsSpin, activePiece, wasLastWallkickUsed, ref comboCount);
            
            // 2. calculate aggregate height, bumpiness, pits and deepest well
            var AggregateHeight = 0;
            var Bumpiness = 0;
            var Pits = 0;
            var DeepestWell = 0;
            
            
            var height = 0;
            var bump = 0;
            
            var previousHeight = 0;
            var wasColumnBlank = true;

            for (var x = 0; x < 10; x++)
            {
                wasColumnBlank = true;
                for (var y = 0; y < 20; y++)
                {
                    if (boardArray[x, y] == 0) continue;
                    wasColumnBlank = false;
                    height = Math.Abs(y - 20);
                    
                    if (x != 0)
                    {
                        
                        bump = Math.Abs(previousHeight - height);
                        if (bump > DeepestWell) DeepestWell = bump;
                        Bumpiness += bump;
                    }

                    previousHeight = height;
                    AggregateHeight += height;
                    break;
                }

                if (!wasColumnBlank) continue;
                height = 0;
                
                bump = Math.Abs(previousHeight - height);
                if (bump > DeepestWell) DeepestWell = bump;
                previousHeight = height;
                Bumpiness += bump;
                Pits++;
            }
            
            // 3. calculate number of holes + columns with at least one hole
            var HolesNum = 0;
            var ColumnHolesNum = 0;
            var holesColumnCheck = false;

            for (var x = 0; x < 10; x++)
            {
                holesColumnCheck = false;
                for (var y = 0; y < 20; y++)
                {
                    if (boardArray[x, y] == 0) continue;
                    for (var j = y + 1; j < 20; j++)
                    {
                        if (boardArray[x, j] != 0) continue;
                        HolesNum++;
                        holesColumnCheck = true;
                    }

                    break;
                }

                if (holesColumnCheck) ColumnHolesNum++;
            }

            // 4. calculate transitions
            
            var VerticalTransitions = 0;
            var HorizontalTransitions = 0;
            var currentState = 0;
            var previousState = 0;
            
            for (var x = 0; x < 10; x++)
            {
                currentState = 0;
                previousState = 0;
                for (var y = 0; y < 20; y++)
                {
                    currentState = boardArray[x, y] == 0 ? 0 : 1;
                
                    if (y == 0) continue;

                    if (previousState == currentState) continue;
                    VerticalTransitions++;
                    previousState = currentState;
                    //Debug.WriteLine($"x: {x}, y: {y}");
                }
            }
            
            for (var y = 0; y < 20; y++)
            {
                currentState = 0;
                previousState = 0;
                for (var x = 0; x < 10; x++)
                {
                    currentState = boardArray[x, y] == 0 ? 0 : 1;
                
                    if (x == 0) continue;

                    if (previousState == currentState) continue;
                    HorizontalTransitions++;
                    previousState = currentState;
                    //Debug.WriteLine($"x: {x}, y: {y}");
                }
            }
            
            // output
            //Debug.WriteLine(
            //    $"Garbage Amount: {GarbageAmount}\nAggregate Height: {AggregateHeight}\nBumpiness: {Bumpiness}\nPits: {Pits}\nDeepest Well: {DeepestWell}\nNumber of holes: {HolesNum}\nNumber of holes in columns: {ColumnHolesNum}\nVertical transitions: {VerticalTransitions}\nHorizontal transitions: {HorizontalTransitions}");

            // var AggregateHeight = 0;
            // var GarbageAmount = 0;
            // var HolesNum = 0;
            // var ColumnHolesNum = 0;
            // var Bumpiness = 0;
            // var HorizontalTransitions = 0;
            // var VerticalTransitions = 0;
            // var Pits = 0;
            // var DeepestWell = 0;
            
            var cost = (heuristicWeights[0] * AggregateHeight) + (heuristicWeights[1] * GarbageAmount) +
                       (heuristicWeights[2] * HolesNum) + (heuristicWeights[3] * ColumnHolesNum) +
                       (heuristicWeights[4] * Bumpiness) + (heuristicWeights[5] * HorizontalTransitions) +
                       (heuristicWeights[6] * VerticalTransitions) + (heuristicWeights[7] * Pits) +
                       (heuristicWeights[8] * DeepestWell);
            
            return 0.5;
        }
        
        //check through move permutations to see if a position is valid
        public static string[] FindValidMove(int activePieceType, int[,] boardArray,
            PiecePosition start)
        {
            var pieceDictionary = new PieceDictionary();
            var validMoves = new List<int>();
            var activePieceTemp = new ActivePiece(new IntVector2(start.Position.X, start.Position.Y), start.Rotation, activePieceType, activePieceType == 2 ? 4 : 3);


            //if just a hard drop can do the rotation
            if (activePieceTemp.CanSeeRoof(boardArray))
            {
                return new[] {"888", activePieceTemp.CurrentLocation.X.ToString(), activePieceTemp.CurrentLocation.Y.ToString(), activePieceTemp.RotState.ToString()};
            }

            foreach (var permutation in _permutations.MovePermutationsList)
            {
                //reset position
                activePieceTemp.CurrentLocation = start.Position;
                activePieceTemp.RotState = start.Rotation;
                    
                for (var i = 0; i < MoveCheckAmount; i++)
                {
                    var newRotStateL = activePieceTemp.RotState == 3 ? 0 : activePieceTemp.RotState + 1;
                    var newRotStateR = activePieceTemp.RotState == 0 ? 3 : activePieceTemp.RotState - 1;
                    
                    switch (int.Parse(permutation.Substring(i, 1)))
                    {
                        case 0:
                            //check right
                            if (activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState,
                                    new IntVector2(1, 0))) activePieceTemp.CurrentLocation.X -= 1;
                            break;
                        case 1:
                            //check left
                            if (activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState, new IntVector2(-1, 0))) activePieceTemp.CurrentLocation.X += 1;
                            break;
                        case 2:
                            //check down
                            if (activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState, new IntVector2(0, -1))) activePieceTemp.CurrentLocation.Y -= 1;
                            break;
                        case 3:
                            //check rot R
                            for (var j = 0; j < 5; j++)
                            {
                                var wallkick = new IntVector2(pieceDictionary.GetWallKick(false, true, newRotStateR)[j, 0] * -1, pieceDictionary.GetWallKick(false, true, newRotStateR)[j, 1]);

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

                    if (!activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState, new IntVector2(0, 0)))
                        continue;
                    if (activePieceTemp.CanSeeRoof(boardArray))
                    {
                        //FORMAT: 0 - moves to perform AFTER dropping, 1 - position to drop to X, 1 - position to drop to Y, 1 - position to drop to ROTATION
                        return new[] {permutation[..(i + 1)], activePieceTemp.CurrentLocation.X.ToString(), activePieceTemp.CurrentLocation.Y.ToString(), activePieceTemp.RotState.ToString()};
                    }
                }
                //Debug.WriteLine($"permutation {permutation} ended up with pos {activePieceTemp.CurrentLocation.X.ToString()}, {activePieceTemp.CurrentLocation.Y.ToString()}  rotation {activePieceTemp.RotState.ToString()}");
            }
            
            

            return new[] {"999"};
        }
        
        public static IEnumerable<PossibleMove> FindPossiblePositionsAndPossibleMove(int activePieceType, int heldPieceType,
            int[,] boardArray, bool backToBack, bool lastMoveIsSpin, bool wasLastWallkickUsed, int comboCount)
        {
            var activePieceTemp = new ActivePiece(new IntVector2(4, 0), 0, activePieceType, activePieceType == 2 ? 4 : 3);
            var possiblePositions = new List<PossibleMove>();

           
            activePieceTemp.RotState = 0;

            if (activePieceType != 2 && activePieceType != 5)
            {
                for (var rotstate = 0; rotstate < 4; rotstate++)
                {
                    activePieceTemp.RotState = rotstate;
                    for (var i = -1; i <= 8; i++)
                    {
                        for (var j = 1; j <= 18; j++)
                        {
                            activePieceTemp.CurrentLocation = new IntVector2(i, j);
                            if (!activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState,
                                    new IntVector2(0, 0)))
                                continue;
                            if (!activePieceTemp.IsTouchingBlock(boardArray))
                                continue;

                            //Debug.WriteLine((i.ToString(), j.ToString()));
                            
                            var validMove = FindValidMove(activePieceType, boardArray,
                                new PiecePosition(new IntVector2(i, j), activePieceTemp.RotState));
                            
                            // if (i == 0 && j == 17 && activePieceTemp.RotState == 1)
                            // {
                            //     Debug.WriteLine($"yes: {validMove}");
                            // }
                            // Debug.WriteLine(validMove);
                            
                            if (validMove[0] != "999")
                            {
                                var cost = CalculateCost(boardArray, activePieceTemp.ReturnLockedInBoard(boardArray), new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                                    backToBack, lastMoveIsSpin, activePieceTemp, wasLastWallkickUsed, comboCount);

                                possiblePositions.Add(new PossibleMove(new PiecePosition(new IntVector2(i, j), activePieceTemp.RotState), new PiecePosition(new IntVector2(int.Parse(validMove[1]), int.Parse(validMove[2])), int.Parse(validMove[3])), validMove[0], cost));
                            }
                        }
                    }
                }
            }

            return possiblePositions;
        }
    }
}