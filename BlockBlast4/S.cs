using console_blockBlast_AI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace console_blockBlast_AI
{
    public static class S //Static
    {
        public const char FULL = '#';
        public const char EMPTY = ' ';
        public const int WIDTH = 8;
        public const int HEIGHT = 8;
        public const int BOARDLENGHT = WIDTH * HEIGHT;
        public const int BLOCKCOUNT = 3-2;// - 2;
        public const int BLOCKSIZE = 5;
        public const int BLOCKLENGHT = BLOCKSIZE * BLOCKSIZE;
        public const int PICEWIDTH = 2;
        public const int COST_COUNT = 6;    //kolik ruznejch costs existuje
        public const float MUTATIONMULT = 1.1f;
        public const int ANIMATIONDELAYMS = 200;
        private static int NEXTCOLORINDEX = -1;
        private static char[] colorString = { 'R', 'G', 'B', 'C', 'M' };
        public static void ChangeColorTo(char colorCode)
        {
            switch (colorCode)
            {
                default:
                    Console.BackgroundColor = ConsoleColor.Green;
                    break;
                case 'R':
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    break;
                case 'G':
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    break;
                case 'B':
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    break;
                case 'C':
                    Console.BackgroundColor = ConsoleColor.DarkCyan;
                    break;
                case 'M':
                    Console.BackgroundColor = ConsoleColor.DarkMagenta;
                    break;
            }
        }
        public static char GetRandomColor() //mozna presunout do Static classy
        {
            return colorString[S.NextRandom() % colorString.Length];
        }
        public static char GetNextColor()
        {
            if (NEXTCOLORINDEX == -1)
                NEXTCOLORINDEX = S.NextRandom() % colorString.Length;
            else NEXTCOLORINDEX++;

            return colorString[NEXTCOLORINDEX % colorString.Length];
        }

        public static uint rngState = 1337;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSeed(uint seed)
        {
            if (seed == 0) rngState = 1337;
            else rngState = seed;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSeed(int seed)
        {
            if ((uint)seed == 0) rngState = 1337;
            else rngState = (uint)seed;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextRandom()
        {
            uint x = rngState;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            rngState = x;
            return (int)x;
        }
    }
}
