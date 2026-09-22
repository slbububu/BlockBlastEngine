using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using static System.Net.Mime.MediaTypeNames;

namespace console_blockBlast_AI
{
    public class Game
    {
        public Random rand;
        public Map map;
        public Pice pice;
        public bool gameOver = false;
        public int moveCount = 0;
        private int seed = 0;
        public Game(int seed)
        {
            this.seed = seed;
            rand = new Random(seed);
            map = new Map();
            pice = new Pice(rand);
        }
        public Game()
        {
            this.seed = Environment.TickCount;  //toto delam jen abych vedel seed
            rand = new Random(seed);
            map = new Map();
            pice = new Pice(rand);
        }
        public Game(Game g)
        {
            this.map = new Map(g.map);      //ulink the data
            this.pice = new Pice(rand, g.pice);
        }
        static bool clearNextFrame = false;
        char whritenChar = '\0';
        static public int x = 0;
        static public int y = 0;
        static public int selectedSpace = 0;
        bool playTillDeath = false;
        static public bool respwanPices = true;
        public void PlayWithUI()
        {
            while (true)
            {
                if (whritenChar == 'p')
                {
                        playTillDeath = true;
                    whritenChar = '\r';
                    Console.WriteLine("Playing until death");
                    clearNextFrame = true;
                }
                if (whritenChar == ' ')
                {
                    if (selectedSpace == 0)
                        map.SetSpot(x, y, S.FULL);
                    else pice.SetSpot(selectedSpace - 1, x, y, S.FULL);
                }
                if ('0' <= whritenChar && whritenChar <= '0' + S.BLOCKCOUNT) selectedSpace = (int)whritenChar - '0';
                if (whritenChar == 'c')
                {
                    if (selectedSpace == 0)
                        map.SetSpot(x, y, S.EMPTY);
                    else pice.SetSpot(selectedSpace - 1, x, y, S.EMPTY);
                }
                if (whritenChar == 'g') pice.GenerateAllPices();
                if (whritenChar == '\r') DoTheBestMove(true);
                if (whritenChar == 'v') map.DoLineClears();
                if (whritenChar == '\r' && !gameOver) Thread.Sleep(S.ANIMATIONDELAYMS);
                if (whritenChar == 'w') y--;
                if (whritenChar == 's') y++;
                if (whritenChar == 'a') x--;
                if (whritenChar == 'd') x++;
                if (selectedSpace == 0)
                {
                    if (y < 0) y = S.HEIGHT - 1;
                    if (y >= S.HEIGHT) y = 0;
                    if (x < 0) x = S.WIDTH - 1;
                    if (x >= S.WIDTH) x = 0;
                }
                else
                {
                    if (y < 0) y = S.BLOCKSIZE - 1;
                    if (y >= S.BLOCKSIZE) y = 0;
                    if (x < 0) x = S.BLOCKSIZE - 1;
                    if (x >= S.BLOCKSIZE) x = 0;
                }
                if (whritenChar == 'q')
                {
                    clearNextFrame = true;
                    Console.WriteLine();
                    return;
                }
                if (whritenChar == 'r')
                {
                    clearNextFrame = true;

                    Console.Write("New game with seed: ");
                    try {
                        seed = int.Parse(Console.ReadLine());
                        Console.WriteLine("Succes");
                        Thread.Sleep(S.INFOMESSAGEDELAYMS);

                        Game game = new Game(seed);
                        game.PlayWithUI();
                        return;
                    }
                    catch { Console.WriteLine("couldnt read that");}
                }
                if (whritenChar == 'n')
                {
                    clearNextFrame = true;
                    Game game = new Game();
                    game.PlayWithUI();
                    return;
                }
                if (whritenChar == '+') PlacePice(0, x, y);
                if (whritenChar == 'ě') PlacePice(1, x, y);
                if (whritenChar == 'š') PlacePice(2, x, y);
                if (whritenChar == '*') moveCount++;
                if (whritenChar == '-') moveCount--;
                Update();
                if (whritenChar == 'j') pice.ClearPices();
                DrawGame();
                if (whritenChar == 'f')
                {
                    clearNextFrame = true;
                    respwanPices = !respwanPices;
                    Console.WriteLine("respwanPices is now " + (respwanPices ? "on" : "off"));
                }
                if (whritenChar == 'm')
                {
                    clearNextFrame = true;
                    Console.WriteLine("Best Score: " + BestScore());
                }
                if (whritenChar == 'b')
                { 
                    clearNextFrame = true;
                    Console.WriteLine("BoardScore: " + map.GetScore());
                }
                if (whritenChar == 'i')
                {
                    clearNextFrame = true;
                    Console.WriteLine("Island Count: " + map.GetIslandCount());
                }
                if (whritenChar == 'e')
                {
                    clearNextFrame = true;
                    Console.WriteLine("Seed: " + seed);
                }
                if (whritenChar == 'h')
                {
                    clearNextFrame = true;
                    Console.WriteLine();
                    Console.WriteLine("\"enter\" - ai plays");
                    Console.WriteLine("wsad - move cursor");
                    Console.WriteLine("space - place block");
                    Console.WriteLine("c - clear block");
                    Console.WriteLine("0 - place into map");
                    Console.WriteLine("1 - place into pice 1");
                    Console.WriteLine("2 - place into pice 2");
                    Console.WriteLine("3 - place into pice 3");
                    Console.WriteLine("+ - place pice 1");
                    Console.WriteLine("ě - place pice 2");
                    Console.WriteLine("š - place pice 3");
                    Console.WriteLine("* - moveCount++");
                    Console.WriteLine("- - moveCount--");
                    Console.WriteLine("f - toggle pice respawnebility");
                    Console.WriteLine("j - clear pices");
                    Console.WriteLine("v - clear lines");
                    Console.WriteLine("m - best calculated score");
                    Console.WriteLine("b - board score");
                    Console.WriteLine("g - generate new pices");
                    Console.WriteLine("i - island count");
                    Console.WriteLine("p - play until game over");
                    Console.WriteLine("q - quit current game");
                    Console.WriteLine("e - show seed");
                    Console.WriteLine("r - set seed game");
                    Console.WriteLine("n - new game");
                }

                if (!playTillDeath || gameOver)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    whritenChar = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }
        public int PlayAndReturnMoveCount()
        {
            for (int ret = 0;; ret++)
            {
                Update();
                DoTheBestMove();

                if (gameOver) return ret;
            }
        }
        public void DrawGame()
        {
            if (clearNextFrame || gameOver) Console.Clear(); 
            else Console.SetCursorPosition(0,0);
            clearNextFrame = false;

            map.Draw();
            pice.Draw();
            DrawHud();
            if (gameOver) PrintGameOver();
        }
        public void DrawHud()
        {
            Console.WriteLine("Move Count: " + moveCount);
            Console.WriteLine("h - help");
        }
        public void Update()
        {
            if (respwanPices && pice.AllSlotsEmpty())
            {
                pice.GenerateAllPices();
            }
        }
        bool CanIPlace(int piceIndex, int x, int y)
        {
            for (int py = 0; py < S.BLOCKSIZE; py++)
            {
                for (int px = 0; px < S.BLOCKSIZE; px++)
                {
                    if (pice.GetSpot(piceIndex,px,py) != S.EMPTY)
                    {
                        if (map.IsFull(x + px, y + py))
                            return false;
                    }
                }
            }
            return true;
        }
        public bool PlacePice(int piceIndex, int x, int y,bool doAnimation = false)
        {
            if (!CanIPlace(piceIndex, x, y)) return false;

            for (int px = 0; px < S.BLOCKSIZE; px++)
            {
                for (int py = 0; py < S.BLOCKSIZE; py++)
                {
                    char picePixel = pice.GetSpot(piceIndex, px, py);
                    if (picePixel != S.EMPTY)
                    {
                        map.SetSpot(x + px, y + py, picePixel);
                    }
                }
            }
            pice.ClearPice(piceIndex);
            if (doAnimation && map.ShouldClear()) //poukud je clear udelam animaci
            {
                DrawGame();
                Thread.Sleep(S.ANIMATIONDELAYMS);
            }
            map.DoLineClears();
            moveCount++;
            return true;
        }
        public void DoTheBestMove(bool doAnimation = false)
        {
            float bestScore = float.MaxValue;
            int bestPiceIndex = -1, bestToX = 0, bestToY = 0;

            for (int i = 0; i < S.BLOCKCOUNT; i++)
            {
                if (pice.IsAPiceInSlot(i))
                {
                    for (int x = 0; x < S.WIDTH; x++)
                        for (int y = 0; y < S.HEIGHT; y++)
                        {
                            Game recursiveGame = new Game(this);
                            if (recursiveGame.PlacePice(i, x, y))
                            {
                                float score = recursiveGame.BestScore();
                                if (score < bestScore)
                                {
                                    bestScore = score;
                                    bestPiceIndex = i; bestToX = x; bestToY = y;
                                }
                            }
                        }
                }
            }
            gameOver = (bestScore == float.MaxValue);
            PlacePice(bestPiceIndex, bestToX, bestToY, doAnimation);
        }
        public float BestScore()
        {
            float bestScore = float.MaxValue;
            bool AllSlotsEmpty = true;

            for (int i = 0; i < S.BLOCKCOUNT; i++)
            {
                if (pice.IsAPiceInSlot(i))
                {
                    AllSlotsEmpty = false;
                    for (int x = 0; x < S.WIDTH; x++)
                        for (int y = 0; y < S.HEIGHT; y++)
                        {
                            Game recursiveGame = new Game(this);
                            if (recursiveGame.PlacePice(i, x, y))
                            {
                                float score = recursiveGame.BestScore();
                                if(score < bestScore) bestScore = score;
                            }
                        }
                }
            }
            if (AllSlotsEmpty) //v sachach napr by tu bylo if (moveDeopth > 3)
                return map.GetScore();
            else
                return bestScore;
        }
        private void PrintGameOver()
        {
            Console.WriteLine("  ____                         ___                 ");
            Console.WriteLine(" / ___| __ _ _ __ ___   ___   / _ \\__   _____ _ __ ");
            Console.WriteLine("| |  _ / _` | '_ ` _ \\ / _ \\ | | | \\ \\ / / _ \\ '__|");
            Console.WriteLine("| |_| | (_| | | | | | |  __/ | |_| |\\ V /  __/ |   ");
            Console.WriteLine(" \\____|\\__,_|_| |_| |_|\\___|  \\___/  \\_/ \\___|_|   ");
            Console.WriteLine($"Seed: {seed}");
            Console.WriteLine("n - new game");
        }
    }
}
