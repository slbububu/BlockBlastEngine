using System.ComponentModel.DataAnnotations;

ulong pice = 0;
int cursorX = 0;
int cursorY = 0;
char input = '\0';
ulong []all = { 
1539, 66306, 1539, 66306,// Z-Shape
774, 131841, 774, 131841,// S-Shape
519, 131842, 1794, 66305,// T-Shape
196865, 1796, 131587, 263,// L-Shape (2x3)
197122, 1793, 65795, 1031,// L-Shape (2x3, flipped)
459009, 65799, 263175, 459780,// L longleg (3x3)
16843009, 15, 16843009, 15,// 1x4 Stick
4311810305, 31, 4311810305, 31,// 1x5 Stick
771, 771, 771, 771,// 2x2 Square
1799, 197379, 1799, 197379,// 2x3 Big Block
460551, 460551, 460551, 460551,// 3x3 Massive Square
};
#region print
void PrintSwitchPices()
{
    Console.WriteLine("Pices:");

    for (int i = 0; i < all.Length; i++)
    {
        Console.Write($"{i} => {all[i]}, ");
        if (i % 4 == 3) Console.WriteLine();
    }
    Console.WriteLine("_ => 0");
}
void PrintSwitchWith()
{
    Console.WriteLine("With:");

    for (int i = 0; i < all.Length; i++)
    { 
        byte w = 0;
        for (int x = 0; x < 8; x++)
        {
            if ((all[i] & (0x0101010101010101UL << x)) != 0) w = (byte)(x + 1);
        }
        Console.Write($"{i} => {w}, ");
        if (i % 4 == 3) Console.WriteLine();

    }

    Console.WriteLine("_ => 0");
}
void PrintSwitchHeight()
{
    Console.WriteLine("Height:");

    for (int i = 0; i < all.Length; i++)
    { 
        byte h = 0;
        for (int y = 0; y < 8; y++)
        {
            if ((all[i] & (0x00000000000000FFUL << (y * 8))) != 0) h = (byte)(y + 1);
        }
        Console.Write($"{i} => {h}, ");
        if (i % 4 == 3) Console.WriteLine();
    }

    Console.WriteLine("_ => 0");
}
#endregion print

#region pice
void SetPice(int x, int y,bool val)
{
    ulong mask = 1;
    mask <<= y * 8 + x;
    if(val) pice |= mask;
    else pice &= ~mask;
}
bool GetPice(int x, int y)
{
    ulong mask = 1;
    mask <<= y * 8 + x;
    mask &= pice;
    return mask != 0;
}
void DrawPice()
{
    //draw map
    for (int x = 0; x < 8; x++) Console.Write("--");
    Console.WriteLine();
    for (int y = 0; y < 8; y++)
    {
        for (int x = 0; x < 8; x++)
        {
            if (GetPice(x, y)) Console.BackgroundColor = ConsoleColor.DarkGreen;
            
            if(x == cursorX && y == cursorY) Console.Write("()");
            else Console.Write("  ");

            Console.BackgroundColor = ConsoleColor.Black;
        }
        Console.WriteLine("|");
    }
    for (int x = 0; x < 8; x++) Console.Write("--");
    Console.WriteLine();
}
#endregion pice

while (true)
{
    Console.Clear();

    switch (input)
    {
        case 'a':
            cursorX--;
            break;
        case 'd':
            cursorX++;
            break;
        case 's':
            cursorY++;
            break;
        case 'w':
            cursorY--;
            break;
        case ' ':
            SetPice(cursorX, cursorY, true);
            break;
        case 'c':
            SetPice(cursorX, cursorY, false);
            break;
        case 'e':
            pice = 0;
            break;
        case 'l':
            pice = uint.Parse(Console.ReadLine());
            break;
        case 'p':
            PrintSwitchPices();
            break;
        case 'v':
            PrintSwitchWith();
            break;
        case 'h':
            PrintSwitchHeight();
            break;
    }
    Console.WriteLine("Cursor: " + cursorX + " " + cursorY);
    Console.WriteLine("PiceVal: " + pice);
    DrawPice();
    input = Console.ReadKey().KeyChar;
}