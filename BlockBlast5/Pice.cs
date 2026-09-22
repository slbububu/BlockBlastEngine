using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace console_blockBlast_AI
{
    public class Pice
    {
        public Pice() 
        {
            ClearPices();
        }
        public Pice(Pice p)
        {
            this.data = (char[,,])p.data.Clone(); //ulink the data
        }
        public char[,,] data = new char[S.BLOCKCOUNT, S.BLOCKSIZE, S.BLOCKSIZE]; //block ID/slot, x, y
        public void ClearPice(int piceIndex)
        {
            for (int y = 0; y < S.BLOCKSIZE; y++)
                for (int x = 0; x < S.BLOCKSIZE; x++)
                    SetSpot(piceIndex,x,y,S.EMPTY);
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

            char FULL = S.GetNextColor();

            switch (S.rand.Next(0, 10))
            {
                default:
                case 0: // Z-Shape
                    SetSpot(piceIndex, 0, 0, FULL); SetSpot(piceIndex, 0, 1, FULL);
                    SetSpot(piceIndex, 1, 1, FULL); SetSpot(piceIndex, 1, 2, FULL);
                    break;

                case 1: // S-Shape
                    SetSpot(piceIndex, 1, 0, FULL); SetSpot(piceIndex, 1, 1, FULL);
                    SetSpot(piceIndex, 0, 1, FULL); SetSpot(piceIndex, 0, 2, FULL);
                    break;

                case 2: // T-Shape
                    SetSpot(piceIndex, 0, 0, FULL); SetSpot(piceIndex, 0, 1, FULL); SetSpot(piceIndex, 0, 2, FULL);
                    SetSpot(piceIndex, 1, 1, FULL);
                    break;

                case 3: // L-Shape (Standard 2x3)
                    SetSpot(piceIndex, 0, 0, FULL); SetSpot(piceIndex, 1, 0, FULL); SetSpot(piceIndex, 2, 0, FULL);
                    SetSpot(piceIndex, 2, 1, FULL);
                    break;

                case 4: // L longleg (3x2)
                    SetSpot(piceIndex, 0, 0, FULL); SetSpot(piceIndex, 0, 1, FULL); SetSpot(piceIndex, 0, 2, FULL);
                    SetSpot(piceIndex, 1, 0, FULL);
                    break;

                case 5: // 1x4 Stick
                    SetSpot(piceIndex, 0, 0, FULL); SetSpot(piceIndex, 0, 1, FULL);
                    SetSpot(piceIndex, 0, 2, FULL); SetSpot(piceIndex, 0, 3, FULL);
                    break;

                case 6: // 1x5 Stick (Classic Block Blast)
                    SetSpot(piceIndex, 0, 0, FULL); SetSpot(piceIndex, 0, 1, FULL);
                    SetSpot(piceIndex, 0, 2, FULL); SetSpot(piceIndex, 0, 3, FULL);
                    SetSpot(piceIndex, 0, 4, FULL);
                    break;

                case 7: // 2x2 Square
                    SetSpot(piceIndex, 0, 0, FULL); SetSpot(piceIndex, 0, 1, FULL);
                    SetSpot(piceIndex, 1, 0, FULL); SetSpot(piceIndex, 1, 1, FULL);
                    break;

                case 8: // 2x3 Big Block
                    SetSpot(piceIndex, 0, 0, FULL); SetSpot(piceIndex, 0, 1, FULL); SetSpot(piceIndex, 0, 2, FULL);
                    SetSpot(piceIndex, 1, 0, FULL); SetSpot(piceIndex, 1, 1, FULL); SetSpot(piceIndex, 1, 2, FULL);
                    break;

                case 9: // 3x3 Massive Square
                    SetSpot(piceIndex, 0, 0, FULL); SetSpot(piceIndex, 0, 1, FULL); SetSpot(piceIndex, 0, 2, FULL);
                    SetSpot(piceIndex, 1, 0, FULL); SetSpot(piceIndex, 1, 1, FULL); SetSpot(piceIndex, 1, 2, FULL);
                    SetSpot(piceIndex, 2, 0, FULL); SetSpot(piceIndex, 2, 1, FULL); SetSpot(piceIndex, 2, 2, FULL);
                    break;
            }

            char[,] temp = new char[S.BLOCKSIZE, S.BLOCKSIZE];
            int rotation = (S.rand.Next(0, 4));

            for (int r = 0; r < rotation; r++)
            {
                // Clear temp
                for (int x = 0; x < S.BLOCKSIZE; x++)
                    for (int y = 0; y < S.BLOCKSIZE; y++)
                    {
                        temp[x, y] = S.EMPTY;
                    }

                for (int x = 0; x < S.BLOCKSIZE; x++)
                    for (int y = 0; y < S.BLOCKSIZE; y++)
                    {
                        temp[x, (S.BLOCKSIZE - 1) - y] = GetSpot(piceIndex,x,y);
                    }

                // Copy back from temp to pice
                for (int x = 0; x < S.BLOCKSIZE; x++)
                    for (int y = 0; y < S.BLOCKSIZE; y++)
                        SetSpot(piceIndex, x, y, temp[x, y]);
            }

            //vypocitat jak moc zarovnat nahoru a doleva
            int minX = S.BLOCKSIZE, minY = S.BLOCKSIZE;
            for (int x = 0; x < S.BLOCKSIZE; x++)
                for (int y = 0; y < S.BLOCKSIZE; y++)
                {
                    if (GetSpot(piceIndex,x, y) != S.EMPTY)
                    {
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                    }
                }


            // zarovnat nahoru a doleva
            for (int x = 0; x < S.BLOCKSIZE; x++)
                for (int y = 0; y < S.BLOCKSIZE; y++)
                {
                    char val = GetSpot(piceIndex, x, y);
                    SetSpot(piceIndex, x, y, S.EMPTY);
                    if (val != S.EMPTY)
                    {
                        int newX = x - minX;
                        int newY = y - minY;

                        SetSpot(piceIndex, newX, newY, val); // preserve color!
                    }
                }
        }
        public void GenerateAllPices()
        {
            for (int i = 0; i < S.BLOCKCOUNT; i++) GeneratePice(i);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  //optimalizace
        public char GetSpot(int pice ,int x, int y)
        {
            if (x < 0 || x >= S.BLOCKSIZE) return S.FULL;
            if (y < 0 || y >= S.BLOCKSIZE) return S.FULL;
            if (pice < 0 || pice >= S.BLOCKCOUNT) return S.FULL;
            return data[pice,x, y];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]   //optimalizace
        public bool SetSpot(int pice, int x, int y, char ch)
        {
            if (x < 0 || x >= S.BLOCKSIZE) return false;
            if (y < 0 || y >= S.BLOCKSIZE) return false;
            if (pice < 0 || pice >= S.BLOCKCOUNT) return false;
            data[pice, x, y] = ch;
            return true;
        }
        public void DrawWithOutlines()
        {
            for (int y = 0; y < S.BLOCKSIZE + 2; y++)
            {
                if (y == 0 || y == S.BLOCKSIZE + 1)
                {
                    for (int b = 0; b < S.BLOCKCOUNT; b++)
                    {
                        Console.Write(y == 0 ? "┌" : "└");
                        for (int x = 0; x < S.BLOCKSIZE * S.PICEWIDTH; x++)
                            Console.Write("-");
                        Console.Write(y == 0 ? "┐" : "┘");
                    }
                    Console.Write("\n");
                    continue;
                }

                for (int b = 0; b < S.BLOCKCOUNT; b++)
                {
                    Console.Write("|");
                    for (int x = 0; x < S.BLOCKSIZE; x++)
                    {
                        DrawSquare(b, x, y-1);
                    }
                    Console.Write("|");
                }
                Console.Write("\n");
            }
        }
        public void Draw()
        {
            Console.WriteLine();
            for (int y = 0; y < S.BLOCKSIZE; y++)
            {
                for (int b = 0; b < S.BLOCKCOUNT; b++)
                {
                    for (int x = 0; x < S.BLOCKSIZE; x++)
                    {
                        DrawSquare(b, x, y);
                    }
                    for (int i = 0; i < S.PICEWIDTH; i++) Console.Write(' ');
                }
                Console.WriteLine();
            }
        }
        void DrawSquare(int pice, int x, int y)
        {
            char spot = GetSpot(pice, x, y);

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
        public bool IsAPiceInSlot(int slotIndex)
        {
            for (int x = 0; x < S.BLOCKSIZE; x++)
                for (int y = 0; y < S.BLOCKSIZE; y++)
                {
                    if (GetSpot(slotIndex, x, y) != S.EMPTY) return true;
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
    }
}
