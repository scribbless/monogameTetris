using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonogameTetris.TetrisLib
{
    public class TetrisUtil
    {
        private readonly PieceDictionary _pieceDictionary = new PieceDictionary();

        //render board function
        public void DrawBoard(int sizeX, int sizeY, int posX, int posY, int tileSize, int boardPadding,
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
            var pieceShape = _pieceDictionary.GetTet(piece.pieceType, piece.rotState);
            for (var i = 0; i < piece.sideLength; i++)
            for (var j = 0; j < piece.sideLength; j++)
            {
                if (pieceShape[i, j] == 0) continue;
                boardArray1[piece.CurrentLocation.x + j, piece.CurrentLocation.y + i] = pieceShape[i, j];
            }

            return boardArray1;
        }

        //check if any lines are full
        public List<int> CheckLines(int[,] boardArray)
        {
            var lines = new List<int>();
            bool lineCheck;

            for (var i = 0; i < 20; i++)
            {
                lineCheck = true;
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
        public int[,] ClearRow(int[,] boardArray, int row)
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

        //draw queue
        public void DrawQueue(List<int> queue, Vector2 position, int tileSize, int boardPadding,
            SpriteBatch spriteBatch, Dictionary<int, Color> colorDict, Texture2D squareTexture,
            PieceDictionary pieceDictionary)
        {
            for (var k = 0; k < 5; k++)
            {
                var tet = pieceDictionary.GetTet(queue[k], 1);
                for (var i = 0; i < Math.Sqrt(tet.Length); i++)
                for (var j = 0; j < Math.Sqrt(tet.Length); j++)
                    //dont draw blank tiles
                    if (tet[i, j] != 0)
                    {
                        var rectPos = new Vector2(position.X + (i + 1) * tileSize + tileSize + boardPadding * (i + 1),
                            position.Y + (j + 1 + k * 4) * tileSize + tileSize + boardPadding * (j + 1));
                        spriteBatch.Draw(squareTexture, rectPos, colorDict[tet[i, j]]);
                    }
            }
        }

        //draw held piece
        public void DrawHeld(int pieceType, Vector2 position, int tileSize, int boardPadding,
            SpriteBatch spriteBatch, Dictionary<int, Color> colorDict, Texture2D squareTexture,
            PieceDictionary pieceDictionary)
        {
            if (pieceType != 0)
            {
                var tet = pieceDictionary.GetTet(pieceType, 1);
                for (var i = 0; i < Math.Sqrt(tet.Length); i++)
                for (var j = 0; j < Math.Sqrt(tet.Length); j++)
                    //dont draw blank tiles
                    if (tet[i, j] != 0)
                    {
                        var rectPos =
                            new Vector2(position.X + (i + 1) * tileSize + tileSize + boardPadding * (i + 1),
                                position.Y + (j + 1) * tileSize + tileSize + boardPadding * (j + 1));
                        spriteBatch.Draw(squareTexture, rectPos, colorDict[tet[i, j]]);
                    }
            }
        }
    }
}