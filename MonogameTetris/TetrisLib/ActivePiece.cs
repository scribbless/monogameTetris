namespace MonogameTetris.TetrisLib
{
    public class ActivePiece
    {
        private readonly PieceDictionary _pieceDictionary = new PieceDictionary();
        public IntVector2 CurrentLocation;
        public InputLib InputLib = new InputLib();
        public int PieceType;
        public int RotState;
        public int SideLength;
        private bool _wasLastWallkickUsed = false;

        public ActivePiece(IntVector2 currentLocation, int rotState, int pieceType, int sideLength)
        {
            this.CurrentLocation = currentLocation;
            this.RotState = rotState;
            this.PieceType = pieceType;
            this.SideLength = sideLength;
        }

        private bool IsValidMove(int[,] boardArray, int newRotState, IntVector2 wallKick)
        {
            //Console.Write(wallKick.x);
            //Console.Write(" ");
            //Console.Write(wallKick.y);
            //Console.Write("\n");

            var pieceShape = _pieceDictionary.GetTet(PieceType, newRotState);
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
            if (this.RotState == 3)
                newRotState = 0;
            else
                newRotState++;

            //IF I PIECE
            if (PieceType == 2)
            {
                var wallKickData = _pieceDictionary.GetWallKick(true, true, this.RotState);

                for (var i = 0; i < 5; i++)
                {
                    //contstruct vector from wall kick data and iterate over
                    var wallKick = new IntVector2(wallKickData[i, 0], wallKickData[i, 1] * -1);

                    //check if wall kick is valid
                    if (IsValidMove(boardArray, newRotState, wallKick)) return wallKick;
                }

                return new IntVector2(10, 10);
            }
            //IF O PIECE

            if (PieceType == 5)
            {
                return new IntVector2(0, 0);
            }
            //IF OTHER PIECE
            
            {
                var wallKickData = _pieceDictionary.GetWallKick(false, true, this.RotState);

                for (var i = 0; i < 5; i++)
                {
                    //contstruct vector from wall kick data and iterate over
                    var wallKick = new IntVector2(wallKickData[i, 0], wallKickData[i, 1] * -1);

                    //check if wall kick is valid
                    if (IsValidMove(boardArray, newRotState, wallKick))
                    {
                        if (i == 4) _wasLastWallkickUsed = true;
                        return wallKick;
                    }
                }

                _wasLastWallkickUsed = false;

                return new IntVector2(10, 10);
            }
            //get wall kick data for rotation and piece type
        }

        private IntVector2 CheckRotL(int rotState, int[,] boardArray)
        {
            //Calc next rot state
            var newRotState = rotState;
            if (this.RotState == 0)
                newRotState = 3;
            else
                newRotState--;

            //IF I PIECE
            if (PieceType == 2)
            {
                var wallKickData = _pieceDictionary.GetWallKick(true, false, this.RotState);

                for (var i = 0; i < 5; i++)
                {
                    //contstruct vector from wall kick data and iterate over
                    var wallKick = new IntVector2(wallKickData[i, 0], wallKickData[i, 1] * -1);

                    //check if wall kick is valid
                    if (IsValidMove(boardArray, newRotState, wallKick)) return wallKick;
                }

                return new IntVector2(10, 10);
            }
            //IF O PIECE

            if (PieceType == 5)
            {
                return new IntVector2(0, 0);
            }
            //IF OTHER PIECE

            {
                var wallKickData = _pieceDictionary.GetWallKick(false, false, this.RotState);

                for (var i = 0; i < 5; i++)
                {
                    //contstruct vector from wall kick data and iterate over
                    var wallKick = new IntVector2(wallKickData[i, 0], wallKickData[i, 1] * -1);

                    //check if wall kick is valid
                    if (IsValidMove(boardArray, newRotState, wallKick))
                    {
                        if (i == 4) _wasLastWallkickUsed = true;
                        return wallKick;
                    }
                }

                _wasLastWallkickUsed = false;

                return new IntVector2(10, 10);
            }
            //get wall kick data for rotation and piece type
        }

        //rotate 90 deg
        public bool[] IncreaseRotState(int[,] boardArray)
        {
            var checkRotReturnVal = CheckRotR(RotState, boardArray);
            //if invalid
            if (checkRotReturnVal.Equals(new IntVector2(10, 10))) return new [] {false, _wasLastWallkickUsed};

            //update position
            CurrentLocation.X += checkRotReturnVal.X;
            CurrentLocation.Y += checkRotReturnVal.Y;

            if (RotState == 3)
                RotState = 0;
            else
                RotState++;

            return new [] {true, _wasLastWallkickUsed};
        }

        //rotate -90 deg
        public bool[] DecreaseRotState(int[,] boardArray)
        {
            var checkRotReturnVal = CheckRotL(RotState, boardArray);
            if (checkRotReturnVal.Equals(new IntVector2(10, 10))) return new [] {false, _wasLastWallkickUsed};;

            CurrentLocation.X += checkRotReturnVal.X;
            CurrentLocation.Y += checkRotReturnVal.Y;

            if (RotState == 0)
                RotState = 3;
            else
                RotState--;

            return new [] {true, _wasLastWallkickUsed};;
        }

        public bool MoveRight(int[,] boardArray)
        {
            if (IsValidMove(boardArray, RotState, new IntVector2(1, 0))) CurrentLocation.X++;
            else return false;
            return true;
        }

        public bool MoveLeft(int[,] boardArray)
        {
            if (IsValidMove(boardArray, RotState, new IntVector2(-1, 0))) CurrentLocation.X--;
            else return false;
            return true;
        }

        public bool MoveDown(int[,] boardArray)
        {
            if (IsValidMove(boardArray, RotState, new IntVector2(0, 1))) CurrentLocation.Y++;
            else return false;
            return true;
        }

        public bool IsTouchingBlock(int[,] boardArray)
        {
            if (IsValidMove(boardArray, RotState, new IntVector2(0, 1))) return false;

            return true;
        }

        public void HardDrop(int[,] boardArray)
        {
            if (IsTouchingBlock(boardArray))
                return;
            MoveDown(boardArray);
            HardDrop(boardArray);
        }

        public int[,] ReturnLockedInBoard(int[,] boardArray)
        {
            var pieceShape = _pieceDictionary.GetTet(PieceType, RotState);
            for (var i = 0; i < SideLength; i++)
            for (var j = 0; j < SideLength; j++)
            {
                if (pieceShape[i, j] == 0) continue;
                boardArray[CurrentLocation.X + j, CurrentLocation.Y + i] = pieceShape[i, j];
            }

            return boardArray;
        }

        public void ResetPiece(int pieceTypeI, int sideLengthI)
        {
            this.CurrentLocation = new IntVector2(4, 0);
            this.SideLength = sideLengthI;
            this.PieceType = pieceTypeI;
            this.RotState = 0;
            this._wasLastWallkickUsed = false;
        }
    }
}