using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlockBlast7
{
    public static class S //Static
    {
        public const int WIDTH = 8; //toto se nesmi mnenit
        public const int HEIGHT = 8;    //toto musi byt aspon 5
        public const int BOARDLENGHT = WIDTH * HEIGHT;
        public const int BLOCKCOUNT = 3;//
        public const int BLOCKSIZE = 5;
        public const int BLOCKLENGHT = BLOCKSIZE * BLOCKSIZE;
        public const int DRAWPICEWIDTH = 2;
        public const byte EMPTY = byte.MaxValue;
        public const int COST_COUNT = 12;    //kolik ruznejch costs existuje (defaultArrayOfCosts)
        //first gen  0.4f, 1.4f, 12f, 100f, 60f, 100f, 101f, 61f, 101f, 13f, 2.4f, 1.4f
        public static float[] defaultArrayOfCosts = {
0.18206647f, 1.75f, 24f, 19.23077f, 60f, 130f, 77.69231f, 39.649998f, 25.25f, 6.933334f, 9.6f, 1.0769231f
        };
        public const int MUTATIONCOUNT = COST_COUNT * 2;    //kolik ruznejch mutaci existuje
        public const float MUTATIONMULT = 1.1f;
    }
}
