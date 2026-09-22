using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace console_blockBlast_AI
{
    public class Map
    {
        public char[,] data;
        public Map(char[,] inputMap)
        {
            this.data = inputMap;
        }
        public Map(Map m)
        {
            this.data = (char[,])m.data.Clone();    //ulink the data
        }
        public Map()
        {
            this.data = new char[S.WIDTH, S.HEIGHT];
            ClearMap();
        }
        public void ClearMap()
        {
            for (int y = 0; y < S.WIDTH; y++)
            {
                ClearLineX(y);
            }
        }
        public bool ShouldClear()
        {
            for (int x = 0; x < S.WIDTH; x++)
                if (IsLineFullY(x))
                    return true;

            for (int y = 0; y < S.WIDTH; y++)
                if (IsLineFullX(y))
                    return true;

            return false;
        }

        /// <summary> called when a pice is placed </summary>
        public void DoLineClears()
        {
            bool[] todoClearX = new bool[S.WIDTH];
            bool[] todoClearY = new bool[S.HEIGHT];
            for (int x = 0; x < S.WIDTH; x++) todoClearX[x] = false;
            for (int y = 0; y < S.HEIGHT; y++) todoClearY[y] = false;

            for (int x = 0; x < S.WIDTH; x++) if (IsLineFullY(x)) todoClearX[x] = true;
            for (int y = 0; y < S.WIDTH; y++) if (IsLineFullX(y)) todoClearY[y] = true;

            for (int x = 0; x < S.WIDTH; x++) if (todoClearX[x]) ClearLineY(x);
            for (int y = 0; y < S.HEIGHT; y++) if (todoClearY[y]) ClearLineX(y);
        }
        private bool IsLineFullX(int y)
        {
            for (int x = 0; x < S.WIDTH; x++)
            {
                if (!IsFull(x,y)) return false;
            }
            return true;
        }
        private bool IsLineFullY(int x)
        {
            for (int y = 0; y < S.HEIGHT; y++)
            {
                if (!IsFull(x, y)) return false;
            }
            return true;
        }
        private void ClearLineX(int y)
        {
            for (int x = 0; x < S.WIDTH; x++)
            {
                SetSpot(x, y, S.EMPTY);
            }
        }
        private void ClearLineY(int x)
        {
            for (int y = 0; y < S.HEIGHT; y++)
            {
                SetSpot(x, y, S.EMPTY);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  //optimalizace
        public char GetSpot(int x, int y)
        {
            if (x < 0 || x >= S.WIDTH) return S.FULL;
            if (y < 0 || y >= S.HEIGHT) return S.FULL;
            return data[x, y];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  //optimalizace
        public bool IsFull(int x, int y)
        {
            return (GetSpot(x, y) != S.EMPTY);
        }
        public bool SetSpot(int x, int y, char ch)
        {
            if (x < 0 || x >= S.WIDTH) return false;
            if (y < 0 || y >= S.HEIGHT) return false;
            data[x, y] = ch;
            return true;
        }
        public void Draw()
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine();
            for (int y = 0; y < S.HEIGHT; y++)
            {
                for (int x = 0; x < S.WIDTH; x++)
                {
                    DrawSquare(x, y);
                }
                Console.WriteLine();
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        void DrawSquare(int x, int y)
        {
            char spot = GetSpot(x, y);

            if (spot == S.EMPTY) //background
            {
                if ((x + y) % 2 == 0)
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                else Console.BackgroundColor = ConsoleColor.Gray;
            }
            else S.ChangeColorTo(spot); //nakreslim cast pice

            for (int i = 0; i < S.PICEWIDTH; i++)
            {
                if (Game.selectedSpace == 0 && Game.x == x && Game.y == y && !(y == 0 && x == 0))
                    Console.Write('X');
                else
                    Console.Write(' ');
            }
            Console.BackgroundColor = ConsoleColor.Black;
        }
        public enum CostIndex : int //enums are turned into costs at compile time
        {
            empty0neibor = 0,
            empty1neibor = 1,
            empty2neiborNext = 2,
            empty2neiborOpposite = 3,
            empty3neibor = 4,
            empty4neibor = 5,
            full0neibor = 6,
            full1neibor = 7,
            full2neiborNext = 8,
            full2neiborOpposite = 9,
            full3neibor = 10,
            full4neibor = 11,
        }
        public float GetScore()
        {
            float[] desidingValCost = new float[32];
            //EMPTY
            //0 neibors
            desidingValCost[0b00000] = S.defaultArrayOfCosts[(int)CostIndex.empty0neibor];
            //1 neibor
            desidingValCost[0b01000] = S.defaultArrayOfCosts[(int)CostIndex.empty1neibor];
            desidingValCost[0b00100] = S.defaultArrayOfCosts[(int)CostIndex.empty1neibor];
            desidingValCost[0b00010] = S.defaultArrayOfCosts[(int)CostIndex.empty1neibor];
            desidingValCost[0b00001] = S.defaultArrayOfCosts[(int)CostIndex.empty1neibor];
            //2 neibors (next to each other)
            desidingValCost[0b01100] = S.defaultArrayOfCosts[(int)CostIndex.empty2neiborNext];
            desidingValCost[0b00110] = S.defaultArrayOfCosts[(int)CostIndex.empty2neiborNext];
            desidingValCost[0b00011] = S.defaultArrayOfCosts[(int)CostIndex.empty2neiborNext];
            desidingValCost[0b01001] = S.defaultArrayOfCosts[(int)CostIndex.empty2neiborNext];
            //2 neibors (opposite side)
            desidingValCost[0b01010] = S.defaultArrayOfCosts[(int)CostIndex.empty2neiborOpposite];
            desidingValCost[0b00101] = S.defaultArrayOfCosts[(int)CostIndex.empty2neiborOpposite];
            //3 neibors
            desidingValCost[0b00111] = S.defaultArrayOfCosts[(int)CostIndex.empty3neibor];
            desidingValCost[0b01011] = S.defaultArrayOfCosts[(int)CostIndex.empty3neibor];
            desidingValCost[0b01101] = S.defaultArrayOfCosts[(int)CostIndex.empty3neibor];
            desidingValCost[0b01110] = S.defaultArrayOfCosts[(int)CostIndex.empty3neibor];
            //4 neibors
            desidingValCost[0b01111] = S.defaultArrayOfCosts[(int)CostIndex.empty4neibor];
            //FULL
            //0 neibors
            desidingValCost[0b10000] = S.defaultArrayOfCosts[(int)CostIndex.full0neibor];
            //1 neibor
            desidingValCost[0b11000] = S.defaultArrayOfCosts[(int)CostIndex.full1neibor];
            desidingValCost[0b10100] = S.defaultArrayOfCosts[(int)CostIndex.full1neibor];
            desidingValCost[0b10010] = S.defaultArrayOfCosts[(int)CostIndex.full1neibor];
            desidingValCost[0b10001] = S.defaultArrayOfCosts[(int)CostIndex.full1neibor];
            //2 neibors (next to each other)
            desidingValCost[0b11100] = S.defaultArrayOfCosts[(int)CostIndex.full2neiborNext];
            desidingValCost[0b10110] = S.defaultArrayOfCosts[(int)CostIndex.full2neiborNext];
            desidingValCost[0b10011] = S.defaultArrayOfCosts[(int)CostIndex.full2neiborNext];
            desidingValCost[0b11001] = S.defaultArrayOfCosts[(int)CostIndex.full2neiborNext];
            //2 neibors (opposite side)
            desidingValCost[0b11010] = S.defaultArrayOfCosts[(int)CostIndex.full2neiborOpposite];
            desidingValCost[0b10101] = S.defaultArrayOfCosts[(int)CostIndex.full2neiborOpposite];
            //3 neibors
            desidingValCost[0b10111] = S.defaultArrayOfCosts[(int)CostIndex.full3neibor];
            desidingValCost[0b11011] = S.defaultArrayOfCosts[(int)CostIndex.full3neibor];
            desidingValCost[0b11101] = S.defaultArrayOfCosts[(int)CostIndex.full3neibor];
            desidingValCost[0b11110] = S.defaultArrayOfCosts[(int)CostIndex.full3neibor];
            //4 neibors
            desidingValCost[0b11111] = S.defaultArrayOfCosts[(int)CostIndex.full4neibor];


            float totalScore = 0;

            for (int x = 0; x < S.WIDTH; x++)
                for (int y = 0; y < S.HEIGHT; y++)
                {
                // Neighbor vals
                byte desidingVal = 0;
                desidingVal += (byte)(IsFull(x, y) ? 1 : 0);
                desidingVal <<= 1;
                desidingVal += (byte)(IsFull(x + 1, y) ? 1 : 0);
                desidingVal <<= 1;
                desidingVal += (byte)(IsFull(x, y + 1) ? 1 : 0);
                desidingVal <<= 1;
                desidingVal += (byte)(IsFull(x - 1, y) ? 1 : 0);
                desidingVal <<= 1;
                desidingVal += (byte)(IsFull(x, y - 1) ? 1 : 0);

                totalScore += desidingValCost[desidingVal];
                // desidingVal = 0b12345 
                // 1 = full
                // 2 = right
                // 2 = down
                // 4 = left
                // 5 = up
            }
            return totalScore;
        }
        public int GetIslandCount()
        {
            int count = 0;
            //create and clear discovered
            bool[,] discovered = new bool[S.WIDTH, S.HEIGHT];
            for (int x = 0; x < S.WIDTH; x++)
                for(int y = 0; y < S.HEIGHT; y++)
                {
                    discovered[x, y] = false;
                }

            //rekurzivne volame tuto funkci pro oznaceni vsech casti islandu
            void DiscoverNeibors(int x, int y, bool fullness)
            {
                if (x < 0 || x >= S.WIDTH) return;
                if (y < 0 || y >= S.HEIGHT) return;
                if ((IsFull(x, y) != fullness)) return;
                if (discovered[x, y] == true) return;

                discovered[x,y] = true;
                DiscoverNeibors(x + 1, y, fullness);
                DiscoverNeibors(x - 1, y, fullness);
                DiscoverNeibors(x, y + 1, fullness);
                DiscoverNeibors(x, y - 1, fullness);
            }

            for (int x = 0; x < S.WIDTH; x++)
                for (int y = 0; y < S.HEIGHT; y++)
                    if (!discovered[x,y])
                    {
                        count++;
                        DiscoverNeibors(x, y, IsFull(x, y));
                    }

            return count;
        }
    }
}
