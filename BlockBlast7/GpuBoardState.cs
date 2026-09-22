using System;
using System.Collections.Generic;
using System.Text;

namespace BlockBlast7
{
    using ILGPU;
    using ILGPU.Algorithms;
    using ILGPU.Backends.OpenCL;
    using ILGPU.IR;
    using System.IO.Pipelines;
    using System.Runtime.CompilerServices;

    public unsafe struct GpuBoardState
    {
        public ulong mapData = 0;
        //public ulong mapData = 18410856566090662016;
        //public ulong mapData = 18410856566090662017;
        public fixed byte piceLookUpTable[S.BLOCKCOUNT];

        public GpuBoardState()
        {
            for (int i = 0; i < S.BLOCKCOUNT; i++)
            {
                piceLookUpTable[i] = S.EMPTY;
            }
        }

        #region Game
        // Replaces the first recursive call
        public float EvaluateDepth2(ref GpuCosts costs)
        {
            float bestScore = float.MaxValue;
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
                                sim2.PlacePice(i, x, y,false);

                                float score = sim2.EvaluateDepth3(ref costs);
                                if (score < bestScore) bestScore = score;
                            }
                        }
                    }
                }
            }
            return allSlotsEmpty ? GetScore(ref costs) : bestScore;
        }
        public float EvaluateDepth3(ref GpuCosts costs)
        {
            float bestScore = float.MaxValue;
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
                                float score = sim3.GetScore(ref costs);
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
        byte GetPiceID(int piceIndex)
        {
            return piceLookUpTable[piceIndex];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ulong GetPiceData(int piceIndex)
        {
            if (!IsAPiceInSlot(piceIndex)) return 0;
            return PieceData.GetPice(piceLookUpTable[piceIndex]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetPiceSpot(int piceIndex, int x, int y)
        {
            ulong mask = 1;
            mask <<= y * S.WIDTH + x;
            mask &= GetPiceData(piceIndex);
            return mask != 0;
        }
        public void ClearPice(int piceIndex)
        {
            piceLookUpTable[piceIndex] = S.EMPTY;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ///<summary> cant be empty</summary>
        public bool IsAPiceInSlot(int piceIndex) //mozna optimalizovat, pokud to bude znatelne zpomalovat
        {
            return piceLookUpTable[piceIndex] != S.EMPTY;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllSlotsEmpty() //no ifs
        {
            for (int i = 0; i < S.BLOCKCOUNT; i++)
                if(IsAPiceInSlot(i)) 
                    return false;
            return true;
        }
        #endregion Pice

        #region Map
        public bool CanIPlace(int piceIndex, int x, int y)
        {
            if (x + PieceData.GetWidth(GetPiceID(piceIndex)) > S.WIDTH) return false;
            if (y + PieceData.GetHeight(GetPiceID(piceIndex)) > S.HEIGHT) return false;

            ulong mask = GetPiceData(piceIndex);
            mask <<= y * S.WIDTH + x;
            return mapData == (mapData & ~mask);
        }
        public bool PlacePice(int piceIndex, int x, int y, bool checkIfItFits = true)
        {
            if (checkIfItFits && !CanIPlace(piceIndex, x, y)) return false;

            ulong mask = GetPiceData(piceIndex);
            mask <<= y * S.WIDTH + x;
            mapData = (mapData | mask);

            ClearPice(piceIndex);
            DoLineClears();
            return true;
        }
        public unsafe float GetScore(ref GpuCosts costs)
        {
            fixed (float* pCosts = costs.arrayOfValues) //OPTIMALIZOVAT NEJSPISE IDK
            {
                float[] desidingValCost = new float[32];
                //EMPTY
                //0 neibors
                desidingValCost[0b00000] = pCosts[(int)CostIndex.empty0neibor];
                //1 neibor
                desidingValCost[0b01000] = pCosts[(int)CostIndex.empty1neibor];
                desidingValCost[0b00100] = pCosts[(int)CostIndex.empty1neibor];
                desidingValCost[0b00010] = pCosts[(int)CostIndex.empty1neibor];
                desidingValCost[0b00001] = pCosts[(int)CostIndex.empty1neibor];
                //2 neibors (next to each other)
                desidingValCost[0b01100] = pCosts[(int)CostIndex.empty2neiborNext];
                desidingValCost[0b00110] = pCosts[(int)CostIndex.empty2neiborNext];
                desidingValCost[0b00011] = pCosts[(int)CostIndex.empty2neiborNext];
                desidingValCost[0b01001] = pCosts[(int)CostIndex.empty2neiborNext];
                //2 neibors (opposite side)
                desidingValCost[0b01010] = pCosts[(int)CostIndex.empty2neiborOpposite];
                desidingValCost[0b00101] = pCosts[(int)CostIndex.empty2neiborOpposite];
                //3 neibors
                desidingValCost[0b00111] = pCosts[(int)CostIndex.empty3neibor];
                desidingValCost[0b01011] = pCosts[(int)CostIndex.empty3neibor];
                desidingValCost[0b01101] = pCosts[(int)CostIndex.empty3neibor];
                desidingValCost[0b01110] = pCosts[(int)CostIndex.empty3neibor];
                //4 neibors
                desidingValCost[0b01111] = pCosts[(int)CostIndex.empty4neibor];
                //FULL
                //0 neibors
                desidingValCost[0b10000] = pCosts[(int)CostIndex.full0neibor];
                //1 neibor
                desidingValCost[0b11000] = pCosts[(int)CostIndex.full1neibor];
                desidingValCost[0b10100] = pCosts[(int)CostIndex.full1neibor];
                desidingValCost[0b10010] = pCosts[(int)CostIndex.full1neibor];
                desidingValCost[0b10001] = pCosts[(int)CostIndex.full1neibor];
                //2 neibors (next to each other)
                desidingValCost[0b11100] = pCosts[(int)CostIndex.full2neiborNext];
                desidingValCost[0b10110] = pCosts[(int)CostIndex.full2neiborNext];
                desidingValCost[0b10011] = pCosts[(int)CostIndex.full2neiborNext];
                desidingValCost[0b11001] = pCosts[(int)CostIndex.full2neiborNext];
                //2 neibors (opposite side)
                desidingValCost[0b11010] = pCosts[(int)CostIndex.full2neiborOpposite];
                desidingValCost[0b10101] = pCosts[(int)CostIndex.full2neiborOpposite];
                //3 neibors
                desidingValCost[0b10111] = pCosts[(int)CostIndex.full3neibor];
                desidingValCost[0b11011] = pCosts[(int)CostIndex.full3neibor];
                desidingValCost[0b11101] = pCosts[(int)CostIndex.full3neibor];
                desidingValCost[0b11110] = pCosts[(int)CostIndex.full3neibor];
                //4 neibors
                desidingValCost[0b11111] = pCosts[(int)CostIndex.full4neibor];

                // Neighbor vals
                ulong rightNeiborVal = ((this.mapData << 1) | 0x0101010101010101);
                ulong leftNeiborVal = ((this.mapData >> 1) | 0x8080808080808080);
                ulong downNeiborVal = ((this.mapData >> 8) | 0xFF00000000000000);
                ulong upNeiborVal = ((this.mapData << 8) | 0x00000000000000FF);

                float totalScore = 0;

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

                    totalScore += desidingValCost[desidingVal];
                    // desidingVal = 0b12345 
                    // 1 = full
                    // 2 = right
                    // 2 = down
                    // 4 = leftW
                    // 5 = up
                }
            return (int)totalScore;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearMap()
        {
            mapData = 0;
        }
        #region lineClears
        public void DoLineClears()
        {
            byte todoClearX = 0;
            byte todoClearY = 0;

            for (int x = 0; x < S.WIDTH; x++) 
                if (IsLineFullY(x))
                {
                    todoClearX |= (byte)(1 << x);
                }
            for (int y = 0; y < S.WIDTH; y++) 
                if (IsLineFullX(y))
                {
                    todoClearY |= (byte)(1 << y);
                }

            for (int x = 0; x < S.WIDTH; x++)
                if ((byte)(todoClearX >> x & 1) == 1)
                    ClearLineY(x);
            for (int y = 0; y < S.HEIGHT; y++) 
                if ((byte)(todoClearY >> y & 1) == 1)
                    ClearLineX(y);
        }
        private bool IsLineFullX(int y)
        {
            ulong mask = 0x00000000000000FF;
            mask <<= y * S.WIDTH;
            return (mapData & mask) == mask;  
        }
        private bool IsLineFullY(int x)
        {
            ulong mask = 0x0101010101010101;
            mask <<= x;
            return (mapData & mask) == mask;
        }
        private void ClearLineX(int y)
        {
            ulong mask = 0x00000000000000FF;
            mask <<= y * S.WIDTH;
            mapData &= ~mask;
        }
        private void ClearLineY(int x)
        {
            ulong mask = 0x0101010101010101;
            mask <<= x;
            mapData &= ~mask;
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
            ulong mask = 1;
            mask <<= y * S.WIDTH + x;
            mask &= mapData;
            return mask != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  //optimalizace
        public bool SetMapSpot(int x, int y, bool value)
        {
            ulong mask = 1;
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
