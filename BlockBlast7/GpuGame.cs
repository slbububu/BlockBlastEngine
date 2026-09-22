using ILGPU.IR;
using System;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace BlockBlast7
{
    public static class PieceData
    {
        // The GPU compiler will inline these values as constants.
        public const int lenght = 44;
        public static ulong GetPice(int index) => index switch
        {
            0 => 1539, 1 => 66306, 2 => 1539, 3 => 66306,
            4 => 774, 5 => 131841, 6 => 774, 7 => 131841,
            8 => 519, 9 => 131842, 10 => 1794, 11 => 66305,
            12 => 196865, 13 => 1796, 14 => 131587, 15 => 263,
            16 => 197122, 17 => 1793, 18 => 65795, 19 => 1031,
            20 => 459009, 21 => 65799, 22 => 263175, 23 => 459780,
            24 => 16843009, 25 => 15, 26 => 16843009, 27 => 15,
            28 => 4311810305, 29 => 31, 30 => 4311810305, 31 => 31,
            32 => 771, 33 => 771, 34 => 771, 35 => 771,
            36 => 1799, 37 => 197379, 38 => 1799, 39 => 197379,
            40 => 460551, 41 => 460551, 42 => 460551, 43 => 460551,
            _ => 0
        };
        public static byte GetWidth(int index) => index switch
        { 
            0 => 3, 1 => 2, 2 => 3, 3 => 2,
            4 => 3, 5 => 2, 6 => 3, 7 => 2,
            8 => 3, 9 => 2, 10 => 3, 11 => 2,
            12 => 2, 13 => 3, 14 => 2, 15 => 3,
            16 => 2, 17 => 3, 18 => 2, 19 => 3,
            20 => 3, 21 => 3, 22 => 3, 23 => 3,
            24 => 1, 25 => 4, 26 => 1, 27 => 4,
            28 => 1, 29 => 5, 30 => 1, 31 => 5,
            32 => 2, 33 => 2, 34 => 2, 35 => 2,
            36 => 3, 37 => 2, 38 => 3, 39 => 2,
            40 => 3, 41 => 3, 42 => 3, 43 => 3,
            _ => 0
        };
        public static byte GetHeight(int index) => index switch
        {
            0 => 2, 1 => 3, 2 => 2, 3 => 3,
            4 => 2, 5 => 3, 6 => 2, 7 => 3,
            8 => 2, 9 => 3, 10 => 2, 11 => 3,
            12 => 3, 13 => 2, 14 => 3, 15 => 2,
            16 => 3, 17 => 2, 18 => 3, 19 => 2,
            20 => 3, 21 => 3, 22 => 3, 23 => 3,
            24 => 4, 25 => 1, 26 => 4, 27 => 1,
            28 => 5, 29 => 1, 30 => 5, 31 => 1,
            32 => 2, 33 => 2, 34 => 2, 35 => 2,
            36 => 2, 37 => 3, 38 => 2, 39 => 3,
            40 => 3, 41 => 3, 42 => 3, 43 => 3,
            _ => 0
        };
    }

    public struct GpuGame
    {
        public GpuCosts costs;
        public uint rngState;
        public bool gameOver;
        public GpuBoardState board; //toto se pri simulaci kopiruje
        public GpuGame(GpuCosts costsX, uint seed = 1)
        {
            this.rngState = seed;
            this.costs = costsX;
            this.gameOver = false;
            this.board = new GpuBoardState(); // Initializes 0 to mapData/piceData
        }
        public GpuGame() : this(new GpuCosts()) { }
        public GpuGame(uint seed) : this(new GpuCosts(), seed) { }
        public uint NextRandom()
        {
            uint x = rngState;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            rngState = x;
            return x;
        }
        public int PlayAndReturnMoveCountWithDebugDraw()
        {
            for (int ret = 0; ; ret++)
            {
                Update();

                //draw pice
                for (int y = 0; y < S.BLOCKSIZE; y++)
                {
                    for (int b = 0; b < S.BLOCKCOUNT; b++)
                    {

                        for (int x = 0; x < S.BLOCKSIZE; x++)
                        {
                            if (board.GetPiceSpot(b, y, x))
                            {
                                Console.BackgroundColor = ConsoleColor.DarkGreen;
                            }
                            Console.Write("  ");
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                        Console.Write("|");
                    }
                    Console.WriteLine();
                }

                DoTheBestMove();

                for (int x = 0; x < S.WIDTH; x++) Console.Write("--");
                Console.Write(ret+1);
                if(ret % 3 == 0) Console.Write("---");
                Console.WriteLine();

                //draw map
                for (int x = 0; x < S.WIDTH; x++)
                {
                    for (int y = 0; y < S.HEIGHT; y++)
                    {
                        if (board.GetMapSpot(x,y))
                        {
                            Console.BackgroundColor = ConsoleColor.DarkGreen;
                        }
                        Console.Write("  ");
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine("|");
                }
                for (int x = 0; x < S.WIDTH; x++) Console.Write("--");
                Console.WriteLine();

                if (gameOver) return ret;
            }
        }
        public int PlayAndReturnMoveCount()
        {
            for (int ret = 0; ; ret++)
            {
                Update();
                DoTheBestMove();

                if (gameOver) return ret;
            }
        }
        private void Update()
        {
            if (board.AllSlotsEmpty())
            {
                GenerateAllPieces();
            }
        }
        public void DoTheBestMove()
        {
            float bestScore = float.MaxValue;
            int bestPiceIndex = -1, bestToX = 0, bestToY = 0;

            for (int i = 0; i < S.BLOCKCOUNT; i++)
            {
                if (board.IsAPiceInSlot(i))
                {
                    for (int x = 0; x < S.WIDTH; x++)
                    {
                        for (int y = 0; y < S.HEIGHT; y++)
                        {
                            if (board.CanIPlace(i, x, y))
                            {
                                // COPY ONLY THE BOARD STATE!
                                GpuBoardState sim1 = this.board;
                                sim1.PlacePice(i, x, y, false);

                                // Pass costs by ref so the simulations can score themselves
                                float score = sim1.EvaluateDepth2(ref this.costs);

                                if (score < bestScore)
                                {
                                    bestScore = score;
                                    bestPiceIndex = i;
                                    bestToX = x;
                                    bestToY = y;
                                }
                            }
                        }
                    }
                }
            }

            gameOver = (bestScore == float.MaxValue);
            if (!gameOver)
            {
                board.PlacePice(bestPiceIndex, bestToX, bestToY);
            }
        }
        public void GenerateAllPieces()
        {
            for (int i = 0; i < S.BLOCKCOUNT; i++) GeneratePice(i);
        }
        public void GeneratePice(int piceIndex)
        {
            unsafe { board.piceLookUpTable[piceIndex] = (byte)(NextRandom() % PieceData.lenght); }
        }
        static public void TestAndGetAvgScore(int seed0)
        {
            Console.WriteLine("seed | moveCount | total avg | Best Seed Worst Seed");

            int longestNumber = 0;
            uint longestSeed = 0;
            int shortestNumber = int.MaxValue;
            uint shortestSeed = 0;

            float avg = 0;

            for (int i = 0; i < 1000; i++)
            {
                uint currentSeed = (uint)(seed0 + i);
                GpuGame game = new GpuGame(currentSeed);
                int movecount = game.PlayAndReturnMoveCount();
                avg += movecount;

                if (shortestNumber > movecount)
                {
                    shortestNumber = movecount;
                    shortestSeed = currentSeed;
                }
                if (longestNumber < movecount)
                {
                    longestNumber = movecount;
                    longestSeed = currentSeed;
                }

                Console.Write(currentSeed + "  ");
                if (currentSeed < 10) Console.Write(" ");
                if (currentSeed < 100) Console.Write(" ");
                if (currentSeed < 1000) Console.Write(" ");
                if (currentSeed < 10000) Console.Write(" ");

                Console.Write(movecount + "       ");
                if(movecount < 10) Console.Write(" ");
                if(movecount < 100) Console.Write(" ");
                if(movecount < 1000) Console.Write(" ");
                if(movecount < 10000) Console.Write(" ");

                float currentAvg = avg / i;
                currentAvg = (int)currentAvg; //mozes zakomentovat
                Console.Write(currentAvg + "       ");

                Console.Write(longestNumber + " ");
                Console.Write(longestSeed + " ");
                Console.Write(shortestNumber + " ");
                Console.Write(shortestSeed + " ");
                Console.WriteLine();
            }
        }
    }
}