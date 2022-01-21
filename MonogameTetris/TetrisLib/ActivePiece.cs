using System.Collections.Generic;

namespace MonogameTetris.TetrisLib
{
    public class ActivePiece
    {
        private readonly PieceDictionary _pieceDictionary = new PieceDictionary();
        private bool _wasLastWallkickUsed;
        public IntVector2 CurrentLocation;
        public InputLib InputLib = new InputLib();
        public int PieceType;
        public int RotState;
        public int SideLength;

        public ActivePiece(IntVector2 currentLocation, int rotState, int pieceType, int sideLength)
        {
            CurrentLocation = currentLocation;
            RotState = rotState;
            PieceType = pieceType;
            SideLength = sideLength;
        }

        public bool IsValidMove(int[,] boardArray, int newRotState, IntVector2 wallKick, bool useInputtedTet,
            int[,] tet)
        {
            //Console.Write(wallKick.x);
            //Console.Write(" ");
            //Console.Write(wallKick.y);
            //Console.Write("\n");

            var pieceShape = !useInputtedTet ? _pieceDictionary.GetTet(PieceType, newRotState) : tet;

            for (var i = 0; i < SideLength; i++)
            for (var j = 0; j < SideLength; j++)
                //if not empty tile
                if (pieceShape[i, j] != 0)
                {
                    var checkBlockX = CurrentLocation.X + wallKick.X + j;
                    var checkBlockY = CurrentLocation.Y + wallKick.Y + i;

                    //Console.Write(checkBlockX);
                    //Console.Write(" ");
                    //Console.Write(checkBlockY);
                    //Console.Write("\n");

                    if (checkBlockX > 9 || checkBlockX < 0) return false;

                    if (checkBlockY > 19 || checkBlockY < 0) return false;

                    if (boardArray[checkBlockX, checkBlockY] != 0) return false;
                }

            return true;
        }

        private IntVector2 CheckRotR(int rotState, int[,] boardArray)
        {
            //Calc next rot state
            var newRotState = rotState;
            if (RotState == 3)
                newRotState = 0;
            else
                newRotState++;

            switch (PieceType)
            {
                //IF I PIECE
                case 2:
                {
                    var wallKickData = _pieceDictionary.GetWallKick(true, true, RotState);

                    for (var i = 0; i < 5; i++)
                    {
                        //construct vector from wall kick data and iterate over
                        var wallKick = new IntVector2(wallKickData[i, 0], wallKickData[i, 1] * -1);

                        //check if wall kick is valid
                        if (IsValidMove(boardArray, newRotState, wallKick, false, null)) return wallKick;
                    }

                    return new IntVector2(10, 10);
                }
                //IF O PIECE
                case 5:
                    return new IntVector2(0, 0);
                default:
                {
                    var wallKickData = _pieceDictionary.GetWallKick(false, true, RotState);

                    for (var i = 0; i < 5; i++)
                    {
                        //construct vector from wall kick data and iterate over
                        var wallKick = new IntVector2(wallKickData[i, 0], wallKickData[i, 1] * -1);

                        //check if wall kick is valid
                        if (IsValidMove(boardArray, newRotState, wallKick, false, null))
                        {
                            if (i == 4) _wasLastWallkickUsed = true;
                            return wallKick;
                        }
                    }

                    _wasLastWallkickUsed = false;

                    return new IntVector2(10, 10);
                }
            }
        }

        private IntVector2 CheckRotL(int rotState, int[,] boardArray)
        {
            //Calc next rot state
            var newRotState = rotState;
            if (RotState == 0)
                newRotState = 3;
            else
                newRotState--;

            switch (PieceType)
            {
                //IF I PIECE
                case 2:
                {
                    var wallKickData = _pieceDictionary.GetWallKick(true, false, RotState);

                    for (var i = 0; i < 5; i++)
                    {
                        //construct vector from wall kick data and iterate over
                        var wallKick = new IntVector2(wallKickData[i, 0], wallKickData[i, 1] * -1);

                        //check if wall kick is valid
                        if (IsValidMove(boardArray, newRotState, wallKick, false, null)) return wallKick;
                    }

                    return new IntVector2(10, 10);
                }
                //IF O PIECE
                case 5:
                    return new IntVector2(0, 0);

                default:
                {
                    var wallKickData = _pieceDictionary.GetWallKick(false, false, RotState);

                    for (var i = 0; i < 5; i++)
                    {
                        //construct vector from wall kick data and iterate over
                        var wallKick = new IntVector2(wallKickData[i, 0], wallKickData[i, 1] * -1);

                        //check if wall kick is valid
                        if (!IsValidMove(boardArray, newRotState, wallKick, false, null)) continue;
                        if (i == 4) _wasLastWallkickUsed = true;
                        return wallKick;
                    }

                    _wasLastWallkickUsed = false;

                    return new IntVector2(10, 10);
                }
            }
        }

        //rotate 90 deg
        public bool[] IncreaseRotState(int[,] boardArray)
        {
            var checkRotReturnVal = CheckRotR(RotState, boardArray);
            //if invalid
            if (checkRotReturnVal.Equals(new IntVector2(10, 10))) return new[] {false, _wasLastWallkickUsed};

            //update position
            CurrentLocation.X += checkRotReturnVal.X;
            CurrentLocation.Y += checkRotReturnVal.Y;

            if (RotState == 3)
                RotState = 0;
            else
                RotState++;

            return new[] {true, _wasLastWallkickUsed};
        }

        //rotate -90 deg
        public bool[] DecreaseRotState(int[,] boardArray)
        {
            var checkRotReturnVal = CheckRotL(RotState, boardArray);
            if (checkRotReturnVal.Equals(new IntVector2(10, 10))) return new[] {false, _wasLastWallkickUsed};

            CurrentLocation.X += checkRotReturnVal.X;
            CurrentLocation.Y += checkRotReturnVal.Y;

            if (RotState == 0)
                RotState = 3;
            else
                RotState--;

            return new[] {true, _wasLastWallkickUsed};
        }

        public bool MoveRight(int[,] boardArray)
        {
            if (IsValidMove(boardArray, RotState, new IntVector2(1, 0), false, null)) CurrentLocation.X++;
            else return false;
            return true;
        }

        public bool MoveLeft(int[,] boardArray)
        {
            if (IsValidMove(boardArray, RotState, new IntVector2(-1, 0), false, null)) CurrentLocation.X--;
            else return false;
            return true;
        }

        public bool MoveDown(int[,] boardArray)
        {
            if (IsValidMove(boardArray, RotState, new IntVector2(0, 1), false, null)) CurrentLocation.Y++;
            else return false;
            return true;
        }

        public bool IsTouchingBlock(int[,] boardArray)
        {
            return !IsValidMove(boardArray, RotState, new IntVector2(0, 1), false, null);
        }

        public void HardDrop(int[,] boardArray)
        {
            while (true)
            {
                if (IsTouchingBlock(boardArray)) return;
                MoveDown(boardArray);
            }
        }

        public int[,] ReturnLockedInBoard(int[,] boardArray)
        {
            var boardArrayCopy = (int[,]) boardArray.Clone();
            var pieceShape = _pieceDictionary.GetTet(PieceType, RotState);
            for (var i = 0; i < SideLength; i++)
            for (var j = 0; j < SideLength; j++)
            {
                if (pieceShape[i, j] == 0) continue;
                boardArrayCopy[CurrentLocation.X + j, CurrentLocation.Y + i] = pieceShape[i, j];
            }

            return boardArrayCopy;
        }

        public void ResetPiece(int pieceTypeI, int sideLengthI)
        {
            CurrentLocation = new IntVector2(4, 0);
            SideLength = sideLengthI;
            PieceType = pieceTypeI;
            RotState = 0;
            _wasLastWallkickUsed = false;
        }

        public bool CanSeeRoof(int[,] boardArray, List<int[,]> tetData)
        {
            var pieceShape = tetData[RotState];
            int[] pieceShapeHeights = {-1, -1, -1, -1};

            for (var y = 0; y < SideLength; y++)
            for (var x = 0; x < SideLength; x++)
            {
                if (pieceShape[x, y] == 0) continue;
                pieceShapeHeights[y] = x;
                break;
            }

            for (var i = 0; i < SideLength; i++)
            for (var j = CurrentLocation.Y + pieceShapeHeights[i]; j > 1; j--)
            {
                if (pieceShapeHeights[i] == -1) continue;
                if (boardArray[CurrentLocation.X + i, j] != 0) return false;
            }

            return true;
        }
    }
}