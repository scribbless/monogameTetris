using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonogameTetris.TetrisLib
{
    public class TetrisUtil
    {
        private readonly PieceDictionary _pieceDictionary = new PieceDictionary();

        //render board function
        public static void DrawBoard(int sizeX, int sizeY, int posX, int posY, int tileSize, int boardPadding,
            SpriteBatch spriteBatch, Dictionary<int, Color> colorDict, int[,] boardArray, Texture2D squareTexture)
        {
            for (var i = 0; i < sizeX; i++)
            for (var j = 0; j < sizeY; j++)
            {
                var rectPos = new Vector2(posX + (i + 1) * tileSize + tileSize + boardPadding * (i + 1),
                    posY + (j + 1) * tileSize + tileSize + boardPadding * (j + 1));
                spriteBatch.Draw(squareTexture, rectPos, colorDict[boardArray[i, j]]);
            }
        }

        //return board array with piece in it
        public int[,] ReturnBoardWithPiece(int[,] boardArray1, ActivePiece piece)
        {
            var pieceShape = _pieceDictionary.GetTet(piece.PieceType, piece.RotState);
            for (var i = 0; i < piece.SideLength; i++)
            for (var j = 0; j < piece.SideLength; j++)
            {
                if (pieceShape[i, j] == 0) continue;
                boardArray1[piece.CurrentLocation.X + j, piece.CurrentLocation.Y + i] = pieceShape[i, j];
            }

            return boardArray1;
        }

        public int[,] ReturnBoardWithGhostPiece(int[,] boardArray1, ActivePiece piece)
        {
            var pieceShape = _pieceDictionary.GetTet(piece.PieceType, piece.RotState);
            for (var i = 0; i < piece.SideLength; i++)
            for (var j = 0; j < piece.SideLength; j++)
            {
                if (pieceShape[i, j] == 0) continue;
                boardArray1[piece.CurrentLocation.X + j, piece.CurrentLocation.Y + i] = pieceShape[i, j] * 10;
            }

            return boardArray1;
        }

        //check if any lines are full
        private static List<int> CheckLines(int[,] boardArray)
        {
            var lines = new List<int>();

            for (var i = 0; i < 20; i++)
            {
                var lineCheck = true;
                for (var j = 0; j < 10; j++)
                    if (boardArray[j, i] == 0)
                    {
                        lineCheck = false;
                        break;
                    }

                if (lineCheck) lines.Add(i);
            }

            return lines;
        }

        //remove row from board
        private static int[,] ClearRow(int[,] boardArray, int row)
        {
            var blankArray = new int[10, 20];

            for (var i = 0; i < row; i++)
            for (var j = 0; j < 10; j++)
                blankArray[j, i + 1] = boardArray[j, i];

            for (var i = row + 1; i < 20; i++)
            for (var j = 0; j < 10; j++)
                blankArray[j, i] = boardArray[j, i];

            return blankArray;
        }

        public static int ClearLines(ref int[,] boardArray, int[,] staticBoardArray, ref bool backToBack,
            bool lastMoveIsSpin,
            ActivePiece activePiece, bool wasLastWallkickUsed, ref int comboCount)
        {
            var lines = CheckLines(boardArray);

            //0 = no tspin 1 = tspin mini 2 = tspin
            var spinType = 0;

            //
            var garbageSent = 0;

            //check for t spin
            if (activePiece.PieceType == 7)
                if (lastMoveIsSpin)
                {
                    var cornerCount = 0;

                    if (activePiece.CurrentLocation.X + 0 > 0 && activePiece.CurrentLocation.Y + 0 > 0)
                    {
                        if (staticBoardArray[activePiece.CurrentLocation.X + 0, activePiece.CurrentLocation.Y + 0] != 0)
                            cornerCount++;
                    }
                    else
                    {
                        cornerCount++;
                    }

                    if (activePiece.CurrentLocation.X + 2 < 10 && activePiece.CurrentLocation.Y + 0 > 0)
                    {
                        if (staticBoardArray[activePiece.CurrentLocation.X + 2, activePiece.CurrentLocation.Y + 0] != 0)
                            cornerCount++;
                    }
                    else
                    {
                        cornerCount++;
                    }

                    if (activePiece.CurrentLocation.X + 0 > 0 && activePiece.CurrentLocation.Y + 2 < 20)
                    {
                        if (staticBoardArray[activePiece.CurrentLocation.X + 0, activePiece.CurrentLocation.Y + 2] != 0)
                            cornerCount++;
                    }
                    else
                    {
                        cornerCount++;
                    }

                    if (activePiece.CurrentLocation.X + 2 < 10 && activePiece.CurrentLocation.Y + 2 < 20)
                    {
                        if (staticBoardArray[activePiece.CurrentLocation.X + 2, activePiece.CurrentLocation.Y + 2] != 0)
                            cornerCount++;
                    }
                    else
                    {
                        cornerCount++;
                    }


                    if (cornerCount >= 3)
                        switch (activePiece.RotState)
                        {
                            case 0:
                                if (staticBoardArray[activePiece.CurrentLocation.X + 0,
                                        activePiece.CurrentLocation.Y + 0] != 0 &&
                                    staticBoardArray[activePiece.CurrentLocation.X + 2,
                                        activePiece.CurrentLocation.Y + 0] != 0)
                                    spinType = 2;
                                else
                                    spinType = wasLastWallkickUsed ? 2 : 1;

                                break;
                            case 1:
                                if (staticBoardArray[activePiece.CurrentLocation.X + 2,
                                        activePiece.CurrentLocation.Y + 0] != 0 &&
                                    staticBoardArray[activePiece.CurrentLocation.X + 2,
                                        activePiece.CurrentLocation.Y + 2] != 0)
                                    spinType = 2;
                                else
                                    spinType = wasLastWallkickUsed ? 2 : 1;

                                break;
                            case 2:
                                if (staticBoardArray[activePiece.CurrentLocation.X + 0,
                                        activePiece.CurrentLocation.Y + 2] != 0 &&
                                    staticBoardArray[activePiece.CurrentLocation.X + 2,
                                        activePiece.CurrentLocation.Y + 2] != 0)
                                    spinType = 2;
                                else
                                    spinType = wasLastWallkickUsed ? 2 : 1;

                                break;
                            case 3:
                                if (staticBoardArray[activePiece.CurrentLocation.X + 0,
                                        activePiece.CurrentLocation.Y + 0] != 0 &&
                                    staticBoardArray[activePiece.CurrentLocation.X + 0,
                                        activePiece.CurrentLocation.Y + 2] != 0)
                                    spinType = 2;
                                else
                                    spinType = wasLastWallkickUsed ? 2 : 1;

                                break;
                        }
                }
            
            if (spinType == 0)
            {
                backToBack = false;
            }

            if (backToBack && spinType != 0) garbageSent += 2;

            //conditions for garbage sent
            //if normal tspin
            if (spinType == 2)
            {
                garbageSent = lines.Count switch
                {
                    1 => 2,
                    2 => 4,
                    3 => 6,
                    _ => garbageSent
                };
            }
            //if not tspin
            else
            {
                if (lines.Count >= 2)
                    switch (lines.Count)
                    {
                        case 2:
                            garbageSent = 1;
                            break;
                        case 3:
                            garbageSent = 2;
                            break;
                        case 4:
                            garbageSent = 4;
                            if (backToBack) garbageSent += 2;
                            backToBack = true;
                            break;
                    }
            }

            if (lines.Count != 0)
            {
                comboCount++;

                garbageSent += comboCount switch
                {
                    0 => 0,
                    1 => 0,
                    2 => 1,
                    3 => 1,
                    4 => 2,
                    5 => 2,
                    6 => 3,
                    7 => 3,
                    8 => 4,
                    9 => 4,
                    10 => 4,
                    11 => 4,
                    12 => 4,
                    _ => 5
                };
            }
            else comboCount = -1;

                //clear lines
            boardArray = lines.Aggregate(boardArray, ClearRow);

            //check for perfect clear
            if (boardArray.Cast<int>().Sum() != 0) return garbageSent;

            garbageSent = 10;
            if (backToBack) garbageSent += 2;
            backToBack = true;

            return garbageSent;
        }

        //draw queue
        public static void DrawQueue(List<int> queue, Vector2 position, int tileSize, int boardPadding,
            SpriteBatch spriteBatch, Dictionary<int, Color> colorDict, Texture2D squareTexture,
            PieceDictionary pieceDictionary)
        {
            for (var k = 0; k < 5; k++)
            {
                var tet = pieceDictionary.GetTet(queue[k], 0);
                for (var i = 0; i < Math.Sqrt(tet.Length); i++)
                for (var j = 0; j < Math.Sqrt(tet.Length); j++)
                    //dont draw blank tiles
                    if (tet[i, j] != 0)
                    {
                        var rectPos = new Vector2(position.X + (j + 1) * tileSize + tileSize + boardPadding * (j + 1),
                            position.Y + (i + 1 + k * 4) * tileSize + tileSize + boardPadding * (i + 1));
                        spriteBatch.Draw(squareTexture, rectPos, colorDict[tet[i, j]]);
                    }
            }
        }

        //draw held piece
        public static void DrawHeld(int pieceType, Vector2 position, int tileSize, int boardPadding,
            SpriteBatch spriteBatch, Dictionary<int, Color> colorDict, Texture2D squareTexture,
            PieceDictionary pieceDictionary)
        {
            if (pieceType != 0)
            {
                var tet = pieceDictionary.GetTet(pieceType, 0);
                for (var i = 0; i < Math.Sqrt(tet.Length); i++)
                for (var j = 0; j < Math.Sqrt(tet.Length); j++)
                    //dont draw blank tiles
                    if (tet[i, j] != 0)
                    {
                        var rectPos =
                            new Vector2(position.X + (j + 1) * tileSize + tileSize + boardPadding * (j + 1),
                                position.Y + (i + 1) * tileSize + tileSize + boardPadding * (i + 1));
                        spriteBatch.Draw(squareTexture, rectPos, colorDict[tet[i, j]]);
                    }
            }
        }

        private static int[,] AddGarbageRow(int[,] boardArray, ref bool causesLoss, int holePlacement)
        {
            //Console.Write("added garbage row\n");
            var blankArray = new int[10, 20];
            Array.Clear(blankArray, 0, blankArray.Length);

            for (var i = 0; i < 10; i++)
            {
                //Console.Write(boardArray[i, 0].ToString());
                if (boardArray[i, 0] == 0) continue;
                //Console.Write("Causes loss!");
                causesLoss = true;
                return boardArray;
            }

            for (var i = 1; i < 20; i++)
            for (var j = 0; j < 10; j++)
                blankArray[j, i - 1] = boardArray[j, i];

            for (var i = 0; i < 10; i++)
                if (i != holePlacement)
                    blankArray[i, 19] = 1;

            return blankArray;
        }

        public static bool AddGarbage(ref int[,] boardArray, int garbageData)
        {
            //Console.Write("garbage added");
            var r = new Random();
            var causesLoss = false;

            var holePlacement = r.Next(0, 9);

            for (var i = 0; i < garbageData; i++) boardArray = AddGarbageRow(boardArray, ref causesLoss, holePlacement);

            return causesLoss;
        }
    }
}