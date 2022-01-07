using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonogameTetris.TetrisLib
{
    public class TetrisUtil
    {
        private PieceDictionary _pieceDictionary = new PieceDictionary();

        public void DrawBoard(int sizeX, int sizeY, int posX, int posY, int tileSize, int boardPadding, SpriteBatch spriteBatch, Dictionary<int, Color> colorDict, int[,] boardArray, Texture2D squareTexture)
        {
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    var rectPos = new Vector2(posX + ((i + 1) * tileSize) + tileSize + (boardPadding * (i+1)), posY + ((j + 1) * tileSize) + tileSize + (boardPadding * (j+1)));
                    spriteBatch.Draw(squareTexture, rectPos, colorDict[boardArray[i, j]]);
                }   
            }
        }

        public int[,] ReturnBoardWithPiece(int[,] boardArray1, ActivePiece piece)
        {
            int[,] pieceShape = _pieceDictionary.GetTet(piece.pieceType, piece.rotState);
            for (int i = 0; i < piece.sideLength; i++)
            {
                for (int j = 0; j < piece.sideLength; j++)
                {
                    if (pieceShape[i, j] == 0)
                    {
                        continue;
                    }
                    boardArray1[piece.CurrentLocation.x + j, piece.CurrentLocation.y + i] = pieceShape[i, j];
                }
            }
            return boardArray1;
        }

        public List<int> CheckLines(int[,] boardArray)
        {
            List<int> lines = new List<int>();
            bool lineCheck;

            for (int i = 0; i < 20; i++)
            {
                lineCheck = true;
                for (int j = 0; j < 10; j++)
                {
                    if (boardArray[j, i] == 0)
                    {
                        lineCheck = false;
                        break;
                    }
                }

                if (lineCheck)
                {
                    lines.Add(i);
                }
            }

            return lines;
        }
    }
}