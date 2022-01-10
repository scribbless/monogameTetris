namespace MonogameTetris.TetrisLib
{
    public class ActivePiece
    {
        private readonly PieceDictionary _pieceDictionary = new PieceDictionary();
        public IntVector2 CurrentLocation;
        public InputLib InputLib = new InputLib();
        public int pieceType;
        public int rotState;
        public int sideLength;

        public ActivePiece(IntVector2 CurrentLocation, int rotState, int pieceType, int sideLength)
        {
            this.CurrentLocation = CurrentLocation;
            this.rotState = rotState;
            this.pieceType = pieceType;
            this.sideLength = sideLength;
        }

        private bool IsValidMove(int[,] boardArray, int newRotState, IntVector2 wallKick)
        {
            //Console.Write(wallKick.x);
            //Console.Write(" ");
            //Console.Write(wallKick.y);
            //Console.Write("\n");

            var pieceShape = _pieceDictionary.GetTet(pieceType, newRotState);
            for (var i = 0; i < sideLength; i++)
            for (var j = 0; j < sideLength; j++)
                //if not empty tile
                if (pieceShape[i, j] != 0)
                {
                    var checkBlockX = CurrentLocation.x + wallKick.x + j;
                    var checkBlockY = CurrentLocation.y + wallKick.y + i;

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
            if (this.rotState == 3)
                newRotState = 0;
            else
                newRotState++;

            //IF I PIECE
            if (pieceType == 2)
            {
                var wallKickData = _pieceDictionary.GetWallKick(true, true, this.rotState);

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

            if (pieceType == 5)
            {
                return new IntVector2(0, 0);
            }
            //IF OTHER PIECE

            {
                var wallKickData = _pieceDictionary.GetWallKick(false, true, this.rotState);

                for (var i = 0; i < 5; i++)
                {
                    //contstruct vector from wall kick data and iterate over
                    var wallKick = new IntVector2(wallKickData[i, 0], wallKickData[i, 1] * -1);

                    //check if wall kick is valid
                    if (IsValidMove(boardArray, newRotState, wallKick)) return wallKick;
                }

                return new IntVector2(10, 10);
            }
            //get wall kick data for rotation and piece type
        }

        private IntVector2 CheckRotL(int rotState, int[,] boardArray)
        {
            //Calc next rot state
            var newRotState = rotState;
            if (this.rotState == 0)
                newRotState = 3;
            else
                newRotState--;

            //IF I PIECE
            if (pieceType == 2)
            {
                var wallKickData = _pieceDictionary.GetWallKick(true, false, this.rotState);

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

            if (pieceType == 5)
            {
                return new IntVector2(0, 0);
            }
            //IF OTHER PIECE

            {
                var wallKickData = _pieceDictionary.GetWallKick(false, false, this.rotState);

                for (var i = 0; i < 5; i++)
                {
                    //contstruct vector from wall kick data and iterate over
                    var wallKick = new IntVector2(wallKickData[i, 0], wallKickData[i, 1] * -1);

                    //check if wall kick is valid
                    if (IsValidMove(boardArray, newRotState, wallKick)) return wallKick;
                }

                return new IntVector2(10, 10);
            }
            //get wall kick data for rotation and piece type
        }

        //rotate 90 deg
        public bool IncreaseRotState(int[,] boardArray)
        {
            var checkRotReturnVal = CheckRotR(rotState, boardArray);
            //if invalid
            if (checkRotReturnVal.Equals(new IntVector2(10, 10))) return false;

            //update position
            CurrentLocation.x += checkRotReturnVal.x;
            CurrentLocation.y += checkRotReturnVal.y;

            if (rotState == 3)
                rotState = 0;
            else
                rotState++;

            return true;
        }

        //rotate -90 deg
        public bool DecreaseRotState(int[,] boardArray)
        {
            var checkRotReturnVal = CheckRotL(rotState, boardArray);
            if (checkRotReturnVal.Equals(new IntVector2(10, 10))) return false;

            CurrentLocation.x += checkRotReturnVal.x;
            CurrentLocation.y += checkRotReturnVal.y;

            if (rotState == 0)
                rotState = 3;
            else
                rotState--;

            return true;
        }

        public bool MoveRight(int[,] boardArray)
        {
            if (IsValidMove(boardArray, rotState, new IntVector2(1, 0))) CurrentLocation.x++;
            else return false;
            return true;
        }

        public bool MoveLeft(int[,] boardArray)
        {
            if (IsValidMove(boardArray, rotState, new IntVector2(-1, 0))) CurrentLocation.x--;
            else return false;
            return true;
        }

        public bool MoveDown(int[,] boardArray)
        {
            if (IsValidMove(boardArray, rotState, new IntVector2(0, 1))) CurrentLocation.y++;
            else return false;
            return true;
        }

        public bool IsTouchingBlock(int[,] boardArray)
        {
            if (IsValidMove(boardArray, rotState, new IntVector2(0, 1))) return false;

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
            var pieceShape = _pieceDictionary.GetTet(pieceType, rotState);
            for (var i = 0; i < sideLength; i++)
            for (var j = 0; j < sideLength; j++)
            {
                if (pieceShape[i, j] == 0) continue;
                boardArray[CurrentLocation.x + j, CurrentLocation.y + i] = pieceShape[i, j];
            }

            return boardArray;
        }

        public void ResetPiece(int pieceTypeI, int sideLengthI)
        {
            this.CurrentLocation = new IntVector2(4, 0);
            this.sideLength = sideLengthI;
            this.pieceType = pieceTypeI;
            this.rotState = 0;
        }
    }
}