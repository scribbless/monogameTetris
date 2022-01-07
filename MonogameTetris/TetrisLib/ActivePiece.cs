using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Input;

namespace MonogameTetris.TetrisLib
{
    public class ActivePiece
    {
            public IntVector2 CurrentLocation;
            public int rotState;
            public int pieceType;
            public int sideLength;
            private PieceDictionary _pieceDictionary = new PieceDictionary();
            public InputLib InputLib = new InputLib();

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
                
                int[,] pieceShape = _pieceDictionary.GetTet(pieceType, newRotState);
                for (int i = 0; i < sideLength; i++)
                {
                    for (int j = 0; j < sideLength; j++)
                    {
                        //if not empty tile
                        if (pieceShape[i, j] != 0)
                        {
                            int checkBlockX = CurrentLocation.x + wallKick.x + j;
                            int checkBlockY = CurrentLocation.y + wallKick.y + i;
                            
                            //Console.Write(checkBlockX);
                            //Console.Write(" ");
                            //Console.Write(checkBlockY);
                            //Console.Write("\n");
                            
                            if (checkBlockX > 9 || checkBlockX < 0)
                            {
                                return false;
                            }
                            
                            if (checkBlockY > 19 || checkBlockY < 0)
                            {
                                return false;
                            }

                            if (boardArray[checkBlockX, checkBlockY] != 0)
                            {
                                return false;
                            }
                            
                        }
                    }
                }

                return true;
            }

            private IntVector2 CheckRotR(int rotState, int[,] boardArray)
            {
                //Calc next rot state
                int newRotState = rotState;
                if (this.rotState == 3)
                {
                    newRotState = 0;
                }
                else
                {
                    newRotState++;
                }

                //IF I PIECE
                if (pieceType == 2)
                {
                    int[,] wallKickData = _pieceDictionary.GetWallKick(true, true, this.rotState);
                        
                    for (int i = 0; i < 5; i++)
                    {
                        //contstruct vector from wall kick data and iterate over
                        IntVector2 wallKick = new IntVector2(wallKickData[i, 0], wallKickData[i, 1] * -1);
                            
                        //check if wall kick is valid
                        if (IsValidMove(boardArray, newRotState, wallKick))
                        {
                            return wallKick;
                        }
                    }
                    return new IntVector2(10, 10);
                }
                //IF O PIECE
                else if (pieceType == 5)
                {
                    return new IntVector2(0, 0);
                }
                //IF OTHER PIECE
                else
                {
                    int[,] wallKickData = _pieceDictionary.GetWallKick(false, true, this.rotState);
                        
                    for (int i = 0; i < 5; i++)
                    {
                        //contstruct vector from wall kick data and iterate over
                        IntVector2 wallKick = new IntVector2(wallKickData[i, 0], wallKickData[i, 1] * -1);
                            
                        //check if wall kick is valid
                        if (IsValidMove(boardArray, newRotState, wallKick))
                        {
                            return wallKick;
                        }
                    }
                    return new IntVector2(10, 10);
                }
                //get wall kick data for rotation and piece type
            }
            
            private IntVector2 CheckRotL(int rotState, int[,] boardArray)
            {
                //Calc next rot state
                int newRotState = rotState;
                if (this.rotState == 0)
                {
                    newRotState = 3;
                }
                else
                {
                    newRotState--;
                }

                //IF I PIECE
                if (pieceType == 2)
                {
                    int[,] wallKickData = _pieceDictionary.GetWallKick(true, false, this.rotState);
                        
                    for (int i = 0; i < 5; i++)
                    {
                        //contstruct vector from wall kick data and iterate over
                        IntVector2 wallKick = new IntVector2(wallKickData[i, 0], wallKickData[i, 1] * -1);
                            
                        //check if wall kick is valid
                        if (IsValidMove(boardArray, newRotState, wallKick))
                        {
                            return wallKick;
                        }
                    }
                    return new IntVector2(10, 10);
                }
                //IF O PIECE
                else if (pieceType == 5)
                {
                    return new IntVector2(0, 0);
                }
                //IF OTHER PIECE
                else
                {
                    int[,] wallKickData = _pieceDictionary.GetWallKick(false, false, this.rotState);
                        
                    for (int i = 0; i < 5; i++)
                    {
                        //contstruct vector from wall kick data and iterate over
                        IntVector2 wallKick = new IntVector2(wallKickData[i, 0], wallKickData[i, 1] * -1);
                            
                        //check if wall kick is valid
                        if (IsValidMove(boardArray, newRotState, wallKick))
                        {
                            return wallKick;
                        }
                    }
                    return new IntVector2(10, 10);
                }
                //get wall kick data for rotation and piece type
            }
            
            //rotate 90 deg
            public void IncreaseRotState(int[,] boardArray)
            {
                IntVector2 checkRotReturnVal = CheckRotR(this.rotState, boardArray);
                //if invalid
                if (checkRotReturnVal.Equals(new IntVector2(10, 10)))
                {
                    return;
                } else
                {
                    //update position
                    this.CurrentLocation.x += checkRotReturnVal.x;
                    this.CurrentLocation.y += checkRotReturnVal.y;

                    if (this.rotState == 3)
                        this.rotState = 0;
                    else
                        this.rotState++;
                }
            }
            
            //rotate -90 deg
            public void DecreaseRotState(int[,] boardArray)
            {
                IntVector2 checkRotReturnVal = CheckRotL(this.rotState, boardArray);
                if (checkRotReturnVal.Equals(new IntVector2(10, 10)))
                {
                    return;
                } 
                else
                {
                    this.CurrentLocation.x += checkRotReturnVal.x;
                    this.CurrentLocation.y += checkRotReturnVal.y;

                    if (this.rotState == 0)
                        this.rotState = 3;
                    else
                        this.rotState--;
                }
            }

            public void MoveRight(int[,] boardArray)
            {
                if (IsValidMove(boardArray, rotState, new IntVector2(1, 0)))
                {
                    this.CurrentLocation.x++;
                }
            }
            
            public void MoveLeft(int[,] boardArray)
            {
                if (IsValidMove(boardArray, rotState, new IntVector2(-1, 0)))
                {
                    this.CurrentLocation.x--;
                }
            }

            public void MoveDown(int[,] boardArray)
            {
                if (IsValidMove(boardArray, rotState, new IntVector2(0, 1)))
                {
                    this.CurrentLocation.y++;
                }
            }

            public bool IsTouchingBlock(int[,] boardArray)
            {
                if (IsValidMove(boardArray, rotState, new IntVector2(0, 1)))
                {
                    return false;
                }
                
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
                int[,] pieceShape = _pieceDictionary.GetTet(this.pieceType, this.rotState);
                for (int i = 0; i < this.sideLength; i++)
                {
                    for (int j = 0; j < this.sideLength; j++)
                    {
                        if (pieceShape[i, j] == 0)
                        {
                            continue;
                        }
                        boardArray[this.CurrentLocation.x + j, this.CurrentLocation.y + i] = pieceShape[i, j];
                    }
                }
                return boardArray;
            }

            public void ResetPiece(int pieceTypeI, int sideLengthI)
            {
                this.CurrentLocation = new IntVector2(4, 0);
                this.pieceType = pieceTypeI;
                this.rotState = 0;
            }
    }
}