using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MonogameTetris.TetrisLib
{
    public static class AiFunctions
    {
        private const int MoveCheckAmount = 3;
        private static readonly MovePermutations Permutations = new MovePermutations(MoveCheckAmount);

        //simple search for positions
/*
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
*/

        public static double CalculateCost(int[,] boardArray, int[,] staticBoardArray, double[] heuristicWeights,
            bool backToBack, bool lastMoveIsSpin, ActivePiece activePiece, bool wasLastWallkickUsed, int comboCount)
        {
            // 1. calculate any line clears

            var garbageAmount = TetrisUtil.ClearLines(ref boardArray, staticBoardArray, ref backToBack,
                lastMoveIsSpin, activePiece, wasLastWallkickUsed, ref comboCount);

            // 2. calculate aggregate height, bumpiness, pits and deepest well
            var aggregateHeight = 0;
            var bumpiness = 0;
            var pits = 0;
            var deepestWell = 0;


            var previousHeight = 0;

            for (var x = 0; x < 10; x++)
            {
                var wasColumnBlank = true;
                int height;
                int bump;
                for (var y = 0; y < 20; y++)
                {
                    if (boardArray[x, y] == 0) continue;
                    wasColumnBlank = false;
                    height = Math.Abs(y - 20);

                    if (x != 0)
                    {
                        bump = Math.Abs(previousHeight - height);
                        if (bump > deepestWell) deepestWell = bump;
                        bumpiness += bump;
                    }

                    previousHeight = height;
                    aggregateHeight += height;
                    break;
                }

                if (!wasColumnBlank) continue;
                height = 0;

                bump = Math.Abs(previousHeight - height);
                if (bump > deepestWell) deepestWell = bump;
                previousHeight = height;
                bumpiness += bump;
                pits++;
            }

            // 3. calculate number of holes + columns with at least one hole
            var holesNum = 0;
            var columnHolesNum = 0;

            for (var x = 0; x < 10; x++)
            {
                var holesColumnCheck = false;
                for (var y = 0; y < 20; y++)
                {
                    if (boardArray[x, y] == 0) continue;
                    for (var j = y + 1; j < 20; j++)
                    {
                        if (boardArray[x, j] != 0) continue;
                        holesNum++;
                        holesColumnCheck = true;
                    }

                    break;
                }

                if (holesColumnCheck) columnHolesNum++;
            }

            // 4. calculate transitions

            var verticalTransitions = 0;
            var horizontalTransitions = 0;
            int currentState;
            int previousState;

            for (var x = 0; x < 10; x++)
            {
                previousState = 0;
                for (var y = 0; y < 20; y++)
                {
                    currentState = boardArray[x, y] == 0 ? 0 : 1;

                    if (y == 0) continue;

                    if (previousState == currentState) continue;
                    verticalTransitions++;
                    previousState = currentState;
                    //Debug.WriteLine($"x: {x}, y: {y}");
                }
            }

            for (var y = 0; y < 20; y++)
            {
                previousState = 0;
                for (var x = 0; x < 10; x++)
                {
                    currentState = boardArray[x, y] == 0 ? 0 : 1;

                    if (x == 0) continue;

                    if (previousState == currentState) continue;
                    horizontalTransitions++;
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

            /*  0 = AggregateHeight
                1 = GarbageAmount
                2 = HolesNum
                3 = ColumnHolesNum
                4 = Bumpiness
                5 = HorizontalTransitions
                6 = VerticalTransitions
                7 = Pits
                8 = DeepestWell
             */

            var cost = heuristicWeights[0] * aggregateHeight + heuristicWeights[1] * garbageAmount +
                       heuristicWeights[2] * holesNum + heuristicWeights[3] * columnHolesNum +
                       heuristicWeights[4] * bumpiness + heuristicWeights[5] * horizontalTransitions +
                       heuristicWeights[6] * verticalTransitions + heuristicWeights[7] * pits +
                       heuristicWeights[8] * deepestWell;

            return cost;
        }

        //check through move permutations to see if a position is valid
        private static string[] FindValidMove(int[,] boardArray,
            PiecePosition start, ActivePiece activePieceTemp, List<int[,]> tetData)
        {
            var pieceDictionary = new PieceDictionary();
            //var activePieceTemp = new ActivePiece(new IntVector2(start.Position.X, start.Position.Y), start.Rotation, activePieceType, activePieceType == 2 ? 4 : 3);
            activePieceTemp.CurrentLocation = start.Position;
            activePieceTemp.RotState = start.Rotation;


            //if just a hard drop can do the rotation
            if (activePieceTemp.CanSeeRoof(boardArray, tetData))
                return new[]
                {
                    "888", activePieceTemp.CurrentLocation.X.ToString(), activePieceTemp.CurrentLocation.Y.ToString(),
                    activePieceTemp.RotState.ToString()
                };

            foreach (var permutation in Permutations.MovePermutationsList)
            {
                //reset position
                activePieceTemp.CurrentLocation = start.Position;
                activePieceTemp.RotState = start.Rotation;

                if (activePieceTemp.PieceType == 5)
                    if (permutation.Contains("3") || permutation.Contains("4"))
                        continue;

                for (var i = 0; i < MoveCheckAmount; i++)
                {
                    var newRotStateL = activePieceTemp.RotState == 3 ? 0 : activePieceTemp.RotState + 1;
                    var newRotStateR = activePieceTemp.RotState == 0 ? 3 : activePieceTemp.RotState - 1;

                    switch (int.Parse(permutation.Substring(i, 1)))
                    {
                        case 0:
                            //check right
                            if (activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState,
                                    new IntVector2(1, 0), true, tetData[activePieceTemp.RotState]))
                                activePieceTemp.CurrentLocation.X -= 1;
                            break;
                        case 1:
                            //check left
                            if (activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState, new IntVector2(-1, 0),
                                    true, tetData[activePieceTemp.RotState])) activePieceTemp.CurrentLocation.X += 1;
                            break;
                        case 2:
                            //check down
                            if (activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState, new IntVector2(0, -1),
                                    true, tetData[activePieceTemp.RotState])) activePieceTemp.CurrentLocation.Y -= 1;
                            break;
                        case 3:
                            //check rot R
                            for (var j = 0; j < 5; j++)
                            {
                                var wallkick =
                                    new IntVector2(pieceDictionary.GetWallKick(false, true, newRotStateR)[j, 0] * -1,
                                        pieceDictionary.GetWallKick(false, true, newRotStateR)[j, 1]);

                                if (!activePieceTemp.IsValidMove(boardArray, newRotStateR, wallkick, true,
                                        tetData[activePieceTemp.RotState])) continue;
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
                                var wallkick =
                                    new IntVector2(pieceDictionary.GetWallKick(false, false, newRotStateL)[j, 0] * -1,
                                        pieceDictionary.GetWallKick(false, false, newRotStateL)[j, 1] * -1);

                                if (!activePieceTemp.IsValidMove(boardArray, newRotStateL, wallkick, true,
                                        tetData[activePieceTemp.RotState])) continue;
                                activePieceTemp.CurrentLocation.X += wallkick.X;
                                activePieceTemp.CurrentLocation.Y += wallkick.Y;
                                activePieceTemp.RotState = newRotStateL;
                                break;
                            }

                            break;
                    }

                    if (!activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState, new IntVector2(0, 0), true,
                            tetData[activePieceTemp.RotState]))
                        continue;
                    if (activePieceTemp.CanSeeRoof(boardArray, tetData))
                        //FORMAT: 0 - moves to perform AFTER dropping, 1 - position to drop to X, 1 - position to drop to Y, 1 - position to drop to ROTATION
                        return new[]
                        {
                            permutation[..(i + 1)], activePieceTemp.CurrentLocation.X.ToString(),
                            activePieceTemp.CurrentLocation.Y.ToString(), activePieceTemp.RotState.ToString()
                        };
                }
                //Debug.WriteLine($"permutation {permutation} ended up with pos {activePieceTemp.CurrentLocation.X.ToString()}, {activePieceTemp.CurrentLocation.Y.ToString()}  rotation {activePieceTemp.RotState.ToString()}");
            }


            return new[] {"999"};
        }

        public static PossibleMove FindBestMoveAndPath(int activePieceType,
            int[,] boardArray, bool backToBack, bool lastMoveIsSpin, bool wasLastWallkickUsed, int comboCount,
            double[] heuristicWeights)
        {
            var activePieceTemp =
                new ActivePiece(new IntVector2(4, 0), 0, activePieceType, activePieceType == 2 ? 4 : 3);
            var possiblePositions = new List<PossibleMove>();
            var pieceDictionary = new PieceDictionary();

            var tetData = new List<int[,]>
            {
                pieceDictionary.GetTet(activePieceTemp.PieceType, 0),
                pieceDictionary.GetTet(activePieceTemp.PieceType, 1),
                pieceDictionary.GetTet(activePieceTemp.PieceType, 2),
                pieceDictionary.GetTet(activePieceTemp.PieceType, 3)
            };


            activePieceTemp.RotState = 0;

            if (activePieceType != 2 && activePieceType != 5)
            {
                for (var rotstate = 0; rotstate < 4; rotstate++)
                {
                    activePieceTemp.RotState = rotstate;
                    for (var i = -1; i <= 8; i++)
                    for (var j = 18; j > 1; j--)
                    {
                        activePieceTemp.CurrentLocation.X = i;
                        activePieceTemp.CurrentLocation.Y = j;
                        activePieceTemp.RotState = rotstate;
                        //boardArrayCopy = (int[,]) boardArray.Clone();

                        if (!activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState,
                                new IntVector2(0, 0), true, tetData[activePieceTemp.RotState]))
                            continue;

                        //activePieceTemp.HardDrop(boardArray);

                        if (!activePieceTemp.IsTouchingBlock(boardArray))
                            continue;

                        //Debug.WriteLine((i.ToString(), j.ToString()));
                        //Debug.WriteLine((activePieceTemp.CurrentLocation.X.ToString(), activePieceTemp.CurrentLocation.Y.ToString(), rotstate.ToString()));


                        var validMove = FindValidMove(boardArray,
                            new PiecePosition(new IntVector2(i, j), activePieceTemp.RotState), activePieceTemp,
                            tetData);


                        // if (i == 0 && j == 17 && activePieceTemp.RotState == 1)
                        // {
                        //     Debug.WriteLine($"yes: {validMove}");
                        // }
                        // Debug.WriteLine(validMove);

                        if (validMove[0] == "999") continue;
                        var cost = CalculateCost(activePieceTemp.ReturnLockedInBoard(boardArray), boardArray,
                            heuristicWeights,
                            backToBack, lastMoveIsSpin, activePieceTemp, wasLastWallkickUsed, comboCount);

                        possiblePositions.Add(new PossibleMove(
                            new PiecePosition(new IntVector2(i, j), activePieceTemp.RotState),
                            new PiecePosition(new IntVector2(int.Parse(validMove[1]), int.Parse(validMove[2])),
                                int.Parse(validMove[3])), validMove[0], cost));
                        //Debug.WriteLine("cosmo");
                    }
                }
            }
            else if (activePieceType == 2)
            {
                for (var rotstate = 0; rotstate < 4; rotstate++)
                {
                    activePieceTemp.RotState = rotstate;
                    for (var i = -2; i <= 8; i++)
                    for (var j = 18; j > 1; j--)
                    {
                        activePieceTemp.CurrentLocation.X = i;
                        activePieceTemp.CurrentLocation.Y = j;
                        activePieceTemp.RotState = rotstate;
                        //boardArrayCopy = (int[,]) boardArray.Clone();

                        if (!activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState,
                                new IntVector2(0, 0), true, tetData[activePieceTemp.RotState]))
                            continue;

                        //activePieceTemp.HardDrop(boardArray);

                        if (!activePieceTemp.IsTouchingBlock(boardArray))
                            continue;

                        //Debug.WriteLine((i.ToString(), j.ToString()));
                        //Debug.WriteLine((activePieceTemp.CurrentLocation.X.ToString(), activePieceTemp.CurrentLocation.Y.ToString(), rotstate.ToString()));


                        var validMove = FindValidMove(boardArray,
                            new PiecePosition(new IntVector2(i, j), activePieceTemp.RotState), activePieceTemp,
                            tetData);


                        // if (i == 0 && j == 17 && activePieceTemp.RotState == 1)
                        // {
                        //     Debug.WriteLine($"yes: {validMove}");
                        // }
                        // Debug.WriteLine(validMove);

                        if (validMove[0] == "999") continue;
                        var cost = CalculateCost(activePieceTemp.ReturnLockedInBoard(boardArray), boardArray,
                            heuristicWeights,
                            backToBack, lastMoveIsSpin, activePieceTemp, wasLastWallkickUsed, comboCount);

                        possiblePositions.Add(new PossibleMove(
                            new PiecePosition(new IntVector2(i, j), activePieceTemp.RotState),
                            new PiecePosition(new IntVector2(int.Parse(validMove[1]), int.Parse(validMove[2])),
                                int.Parse(validMove[3])), validMove[0], cost));
                        //Debug.WriteLine("cosmo");
                    }
                }
            }
            else
            {
                activePieceTemp.RotState = 0;
                for (var i = -1; i <= 8; i++)
                for (var j = 18; j > 1; j--)
                {
                    activePieceTemp.CurrentLocation.X = i;
                    activePieceTemp.CurrentLocation.Y = j;
                    activePieceTemp.RotState = 0;
                    //boardArrayCopy = (int[,]) boardArray.Clone();

                    if (!activePieceTemp.IsValidMove(boardArray, activePieceTemp.RotState,
                            new IntVector2(0, 0), true, tetData[activePieceTemp.RotState]))
                        continue;

                    //activePieceTemp.HardDrop(boardArray);

                    if (!activePieceTemp.IsTouchingBlock(boardArray))
                        continue;

                    //Debug.WriteLine((i.ToString(), j.ToString()));
                    //Debug.WriteLine((activePieceTemp.CurrentLocation.X.ToString(), activePieceTemp.CurrentLocation.Y.ToString(), rotstate.ToString()));


                    var validMove = FindValidMove(boardArray,
                        new PiecePosition(new IntVector2(i, j), activePieceTemp.RotState), activePieceTemp, tetData);


                    // if (i == 0 && j == 17 && activePieceTemp.RotState == 1)
                    // {
                    //     Debug.WriteLine($"yes: {validMove}");
                    // }
                    // Debug.WriteLine(validMove);

                    if (validMove[0] == "999") continue;
                    var cost = CalculateCost(activePieceTemp.ReturnLockedInBoard(boardArray), boardArray,
                        heuristicWeights,
                        backToBack, lastMoveIsSpin, activePieceTemp, wasLastWallkickUsed, comboCount);

                    possiblePositions.Add(new PossibleMove(
                        new PiecePosition(new IntVector2(i, j), activePieceTemp.RotState),
                        new PiecePosition(new IntVector2(int.Parse(validMove[1]), int.Parse(validMove[2])),
                            int.Parse(validMove[3])), validMove[0], cost));
                    //Debug.WriteLine("cosmo");
                }
            }

            double highestCost = -10000000;
            var bestMove = new PossibleMove();
            foreach (var position in possiblePositions.Where(position => position.cost > highestCost))
            {
                //if (activePieceTemp.PieceType == 2)
                    //Debug.WriteLine(
                    //    $"move cost: {highestCost}\nmove x: {bestMove.endPosition.Position.X}, y: {bestMove.endPosition.Position.Y}, rotation: {bestMove.endPosition.Rotation}");

                highestCost = position.cost;
                bestMove = position;
            }

            //Debug.WriteLine($"best move cost: {highestCost}\nbest move x: {bestMove.endPosition.Position.X}, y: {bestMove.endPosition.Position.Y}, rotation: {bestMove.endPosition.Rotation}");

            return bestMove;
        }
    }
}