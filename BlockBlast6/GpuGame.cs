using BlockBlast;
using BlockBlast4;
using BlockBlast6;
using ILGPU.IR;
using System;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace console_blockBlast_AI
{
    // 'unsafe' allows keyword "fixed", ktery je potreba pro arraye
    public struct GpuGame
    {
        public GpuCosts costs;
        public uint rngState;
        public bool gameOver;

        // The actual game state that gets copied
        public GpuBoardState board;
        public GpuGame(GpuCosts costsX, uint seed = 0)
        {
            this.rngState = seed;
            this.costs = costsX;
            this.gameOver = false;
            this.board = new GpuBoardState(); // Initializes 0 to mapData/piceData
        }
        public GpuGame() : this(new GpuCosts(), 0) { }
        public GpuGame(uint seed) : this(new GpuCosts(), seed) { }
        public int NextRandom()
        {
            uint x = rngState;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            rngState = x;
            return (int)x;
        }
        public int PlayAndReturnMoveCount()
        {
            for (int ret = 0; ; ret++)
            {
                Update();
                DoTheBestMove();

                //debug draw
                /*
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
                for (int x = 0; x < S.WIDTH; x++)
                    Console.Write("--");
                Console.Write(ret+1);
                if(ret % 3 == 2) Console.Write("---");
                Console.WriteLine();
                */

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
            int bestScore = int.MaxValue;
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
                                int score = sim1.EvaluateDepth2(ref this.costs);

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

            gameOver = (bestScore == int.MaxValue);
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
            board.ClearPice(piceIndex);

            switch (NextRandom() % 10)
            {
                default:
                case 0: // Z-Shape
                    board.SetPiceSpot(piceIndex, 0, 0, true); board.SetPiceSpot(piceIndex, 0, 1, true);
                    board.SetPiceSpot(piceIndex, 1, 1, true); board.SetPiceSpot(piceIndex, 1, 2, true);
                    break;

                case 1: // S-Shape
                    board.SetPiceSpot(piceIndex, 1, 0, true); board.SetPiceSpot(piceIndex, 1, 1, true);
                    board.SetPiceSpot(piceIndex, 0, 1, true); board.SetPiceSpot(piceIndex, 0, 2, true);
                    break;

                case 2: // T-Shape
                    board.SetPiceSpot(piceIndex, 0, 0, true); board.SetPiceSpot(piceIndex, 0, 1, true); board.SetPiceSpot(piceIndex, 0, 2, true);
                    board.SetPiceSpot(piceIndex, 1, 1, true);
                    break;

                case 3: // L-Shape (Standard 2x3)
                    board.SetPiceSpot(piceIndex, 0, 0, true); board.SetPiceSpot(piceIndex, 1, 0, true); board.SetPiceSpot(piceIndex, 2, 0, true);
                    board.SetPiceSpot(piceIndex, 2, 1, true);
                    break;

                case 4: // L longleg (3x2)
                    board.SetPiceSpot(piceIndex, 0, 0, true); board.SetPiceSpot(piceIndex, 0, 1, true); board.SetPiceSpot(piceIndex, 0, 2, true);
                    board.SetPiceSpot(piceIndex, 1, 0, true);
                    break;

                case 5: // 1x4 Stick
                    board.SetPiceSpot(piceIndex, 0, 0, true); board.SetPiceSpot(piceIndex, 0, 1, true);
                    board.SetPiceSpot(piceIndex, 0, 2, true); board.SetPiceSpot(piceIndex, 0, 3, true);
                    break;

                case 6: // 1x5 Stick (Classic Block Blast)
                    board.SetPiceSpot(piceIndex, 0, 0, true); board.SetPiceSpot(piceIndex, 0, 1, true);
                    board.SetPiceSpot(piceIndex, 0, 2, true); board.SetPiceSpot(piceIndex, 0, 3, true);
                    board.SetPiceSpot(piceIndex, 0, 4, true);
                    break;

                case 7: // 2x2 Square
                    board.SetPiceSpot(piceIndex, 0, 0, true); board.SetPiceSpot(piceIndex, 0, 1, true);
                    board.SetPiceSpot(piceIndex, 1, 0, true); board.SetPiceSpot(piceIndex, 1, 1, true);
                    break;

                case 8: // 2x3 Big Block
                    board.SetPiceSpot(piceIndex, 0, 0, true); board.SetPiceSpot(piceIndex, 0, 1, true); board.SetPiceSpot(piceIndex, 0, 2, true);
                    board.SetPiceSpot(piceIndex, 1, 0, true); board.SetPiceSpot(piceIndex, 1, 1, true); board.SetPiceSpot(piceIndex, 1, 2, true);
                    break;

                case 9: // 3x3 Massive Square
                    board.SetPiceSpot(piceIndex, 0, 0, true); board.SetPiceSpot(piceIndex, 0, 1, true); board.SetPiceSpot(piceIndex, 0, 2, true);
                    board.SetPiceSpot(piceIndex, 1, 0, true); board.SetPiceSpot(piceIndex, 1, 1, true); board.SetPiceSpot(piceIndex, 1, 2, true);
                    board.SetPiceSpot(piceIndex, 2, 0, true); board.SetPiceSpot(piceIndex, 2, 1, true); board.SetPiceSpot(piceIndex, 2, 2, true);
                    break;
            }

            int rotation = (NextRandom() % 4);

            for (int r = 0; r < rotation; r++)
            {
                // Clear temp
                board.ClearTemp();

                for (int x = 0; x < S.BLOCKSIZE; x++)
                    for (int y = 0; y < S.BLOCKSIZE; y++)
                    {
                        board.SetTempSpot(x, y, board.GetPiceSpot(piceIndex, x, y));
                    }

                // Copy back from temp to pice
                for (int x = 0; x < S.BLOCKSIZE; x++)
                    for (int y = 0; y < S.BLOCKSIZE; y++)
                        board.SetPiceSpot(piceIndex, x, y, board.GetTempSpot(x, y));
            }

            //vypocitat jak moc zarovnat nahoru a doleva
            int minX = S.BLOCKSIZE, minY = S.BLOCKSIZE;
            for (int x = 0; x < S.BLOCKSIZE; x++)
                for (int y = 0; y < S.BLOCKSIZE; y++)
                {
                    if (board.GetPiceSpot(piceIndex, x, y))
                    {
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                    }
                }

            // zarovnat nahoru a doleva
            for (int x = 0; x < S.BLOCKSIZE; x++)
                for (int y = 0; y < S.BLOCKSIZE; y++)
                {
                    bool val = board.GetPiceSpot(piceIndex, x, y);
                    board.SetPiceSpot(piceIndex, x, y, false);
                    if (val)
                    {
                        int newX = x - minX;
                        int newY = y - minY;

                        board.SetPiceSpot(piceIndex, newX, newY, val); // preserve color!
                    }
                }
        }
    }
}