using BlockBlast;
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
        public Costs costs = new Costs();

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
            bool anyCleared = false;

            for (int x = 0; x < S.WIDTH; x++)
                if (IsLineFullY(x))
                    anyCleared = true;

            for (int y = 0; y < S.WIDTH; y++)
                if (IsLineFullX(y))
                    anyCleared = true;

            return anyCleared;
        }

        /// <summary> called when a pice is placed </summary>
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
        public void DrawWithOutlines()
        {
            Console.WriteLine();
            for (int y = 0; y < S.HEIGHT + 2; y++)
            {
                if (y == 0 || y == S.HEIGHT + 1)
                {
                    Console.Write(y == 0 ? "┌" : "└");
                    for (int x = 0; x < S.WIDTH * S.PICEWIDTH; x++) Console.Write("-");
                    Console.WriteLine(y == 0 ? "┐" : "┘");
                    continue;
                }

                Console.Write("|");
                for (int x = 0; x < S.WIDTH; x++)
                {
                    DrawSquare(x, y - 1);
                }
                Console.Write("|\n");
            }
        }
        public void Draw()
        {
            Console.WriteLine();
            for (int y = 0; y < S.HEIGHT; y++)
            {
                for (int x = 0; x < S.WIDTH; x++)
                {
                    DrawSquare(x, y);
                }
                Console.WriteLine();
            }
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

            for (int i = 0; i < S.PICEWIDTH; i++) Console.Write(' ');
            Console.ResetColor();
        }
        public int GetScore()
        {
            float score = 0;
            for (int y = 0; y < S.HEIGHT; y++)
                for (int x = 0; x < S.WIDTH; x++)
                {
                    int unsimularity = 4;
                    bool mySpot = IsFull(x, y);
                    if (mySpot == IsFull(x + 1, y)) unsimularity--;
                    if (mySpot == IsFull(x - 1, y)) unsimularity--;
                    if (mySpot == IsFull(x, y + 1)) unsimularity--;
                    if (mySpot == IsFull(x, y - 1)) unsimularity--;

                    score += costs.arrayOfValues[unsimularity];

                    if (IsFull(x, y)) score += costs.arrayOfValues[5];
                }
            //score += costs.islandCost * GetIslandCount();
            return (int)score;
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
