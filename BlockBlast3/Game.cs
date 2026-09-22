using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

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
        public Game(int seed, Costs costs)
        {
            this.seed = seed;
            rand = new Random(seed);
            map = new Map();
            pice = new Pice(rand);
            map.costs = costs;
        }
        public Game(int seed)
        {
            this.seed = seed;
            rand = new Random(seed);
            map = new Map();
            pice = new Pice(rand);
            map.costs = new Costs();
        }
        public Game(Costs costs)
        {
            this.seed = Environment.TickCount;  //toto delam jen abych vedel seed
            rand = new Random(seed);
            map = new Map();
            pice = new Pice(rand);
            map.costs = costs;
        }
        public Game()
        {
            this.seed = Environment.TickCount;  //toto delam jen abych vedel seed
            rand = new Random(seed);
            map = new Map();
            pice = new Pice(rand);
            map.costs = new Costs();
        }
        public Game(Game g)
        {
            this.map = new Map(g.map);      //ulink the data
            this.pice = new Pice(rand, g.pice);
            map.costs = g.map.costs;
        }
        char whritenChar = (char)0;
        int x = 0;
        int y = 0;
        bool playTillDeath = false;
        public void PlayWithUI()
        {
            while (true)
            {
                if (whritenChar == 'q') { Console.WriteLine(); return; }
                if (whritenChar == 'p')
                {
                        playTillDeath = true;
                    whritenChar = '\r';
                    Console.WriteLine("laying until death"); //u type the p for p-laying
                }
                if (whritenChar == ' ') map.SetSpot(x, y, S.FULL);
                if (whritenChar == 'v') map.SetSpot(x, y, S.EMPTY);
                if (whritenChar == 'g') pice.GenerateAllPices();
                if (whritenChar == '\r') DoTheBestMove(true);
                if (whritenChar == 'c') map.DoLineClears();
                Update();
                Draw();
                if (whritenChar == '\r' && !gameOver) Thread.Sleep(S.ANIMATIONDELAYMS);
                if (whritenChar == 'w') y--;
                if (whritenChar == 's') y++;
                if (whritenChar == 'a') x--;
                if (whritenChar == 'd') x++;
                Console.WriteLine("BoardScore: " + map.GetScore());
                Console.WriteLine("Type h for help");
                Console.WriteLine("Move Count: " + moveCount);
                Console.WriteLine("Cursor pos: " + x + " " + y);
                if (whritenChar == 'b') Console.WriteLine("Best Score: " + BestScore());
                if (whritenChar == 'i') Console.WriteLine("Island Count: " + map.GetIslandCount());
                if (whritenChar == 'e') Console.WriteLine("Seed: " + seed);
                if (whritenChar == 'h')
                {
                    Console.WriteLine();
                    Console.WriteLine("\"enter\" - ai plays");
                    Console.WriteLine("wsad - move cursor");
                    Console.WriteLine("space - place block");
                    Console.WriteLine("v - clear block");
                    Console.WriteLine("c - clear lines");
                    Console.WriteLine("b - best calculated score");
                    Console.WriteLine("g - generate new pices");
                    Console.WriteLine("i - island count");
                    Console.WriteLine("p - play until game over");
                    Console.WriteLine("q - quit current game");
                    Console.WriteLine("e - show seed");
                }

                if (!playTillDeath || gameOver)
                    whritenChar = Console.ReadKey().KeyChar;
            }
        }
        //tuto fci chci ultimatne dat na GPU
        public int PlayAndReturnMoveCount()
        {
            for (int ret = 0;; ret++)
            {
                Update();
                DoTheBestMove();

                if (gameOver) return ret;
            }
        }
        public void Draw()
        {
            Console.Clear(); //vypnuto pro debug
            map.Draw();
            pice.Draw();
            if(gameOver) PrintGameOver();
        }
        public void Update()
        {
            if (pice.AllSlotsEmpty())
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
                Draw();
                Thread.Sleep(S.ANIMATIONDELAYMS);
            }
            map.DoLineClears();
            moveCount++;
            return true;
        }
        public void DoTheBestMove(bool doAnimation = false)
        {
            int bestScore = int.MaxValue;
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
                                int score = recursiveGame.BestScore();
                                if (score < bestScore)
                                {
                                    bestScore = score;
                                    bestPiceIndex = i; bestToX = x; bestToY = y;
                                }
                            }
                        }
                }
            }
            gameOver = (bestScore == int.MaxValue);
            PlacePice(bestPiceIndex, bestToX, bestToY, doAnimation);
        }
        public int BestScore()
        {
            int bestScore = int.MaxValue;
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
                                int score = recursiveGame.BestScore();
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
        }
    }
}
