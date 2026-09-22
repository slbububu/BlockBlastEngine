using BlockBlast;
using BlockBlast4;
using System;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

//TODO
//costs array vraci jen prvni cislo (88.57806f) myslim
//verify ze to dela cleary pri hledani nejlepsi 

namespace console_blockBlast_AI
{
    // 'unsafe' allows keyword "fixed"
    public unsafe struct GpuGame //class -> struct      + no "new" keyword
    {
        public GpuCosts costs;
        public bool gameOver;
        public fixed char mapData[S.BOARDLENGHT];
        public fixed char piceData[S.BLOCKCOUNT * S.BLOCKLENGHT];
        public fixed char tempPice[S.BLOCKLENGHT];
        public GpuGame(GpuCosts costsX)
        {
            costs = costsX;
            gameOver = false;

            // Initialize buffers to empty
            ClearMap();
            ClearPices();
            ClearTemp();
        }

        // --- MAIN ENTRY POINT FOR THE KERNEL ---
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
                        if (IsMapFull(x,y))
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
                Console.WriteLine(ret+1);
                */

                if (gameOver) return ret;
            }
        }

        #region Game
        private void Update()
        {
            if (AllSlotsEmpty())
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
                if (IsAPiceInSlot(i))
                {
                    for (int x = 0; x < S.WIDTH; x++)
                    {
                        for (int y = 0; y < S.HEIGHT; y++)
                        {
                            // INSTEAD OF: Game recursiveGame = new Game(this);
                            // WE COPY THE STRUCT BY VALUE:
                            GpuGame simulatedGame = this;

                            if (simulatedGame.PlacePice(i, x, y))
                            {
                                // We call BestScore on the simulated struct
                                int score = simulatedGame.BestScore();
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
                PlacePice(bestPiceIndex, bestToX, bestToY);
            }
        }
        // GPU recursion is not prefered but 1-2 dept wont crash
        public int BestScore()
        {
            int bestScore = int.MaxValue;
            bool allSlotsEmpty = true;

            for (int i = 0; i < S.BLOCKCOUNT; i++)
            {
                if (IsAPiceInSlot(i))
                {
                    allSlotsEmpty = false;
                    for (int x = 0; x < S.WIDTH; x++)
                    {
                        for (int y = 0; y < S.HEIGHT; y++)
                        {
                            // Copy struct by value
                            GpuGame simulatedGame = this;
                            if (simulatedGame.PlacePice(i, x, y))
                            {
                                int score = simulatedGame.BestScore();
                                if (score < bestScore) bestScore = score;
                            }
                        }
                    }
                }
            }

            if (allSlotsEmpty)
                return GetScore(); // Replaces map.GetScore()
            else
                return bestScore;
        }
        #endregion Game

        #region Pice
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char GetPiceSpot(int pice, int x, int y)
        {
            unsafe
            {
                if (x < 0 || x >= S.BLOCKSIZE) return S.FULL;
                if (y < 0 || y >= S.BLOCKSIZE) return S.FULL;
                if (pice < 0 || pice >= S.BLOCKCOUNT) return S.FULL;
                return piceData[pice * S.BLOCKLENGHT + x * S.BLOCKSIZE + y];
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetPiceSpot(int pice, int x, int y, char ch)
        {
            unsafe
            {
                if (x < 0 || x >= S.BLOCKSIZE) return false;
                if (y < 0 || y >= S.BLOCKSIZE) return false;
                if (pice < 0 || pice >= S.BLOCKCOUNT) return false;
                piceData[pice * S.BLOCKLENGHT + x * S.BLOCKSIZE + y] = ch;
                return true;
            }
        }
        public void ClearTemp()
        {
            unsafe
            {
                for (int x = 0; x < S.BLOCKSIZE; x++)
                    for (int y = 0; y < S.BLOCKSIZE; y++)
                        tempPice[x * S.BLOCKSIZE + y] = S.EMPTY;
            }
        }
        public void ClearPice(int piceIndex)
        {
            for (int x = 0; x < S.BLOCKSIZE; x++)
                for (int y = 0; y < S.BLOCKSIZE; y++)
                    SetPiceSpot(piceIndex, x, y, S.EMPTY);
        }
        public void ClearPices()
        {
            for (int b = 0; b < S.BLOCKCOUNT; b++)
            {
                ClearPice(b);
            }
        }
        public void GeneratePice(int piceIndex)
        {
            ClearPice(piceIndex);

            char FULL = S.FULL;

            switch (S.NextRandom() % 10)
            {
                default:
                case 0: // Z-Shape
                    SetPiceSpot(piceIndex, 0, 0, FULL); SetPiceSpot(piceIndex, 0, 1, FULL);
                    SetPiceSpot(piceIndex, 1, 1, FULL); SetPiceSpot(piceIndex, 1, 2, FULL);
                    break;

                case 1: // S-Shape
                    SetPiceSpot(piceIndex, 1, 0, FULL); SetPiceSpot(piceIndex, 1, 1, FULL);
                    SetPiceSpot(piceIndex, 0, 1, FULL); SetPiceSpot(piceIndex, 0, 2, FULL);
                    break;

                case 2: // T-Shape
                    SetPiceSpot(piceIndex, 0, 0, FULL); SetPiceSpot(piceIndex, 0, 1, FULL); SetPiceSpot(piceIndex, 0, 2, FULL);
                    SetPiceSpot(piceIndex, 1, 1, FULL);
                    break;

                case 3: // L-Shape (Standard 2x3)
                    SetPiceSpot(piceIndex, 0, 0, FULL); SetPiceSpot(piceIndex, 1, 0, FULL); SetPiceSpot(piceIndex, 2, 0, FULL);
                    SetPiceSpot(piceIndex, 2, 1, FULL);
                    break;

                case 4: // L longleg (3x2)
                    SetPiceSpot(piceIndex, 0, 0, FULL); SetPiceSpot(piceIndex, 0, 1, FULL); SetPiceSpot(piceIndex, 0, 2, FULL);
                    SetPiceSpot(piceIndex, 1, 0, FULL);
                    break;

                case 5: // 1x4 Stick
                    SetPiceSpot(piceIndex, 0, 0, FULL); SetPiceSpot(piceIndex, 0, 1, FULL);
                    SetPiceSpot(piceIndex, 0, 2, FULL); SetPiceSpot(piceIndex, 0, 3, FULL);
                    break;

                case 6: // 1x5 Stick (Classic Block Blast)
                    SetPiceSpot(piceIndex, 0, 0, FULL); SetPiceSpot(piceIndex, 0, 1, FULL);
                    SetPiceSpot(piceIndex, 0, 2, FULL); SetPiceSpot(piceIndex, 0, 3, FULL);
                    SetPiceSpot(piceIndex, 0, 4, FULL);
                    break;

                case 7: // 2x2 Square
                    SetPiceSpot(piceIndex, 0, 0, FULL); SetPiceSpot(piceIndex, 0, 1, FULL);
                    SetPiceSpot(piceIndex, 1, 0, FULL); SetPiceSpot(piceIndex, 1, 1, FULL);
                    break;

                case 8: // 2x3 Big Block
                    SetPiceSpot(piceIndex, 0, 0, FULL); SetPiceSpot(piceIndex, 0, 1, FULL); SetPiceSpot(piceIndex, 0, 2, FULL);
                    SetPiceSpot(piceIndex, 1, 0, FULL); SetPiceSpot(piceIndex, 1, 1, FULL); SetPiceSpot(piceIndex, 1, 2, FULL);
                    break;

                case 9: // 3x3 Massive Square
                    SetPiceSpot(piceIndex, 0, 0, FULL); SetPiceSpot(piceIndex, 0, 1, FULL); SetPiceSpot(piceIndex, 0, 2, FULL);
                    SetPiceSpot(piceIndex, 1, 0, FULL); SetPiceSpot(piceIndex, 1, 1, FULL); SetPiceSpot(piceIndex, 1, 2, FULL);
                    SetPiceSpot(piceIndex, 2, 0, FULL); SetPiceSpot(piceIndex, 2, 1, FULL); SetPiceSpot(piceIndex, 2, 2, FULL);
                    break;
            }

            int rotation = (S.NextRandom() % 4);

            unsafe
            {
                for (int r = 0; r < rotation; r++)
                {
                    // Clear temp
                    for (int x = 0; x < S.BLOCKSIZE; x++)
                        for (int y = 0; y < S.BLOCKSIZE; y++)
                        {
                            tempPice[x * S.BLOCKSIZE + y] = S.EMPTY;
                        }

                    for (int x = 0; x < S.BLOCKSIZE; x++)
                        for (int y = 0; y < S.BLOCKSIZE; y++)
                        {
                            tempPice[x * S.BLOCKSIZE + (S.BLOCKSIZE - 1) - y] = GetPiceSpot(piceIndex, x, y);
                        }

                    // Copy back from temp to pice
                    for (int x = 0; x < S.BLOCKSIZE; x++)
                        for (int y = 0; y < S.BLOCKSIZE; y++)
                            SetPiceSpot(piceIndex, x, y, tempPice[x * S.BLOCKSIZE + y]);
                }
            }

            //vypocitat jak moc zarovnat nahoru a doleva
            int minX = S.BLOCKSIZE, minY = S.BLOCKSIZE;
            for (int x = 0; x < S.BLOCKSIZE; x++)
                for (int y = 0; y < S.BLOCKSIZE; y++)
                {
                    if (GetPiceSpot(piceIndex, x, y) != S.EMPTY)
                    {
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                    }
                }

            // zarovnat nahoru a doleva
            for (int x = 0; x < S.BLOCKSIZE; x++)
                for (int y = 0; y < S.BLOCKSIZE; y++)
                {
                    char val = GetPiceSpot(piceIndex, x, y);
                    SetPiceSpot(piceIndex, x, y, S.EMPTY);
                    if (val != S.EMPTY)
                    {
                        int newX = x - minX;
                        int newY = y - minY;

                        SetPiceSpot(piceIndex, newX, newY, val); // preserve color!
                    }
                }
        }
        private void GenerateAllPieces()
        {
            for (int i = 0; i < S.BLOCKCOUNT; i++) GeneratePice(i);
        }
        public bool IsAPiceInSlot(int slotIndex) //mozna optimalizovat, pokud to bude znatelne zpomalovat
        {
            for (int x = 0; x < S.BLOCKSIZE; x++)
                for (int y = 0; y < S.BLOCKSIZE; y++)
                {
                    if (GetPiceSpot(slotIndex, x, y) != S.EMPTY) return true;
                }
            return false;
        }
        public bool AllSlotsEmpty()
        {
            for (int i = 0; i < S.BLOCKCOUNT; i++)
            {
                if (IsAPiceInSlot(i)) return false;
            }
            return true;
        }
        #endregion Pice

        #region Map
        bool CanIPlace(int piceIndex, int x, int y)
        {
            for (int py = 0; py < S.BLOCKSIZE; py++)
            {
                for (int px = 0; px < S.BLOCKSIZE; px++)
                {
                    if (GetPiceSpot(piceIndex, px, py) != S.EMPTY)
                    {
                        if (IsMapFull(x + px, y + py))
                            return false;
                    }
                }
            }
            return true;
        }
        public bool PlacePice(int piceIndex, int x, int y)
        {
            if (!CanIPlace(piceIndex, x, y)) return false;
            for (int px = 0; px < S.BLOCKSIZE; px++)
            {
                for (int py = 0; py < S.BLOCKSIZE; py++)
                {
                    char picePixel = GetPiceSpot(piceIndex, px, py);
                    if (picePixel != S.EMPTY)
                    {
                        SetMapSpot(x + px, y + py, picePixel);
                    }
                }
            }
            ClearPice(piceIndex);
            DoLineClears();
            return true;
        }
        private int GetScore()
        {
            float score = 0;
            for (int y = 0; y < S.HEIGHT; y++)
                for (int x = 0; x < S.WIDTH; x++)
                {
                    int unsimularity = 4;
                    bool mySpot = IsMapFull(x, y);
                    if (mySpot == IsMapFull(x + 1, y)) unsimularity--;
                    if (mySpot == IsMapFull(x - 1, y)) unsimularity--;
                    if (mySpot == IsMapFull(x, y + 1)) unsimularity--;
                    if (mySpot == IsMapFull(x, y - 1)) unsimularity--;

                    score += costs.arrayOfValues[unsimularity];

                    if (IsMapFull(x, y)) score += costs.arrayOfValues[(int)CostIndex.full];
                }
            //score += costs.costs.arrayOfValues[(int)CostIndex.island] * GetIslandCount();
            return (int)score;
        }
        public void ClearMap()
        {
            for (int y = 0; y < S.WIDTH; y++)
            {
                ClearLineX(y);
            }
        }
        public void DoLineClears()
        {
            //line clears
            for (int x = 0; x < S.WIDTH; x++)
                if (IsLineFullY(x))
                    ClearLineY(x);

            for (int y = 0; y < S.WIDTH; y++)
                if (IsLineFullX(y))
                    ClearLineX(y);
        }
        private bool IsLineFullX(int y)
        {
            for (int x = 0; x < S.WIDTH; x++)
            {
                if (!IsMapFull(x, y)) return false;
            }
            return true;
        }
        private bool IsLineFullY(int x)
        {
            for (int y = 0; y < S.HEIGHT; y++)
            {
                if (!IsMapFull(x, y)) return false;
            }
            return true;
        }
        private void ClearLineX(int y)
        {
            for (int x = 0; x < S.WIDTH; x++)
            {
                SetMapSpot(x, y, S.EMPTY);
            }
        }
        private void ClearLineY(int x)
        {
            for (int y = 0; y < S.HEIGHT; y++)
            {
                SetMapSpot(x, y, S.EMPTY);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  //optimalizace
        public char GetMapSpot(int x, int y)
        {
            unsafe
            {
                if (x < 0 || x >= S.WIDTH) return S.FULL;
                if (y < 0 || y >= S.HEIGHT) return S.FULL;
                return mapData[x * S.WIDTH + y];
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  //optimalizace
        public bool IsMapFull(int x, int y)
        {
            return (GetMapSpot(x, y) != S.EMPTY);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetMapSpot(int x, int y, char ch)
        {
            unsafe
            {
                if (x < 0 || x >= S.WIDTH) return false;
                if (y < 0 || y >= S.HEIGHT) return false;
                mapData[x * S.WIDTH + y] = ch;
                return true;
            }
        }
        #endregion Map
    }
}