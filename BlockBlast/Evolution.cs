using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace console_blockBlast_AI
{
    public class Evolution
    {
        Costs baseCost = new Costs();
        /// <summary>seedOffset == -1 -> allways random seed</summary>
        public void EvolveRepeatedly(int howManyTimes, int sampleSize = 10, int seedOffset = 0)
        {
            baseCost.PrintValues();

            for(int i = 0; i< howManyTimes;i++)
            {
                Console.WriteLine($"@ ({i+1}/{howManyTimes}) @");
                if(seedOffset == -1) Evolve(sampleSize, S.rand.Next());
                else Evolve(sampleSize,seedOffset);
            }
        }
        public void Evolve(int sampleSize = 10, int seedOffset = 0)
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine($"Begin Testing With: sampleSize: {sampleSize}, seedOffset: {seedOffset}");
            Costs bestFound = new Costs();
            int bestAvgIndex = 0;
            float[] allAvgs = new float[S.EVOLUTIONPATHS];
            for (int i = 0; i < S.EVOLUTIONPATHS; i++)
            {
                //Console.Write($"Evolve test ({i + 1}/" + S.EVOLUTIONPATHS + ") ");
                Console.Write(".");
                Costs testCosts = new Costs(baseCost);

                testCosts.MutatePredictabely(i);

                float avg = ReturnAvgScore(testCosts, sampleSize, seedOffset);
                if (avg > allAvgs[bestAvgIndex])
                {
                    bestAvgIndex = i;
                    bestFound = new Costs(testCosts);
                }
                allAvgs[i] = avg;
            }
            baseCost = new Costs(bestFound);
            Console.WriteLine();
            baseCost.MutatePredictabelyPrint(bestAvgIndex);
            Console.WriteLine(" avg: " + allAvgs[bestAvgIndex].ToString(CultureInfo.InvariantCulture));
            baseCost.PrintValues();
            Console.WriteLine("----------------------------------");
        }
        private float ReturnAvgScore(Costs c, int sampleSize = 10, int seedOffset = 0)
        {
            //c.PrintValues();
            int[] results = new int[sampleSize];
            for (int i = 0; i < sampleSize; i++)
            {
                Game game = new Game(i + seedOffset, c);
                results[i] = game.PlayAndReturnMoveCount();
            }

            float avg = 0;
            for (int i = 0; i < sampleSize; i++)
                avg += results[i];
            avg /= sampleSize;

            //Console.WriteLine("avg: " + avg);

            return avg;
        }
    }
    public class Costs
    {
        //public float fullCost = 50;
        //public float[] unsimularity = { 1, 3, 20, 27, 81 };

        public float fullCost = 88.57806f;
        public float[] unsimularity = { 0.3504938f, 1.3995218f, 12.418425f, 57.876907f, 107.811005f };

        static float MUTATIONMULT = 1.1f;//play around with
        public Costs() { }
        public Costs(Costs costs)
        {
            fullCost = costs.fullCost;
            unsimularity = (float[])costs.unsimularity.Clone();
        }
        public Costs(float fullCost, float[] unsimularity)
        {
            this.fullCost = fullCost;
            this.unsimularity = unsimularity;
        }
        public void MutateRandomly()
        {
            float mult = S.rand.Next(0, 2) == 1 ? MUTATIONMULT : 1f / MUTATIONMULT;

            switch (S.rand.Next(0, S.EVOLUTIONPATHS / 2))
            {
                case 0:
                    unsimularity[0] *= mult;
                    break;
                case 1:
                    unsimularity[1] *= mult;
                    break;
                case 2:
                    unsimularity[2] *= mult;
                    break;
                case 3:
                    unsimularity[3] *= mult;
                    break;
                case 4:
                    unsimularity[4] *= mult;
                    break;
                case 5:
                    fullCost *= mult;
                    break;
            }
        }
        public void MutatePredictabely(int predictNumber)
        {
            float mult = predictNumber >= S.EVOLUTIONPATHS / 2 ? MUTATIONMULT : 1f / MUTATIONMULT;

            switch (predictNumber % (S.EVOLUTIONPATHS / 2))
            {
                case 0:
                    unsimularity[0] *= mult;
                    break;
                case 1:
                    unsimularity[1] *= mult;
                    break;
                case 2:
                    unsimularity[2] *= mult;
                    break;
                case 3:
                    unsimularity[3] *= mult;
                    break;
                case 4:
                    unsimularity[4] *= mult;
                    break;
                case 5:
                    fullCost *= mult;
                    break;
            }
        }
        public void MutatePredictabelyPrint(int predictNumber)
        {
            float mult = predictNumber >= S.EVOLUTIONPATHS / 2 ? MUTATIONMULT : 1f / MUTATIONMULT;
            Console.Write("Multiplied ");

            switch (predictNumber % (S.EVOLUTIONPATHS / 2))
            {
                case 0:
                    Console.Write("unsimularity[0]");
                    break;
                case 1:
                    Console.Write("unsimularity[1]");
                    break;
                case 2:
                    Console.Write("unsimularity[2]");
                    break;
                case 3:
                    Console.Write("unsimularity[3]");
                    break;
                case 4:
                    Console.Write("unsimularity[4]");
                    break;
                case 5:
                    Console.Write("fullCost");
                    break;
            }
            Console.Write(" by " + mult.ToString(CultureInfo.InvariantCulture));

        }
        public void PrintValues()
        {
            Console.WriteLine($"        public float fullCost = {fullCost.ToString(CultureInfo.InvariantCulture)}f;");
            Console.WriteLine($"        public float[] unsimularity = {{" +
                $"{unsimularity[0].ToString(CultureInfo.InvariantCulture)}f, " +
                $"{unsimularity[1].ToString(CultureInfo.InvariantCulture)}f, " +
                $"{unsimularity[2].ToString(CultureInfo.InvariantCulture)}f, " +
                $"{unsimularity[3].ToString(CultureInfo.InvariantCulture)}f, " +
                $"{unsimularity[4].ToString(CultureInfo.InvariantCulture)}f }};");
        }
    }
}
