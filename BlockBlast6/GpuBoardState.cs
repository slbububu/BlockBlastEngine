using System;
using System.Collections.Generic;
using System.Text;

namespace BlockBlast6
{
    using BlockBlast;
    using BlockBlast4;
    using console_blockBlast_AI;
    using ILGPU;
    using ILGPU.Algorithms;
    using ILGPU.Backends.OpenCL;
    using System.Runtime.CompilerServices;

    public struct GpuBoardState
    {
        public ulong mapData;
        public ulong piceData;
        public uint tempPice;

        #region Game

        // Replaces the first recursive call
        public int EvaluateDepth2(ref GpuCosts costs)
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
                            if (CanIPlace(i, x, y))
                            {
                                // WE ONLY COPY THE 20-BYTE BOARD STATE NOW!
                                GpuBoardState sim2 = this;
                                sim2.PlacePice(i, x, y, false);

                                int score = sim2.EvaluateDepth3(ref costs);
                                if (score < bestScore) bestScore = score;
                            }
                        }
                    }
                }
            }
            return allSlotsEmpty ? GetScore(ref costs) : bestScore;
        }
        public int EvaluateDepth3(ref GpuCosts costs)
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
                            if (CanIPlace(i, x, y))
                            {
                                GpuBoardState sim3 = this;
                                sim3.PlacePice(i, x, y, false);

                                // Assuming Depth 3 is the bottom, we just get the score
                                int score = sim3.GetScore(ref costs);
                                if (score < bestScore) bestScore = score;
                            }
                        }
                    }
                }
            }
            return allSlotsEmpty ? GetScore(ref costs) : bestScore;
        }        
        #endregion Game

        #region Pice
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetPiceSpot(int pice, int x, int y)
        {
            //if (x < 0 || x >= S.BLOCKSIZE) return true;           //optimalizace
            //if (y < 0 || y >= S.BLOCKSIZE) return true;           //optimalizace
            //if (pice < 0 || pice >= S.BLOCKCOUNT) return true;    //optimalizace

            ulong mask = 1UL;    //1UL je 1, ale 1 by nefungovala myslim
            mask <<= pice * S.BLOCKLENGHT + y * S.BLOCKSIZE + x;
            mask &= piceData;
            return mask != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetPiceSpot(int pice, int x, int y, bool value)
        {
            //if (x < 0 || x >= S.BLOCKSIZE) return false;          //optimalizace
            //if (y < 0 || y >= S.BLOCKSIZE) return false;          //optimalizace
            //if (pice < 0 || pice >= S.BLOCKCOUNT) return false;   //optimalizace

            ulong mask = 1UL;   //1UL je 1, ale 1 by nefungovala myslim
            mask <<= pice * S.BLOCKLENGHT + y * S.BLOCKSIZE + x;
            if (value) //na true
                piceData |= mask;
            else    //na false
                piceData &= ~mask;
            return true;
        }
        #region temp
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  //optimalizace
        public void ClearTemp()
        {
            tempPice = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  //optimalizace
        public bool GetTempSpot(int x, int y)
        {
            //if (x < 0 || x >= S.BLOCKSIZE) return true;     //optimalizace
            //if (y < 0 || y >= S.BLOCKSIZE) return true;    //optimalizace
            uint mask = 1U;
            mask <<= y * S.BLOCKSIZE + x;
            mask &= tempPice;
            return mask != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  //optimalizace
        public bool SetTempSpot(int x, int y, bool value)
        {
            //if (x < 0 || x >= S.BLOCKSIZE) return false;      //optimalizace
            //if (y < 0 || y >= S.BLOCKSIZE) return false;     //optimalizace
            uint mask = 1U;   //1UL je 1, ale 1 by nefungovala myslim
            mask <<= y * S.BLOCKSIZE + x;
            if (value) //na true
                tempPice |= mask;
            else    //na false
                tempPice &= ~mask;
            return true;
        }
        #endregion temp
        public void ClearPice(int piceIndex)
        {
            for (int x = 0; x < S.BLOCKSIZE; x++)
                for (int y = 0; y < S.BLOCKSIZE; y++)
                    SetPiceSpot(piceIndex, x, y, false);
        }
        public bool IsAPiceInSlot(int slotIndex) //mozna optimalizovat, pokud to bude znatelne zpomalovat
        {
            for (int x = 0; x < S.BLOCKSIZE; x++)
                for (int y = 0; y < S.BLOCKSIZE; y++)
                {
                    if (GetPiceSpot(slotIndex, x, y)) return true;
                }
            return false;
        }
        public bool AllSlotsEmpty()
        {
            return piceData == 0;
        }
        #endregion Pice

        #region Map
        public bool CanIPlace(int piceIndex, int x, int y)
        {
            for (int py = 0; py < S.BLOCKSIZE; py++)
            {
                for (int px = 0; px < S.BLOCKSIZE; px++)
                {
                    if (GetPiceSpot(piceIndex, px, py))
                    {
                        if (GetMapSpot(x + px, y + py, true)) 
                            return false;
                    }
                }
            }
            return true;
        }
        public bool PlacePice(int piceIndex, int x, int y, bool checkIfItFits = true)
        {
            if (checkIfItFits && !CanIPlace(piceIndex, x, y)) return false;
            for (int px = 0; px < S.BLOCKSIZE; px++)
            {
                for (int py = 0; py < S.BLOCKSIZE; py++)
                {
                    bool picePixel = GetPiceSpot(piceIndex, px, py);
                    if (picePixel)
                    {
                        SetMapSpot(x + px, y + py, picePixel);
                    }
                }
            }
            ClearPice(piceIndex);
            DoLineClears();
            return true;
        }
        /*
        public unsafe int GetScore(ref GpuCosts costs)
        {
            ulong b = this.mapData;

            // Neighbor Matching Masks
            ulong matchR = ~(b ^ (b >> 1)) & 0x7F7F7F7F7F7F7F7F;
            ulong matchL = ~(b ^ (b << 1)) & 0xFEFEFEFEFEFEFEFE;
            ulong matchD = ~(b ^ (b >> 8));
            ulong matchU = ~(b ^ (b << 8));

            float totalScore = 0;

            fixed (float* pCosts = costs.arrayOfValues)
            {
                float fullCost = pCosts[(int)CostIndex.full];

                // 1. Process Unsimilarity Scores
                // Instead of a nested x/y loop, we loop 0-63 over the bitboard.
                // This is much faster on GPU because it uses registers, not memory.
                for (int i = 0; i < 64; i++)
                {
                    // Extract the i-th bit from each match mask
                    int mCount = (int)((matchR >> i) & 1) +
                                 (int)((matchL >> i) & 1) +
                                 (int)((matchD >> i) & 1) +
                                 (int)((matchU >> i) & 1);

                    totalScore += pCosts[4 - mCount];
                }

                // 2. Add Score for occupied spots (Full Cost)
                // XMath.PopCount is a hardware instruction—it's nearly instant.
                totalScore += XMath.PopCount(b) * fullCost;
            }
            return (int)totalScore;
        }
        */
        public unsafe int GetScore(ref GpuCosts costs)
        {
            // Neighbor vals
            ulong rightNeiborVal= ((this.mapData << 1) | 0x0101010101010101);
            ulong leftNeiborVal = ((this.mapData >> 1) | 0x8080808080808080);
            ulong downNeiborVal = ((this.mapData >> 8) | 0xFF00000000000000);
            ulong upNeiborVal   = ((this.mapData << 8) | 0x00000000000000FF);

            float totalScore = 0;

            fixed (float* pCosts = costs.arrayOfValues)
            {
                for (int i = 0; i < 64; i++)
                {
                    byte desidingVal = 0;
                    desidingVal += (byte)(this.mapData >> i & 1);
                    desidingVal <<= 1;
                    desidingVal += (byte)(rightNeiborVal >> i & 1);
                    desidingVal <<= 1;
                    desidingVal += (byte)(downNeiborVal >> i & 1);
                    desidingVal <<= 1;
                    desidingVal += (byte)(leftNeiborVal >> i & 1);
                    desidingVal <<= 1;
                    desidingVal += (byte)(upNeiborVal >> i & 1);

                    // desidingVal = 0b12345 
                    // 1 = full
                    // 2 = right
                    // 2 = down
                    // 4 = left
                    // 5 = up
                    switch (desidingVal)
                    {
                        default:
                            totalScore -= 9999999;
                            //Console.WriteLine("Pixel score ERROR, pice has invalid neibors");
                            break;
                        //EMPTY ONES
                        case 0b00000:   //0 neibors
                            totalScore += pCosts[(int)CostIndex.empty0neibor];
                            break;
                        case 0b01000:   //1 neibor 
                        case 0b00100: 
                        case 0b00010: 
                        case 0b00001:
                            totalScore += pCosts[(int)CostIndex.empty1neibor];
                            break;
                        case 0b01100:   //2 neibors (next to each other)
                        case 0b00110: 
                        case 0b00011: 
                        case 0b01001:
                            totalScore += pCosts[(int)CostIndex.empty2neiborNext];
                            break;
                        case 0b01010:   //2 neibors (opposite side)
                        case 0b00101:
                            totalScore += pCosts[(int)CostIndex.empty2neiborOpposite];
                            break;
                        case 0b00111:   //3 neibors
                        case 0b01011:
                        case 0b01101:
                        case 0b01110:
                            totalScore += pCosts[(int)CostIndex.empty3neibor];
                            break;
                        case 0b01111:   //4 neibors
                            totalScore += pCosts[(int)CostIndex.empty4neibor];
                            break;
                        //FULL ONES
                        case 0b10000:   //0 neibors
                            totalScore += pCosts[(int)CostIndex.full0neibor];
                            break;
                        case 0b11000:   //1 neibor 
                        case 0b10100:
                        case 0b10010:
                        case 0b10001:
                            totalScore += pCosts[(int)CostIndex.full1neibor];
                            break;
                        case 0b11100:   //2 neibors (next to each other)
                        case 0b10110:
                        case 0b10011:
                        case 0b11001:
                            totalScore += pCosts[(int)CostIndex.full2neiborNext];
                            break;
                        case 0b11010:   //2 neibors (opposite side)
                        case 0b10101:
                            totalScore += pCosts[(int)CostIndex.full2neiborOpposite];
                            break;
                        case 0b10111:   //3 neibors
                        case 0b11011:
                        case 0b11101:
                        case 0b11110:
                            totalScore += pCosts[(int)CostIndex.full3neibor];
                            break;
                        case 0b11111:   //4 neibors
                            totalScore += pCosts[(int)CostIndex.full4neibor];
                            break;
                    }
                }
            }
            return (int)totalScore;
        }
        public void ClearMap()
        {
            mapData = 0;
        }
        #region lineClears
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
                if (!GetMapSpot(x, y)) return false;
            }
            return true;
        }
        private bool IsLineFullY(int x)
        {
            for (int y = 0; y < S.HEIGHT; y++)
            {
                if (!GetMapSpot(x, y)) return false;
            }
            return true;
        }
        private void ClearLineX(int y)
        {
            for (int x = 0; x < S.WIDTH; x++)
            {
                SetMapSpot(x, y, false);
            }
        }
        private void ClearLineY(int x)
        {
            for (int y = 0; y < S.HEIGHT; y++)
            {
                SetMapSpot(x, y, false);
            }
        }
        #endregion lineClears

        [MethodImpl(MethodImplOptions.AggressiveInlining)]  //optimalizace
        public bool GetMapSpot(int x, int y,bool checkBouneries = false)
        {
            if(checkBouneries)//optimalizace
            {
                if (x < 0 || x >= S.WIDTH) return true;
                if (y < 0 || y >= S.HEIGHT) return true;
            }
            ulong mask = 1UL;
            mask <<= y * S.WIDTH + x;
            mask &= mapData;
            return mask != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  //optimalizace
        public bool SetMapSpot(int x, int y, bool value)
        {
            //if (x < 0 || x >= S.WIDTH) return false;  //optimalizace
            //if (y < 0 || y >= S.HEIGHT) return false; //optimalizace
            ulong mask = 1UL;   //1UL je 1, ale 1 by nefungovala myslim
            mask <<= y * S.WIDTH + x;
            if (value) //na true
                mapData |= mask;
            else    //na false
                mapData &= ~mask;
            return true;
        }
        #endregion Map
    }

}
