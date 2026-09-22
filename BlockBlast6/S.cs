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
        public const int BLOCKCOUNT = 3-1;// - 2;
        public const int BLOCKSIZE = 5;
        public const int BLOCKLENGHT = BLOCKSIZE * BLOCKSIZE;
        public const int PICEWIDTH = 2;
        public const int COST_COUNT = 12;    //kolik ruznejch costs existuje V
        public static float []defaultArrayOfCosts = { 0.4f, 1.4f, 12f, 70f, 60f, 100f, 101f, 61f, 71f, 13f, 2.4f, 1.4f};
        public const int MUTATIONCOUNT = COST_COUNT * 2;    //kolik ruznejch mutaci existuje
        public const float MUTATIONMULT = 2f;
        public const int ANIMATIONDELAYMS = 200;
        private static int NEXTCOLORINDEX = -1;
        private static char[] colorString = { 'R', 'G', 'B', 'C', 'M' };
        public static Random rand = new Random();
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
            return colorString[rand.Next(0,colorString.Length)];
        }
        public static char GetNextColor()
        {
            if (NEXTCOLORINDEX == -1)
                NEXTCOLORINDEX = rand.Next(0, colorString.Length);
            else NEXTCOLORINDEX++;

            return colorString[NEXTCOLORINDEX % colorString.Length];
        }
    }
}
