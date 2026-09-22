using console_blockBlast_AI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace console_blockBlast_AI
{
    public static class S //Static
    {
        public static char FULL = '#';
        public static char EMPTY = ' ';
        public static int WIDTH = 8;
        public static int HEIGHT = 8;
        public static int BLOCKCOUNT = 3;// - 2;
        public static int BLOCKSIZE = 5;
        public static int PICEWIDTH = 2;
        //public static int EVOLUTIONPATHS = 12;
        public static int ANIMATIONDELAYMS = 200;
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
            return colorString[rand.Next(0, colorString.Length)];
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
