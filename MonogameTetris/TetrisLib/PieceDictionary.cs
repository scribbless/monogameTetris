using System;
using System.Collections.Generic;

namespace MonogameTetris.TetrisLib
{
    public class PieceDictionary
    {
        private int[,] _tet;

        public Dictionary<int, int[,]> PositiveDegWallKicks = new Dictionary<int, int[,]>
        {
            //0 -> 1
            {0, new int[,] {{0, 0}, {-1, 0}, {-1, 1}, {0, -2}, {-1, -2}}},
            //1 -> 2
            {1, new int[,] {{0, 0}, {1, 0}, {1, -1}, {0, 2}, {1, 2}}},
            //2 -> 3
            {2, new int[,] {{0, 0}, {1, 0}, {1, 1}, {0, -2}, {1, -2}}},
            //3 -> 4
            {3, new int[,] {{0, 0}, {-1, 0}, {-1, -1}, {0, 2}, {-1, 2}}}
        };
        
        public Dictionary<int, int[,]> NegativeDegWallKicks = new Dictionary<int, int[,]>
        {
            //0 -> 3
            {0, new int[,] {{0, 0}, {1, 0}, {1, 1}, {0, -2}, {1, -2}}},
            //3 -> 2
            {3, new int[,] {{0, 0}, {-1, 0}, {-1, -1}, {0, 2}, {-1, 2}}},
            //2 -> 1
            {2, new int[,] {{0, 0}, {-1, 0}, {-1, 1}, {0, -2}, {-1, -2}}},
            //1 -> 0
            {1, new int[,] {{0, 0}, {1, 0}, {1, -1}, {0, 2}, {1, 2}}}
        };
        
        public Dictionary<int, int[,]> PositiveDegWallKicksI = new Dictionary<int, int[,]>
        {
            //0 -> 1
            {0, new int[,] {{0, 0}, {-2, 0}, {1, 0}, {-2, -1}, {1, 2}}},
            //1 -> 2
            {1, new int[,] {{0, 0}, {-1, 0}, {2, 0}, {-1, 2}, {2, -1}}},
            //2 -> 3
            {2, new int[,] {{0, 0}, {2, 0}, {-1, 0}, {2, 1}, {-1, -2}}},
            //3 -> 0
            {3, new int[,] {{0, 0}, {1, 0}, {-2, 0}, {1, -2}, {-2, 1}}}
        };
        
        public Dictionary<int, int[,]> NegativeDegWallKicksI = new Dictionary<int, int[,]>
        {
            //0 -> 3
            {0, new int[,] {{0, 0}, {-1, 0}, {2, 0}, {-1, 2}, {2, -1}}},
            //3 -> 2
            {1, new int[,] {{0, 0}, {-2, 0}, {1, 0}, {-2, -1}, {1, 2}}},
            //2 -> 1
            {2, new int[,] {{0, 0}, {1, 0}, {-2, 0}, {1, -2}, {-2, 1}}},
            //1 -> 0
            {3, new int[,] {{0, 0}, {2, 0}, {-1, 0}, {2, 1}, {-1, -2}}}
        };

        public int[,] GetWallKick(bool isI, bool isPos, int rotState)
        {
            if (isI)
                if (isPos)
                    return PositiveDegWallKicksI[rotState];
                else
                    return NegativeDegWallKicksI[rotState];
            else if (isPos)
                return PositiveDegWallKicks[rotState];
            else
                return NegativeDegWallKicks[rotState];
        }

        public Dictionary<int, int[,,]> TetrominoDictionary = new Dictionary<int, int[,,]>
        {
            {
                //I piece
                2,
                new[,,]
                {
                    {
                        //rot state 0 - default
                        {0, 0, 0, 0, 0},
                        {0, 0, 0, 0, 0},
                        {0, 2, 2, 2, 2},
                        {0, 0, 0, 0, 0},
                        {0, 0, 0, 0, 0}
                    },
                    {
                        //rot state 1
                        {0, 0, 0, 0, 0},
                        {0, 0, 2, 0, 0},
                        {0, 0, 2, 0, 0},
                        {0, 0, 2, 0, 0},
                        {0, 0, 2, 0, 0}
                    },
                    {
                        //rot state 2
                        {0, 0, 0, 0, 0},
                        {0, 0, 0, 0, 0},
                        {2, 2, 2, 2, 0},
                        {0, 0, 0, 0, 0},
                        {0, 0, 0, 0, 0}
                    },
                    {
                        //rot state 3
                        {0, 0, 2, 0, 0},
                        {0, 0, 2, 0, 0},
                        {0, 0, 2, 0, 0},
                        {0, 0, 2, 0, 0},
                        {0, 0, 0, 0, 0}
                    }
                }
            },
            {
                //J piece
                4,
                new[,,]
                {
                    {
                        //rot state 0 - default
                        {0, 0, 4},
                        {4, 4, 4},
                        {0, 0, 0}
                    },
                    {
                        //rot state 1
                        {0, 4, 0},
                        {0, 4, 0},
                        {0, 4, 4}
                    },
                    {
                        //rot state 2
                        {0, 0, 0},
                        {4, 4, 4},
                        {4, 0, 0}
                    },
                    {
                        //rot state 3
                        {4, 4, 0},
                        {0, 4, 0},
                        {0, 4, 0}
                    }
                }
            },
            {
                //L piece
                3,
                new[,,]
                {
                    {
                        //rot state 0 - default
                        {3, 0, 0},
                        {3, 3, 3},
                        {0, 0, 0}
                    },
                    {
                        //rot state 1
                        {0, 3, 3},
                        {0, 3, 0},
                        {0, 3, 0}
                    },
                    {
                        //rot state 2
                        {0, 0, 0},
                        {3, 3, 3},
                        {0, 0, 3}
                    },
                    {
                        //rot state 3
                        {0, 3, 0},
                        {0, 3, 0},
                        {3, 3, 0}
                    }
                }
            },
            {
                //O piece
                5,
                new[,,]
                {
                    {
                        //rot state 0 - default
                        {0, 5, 5},
                        {0, 5, 5},
                        {0, 0, 0}
                    },
                    {
                        //rot state 1
                        {0, 5, 5},
                        {0, 5, 5},
                        {0, 0, 0}
                    },
                    {
                        //rot state 2
                        {0, 5, 5},
                        {0, 5, 5},
                        {0, 0, 0}
                    },
                    {
                        //rot state 3
                        {0, 5, 5},
                        {0, 5, 5},
                        {0, 0, 0}
                    }
                }
            },
            {
                //S piece
                6,
                new[,,]
                {
                    {
                        //rot state 0 - default
                        {0, 6, 6},
                        {6, 6, 0},
                        {0, 0, 0}
                    },
                    {
                        //rot state 1
                        {0, 6, 0},
                        {0, 6, 6},
                        {0, 0, 6}
                    },
                    {
                        //rot state 2
                        {0, 0, 0},
                        {0, 6, 6},
                        {6, 6, 0}
                    },
                    {
                        //rot state 3
                        {6, 0, 0},
                        {6, 6, 0},
                        {0, 6, 0}
                    }
                }
            },
            {
                //J piece
                7,
                new[,,]
                {
                    {
                        //rot state 0 - default
                        {0, 7, 0},
                        {7, 7, 7},
                        {0, 0, 0}
                    },
                    {
                        //rot state 1
                        {0, 7, 0},
                        {0, 7, 7},
                        {0, 7, 0}
                    },
                    {
                        //rot state 2
                        {0, 0, 0},
                        {7, 7, 7},
                        {0, 7, 0}
                    },
                    {
                        //rot state 3
                        {0, 7, 0},
                        {7, 7, 0},
                        {0, 7, 0}
                    }
                }
            },
            {
                //J piece
                8,
                new[,,]
                {
                    {
                        //rot state 0 - default
                        {8, 8, 0},
                        {0, 8, 8},
                        {0, 0, 0}
                    },
                    {
                        //rot state 1
                        {0, 0, 8},
                        {0, 8, 8},
                        {0, 8, 0}
                    },
                    {
                        //rot state 2
                        {0, 0, 0},
                        {8, 8, 0},
                        {0, 8, 8}
                    },
                    {
                        //rot state 3
                        {0, 8, 0},
                        {8, 8, 0},
                        {8, 0, 0}
                    }
                }
            }
        };

        public Dictionary<int, int[,,]> GetTetDict()
        {
            return TetrominoDictionary;
        }

        public int[,] GetTet(int pieceType, int rotState)
        {
            if (pieceType == 2)
            {
                _tet = new int[5, 5];
            } else
            {
                _tet = new int[3, 3];
            }
            
            Array.Clear(_tet, 0, _tet.Length);
            int tetTemp = 0;
            for (int i = 0; i < Math.Sqrt(_tet.Length); i++)
            {
                for (int j = 0; j < Math.Sqrt(_tet.Length); j++)
                {
                    tetTemp = TetrominoDictionary[pieceType][rotState, i, j];
                    _tet[i, j] = tetTemp;
                }
            }

            return _tet;
        }
    }
}