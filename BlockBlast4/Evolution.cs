using BlockBlast4;
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
        GpuCosts baseCost = new GpuCosts();
        /// <summary>seedOffset == -1 -> allways random seed</summary>
        public void EvolveRepeatedly(int howManyTimes, int sampleSize = 10, int seedOffset = 0)
        {
            baseCost.PrintValues();

            for (int i = 0; i < howManyTimes; i++)
            {
                Console.WriteLine($"---------------------------------- {i + 1}/{howManyTimes} ----------------------------------");
                if (seedOffset == 0) Evolve(sampleSize, S.NextRandom());
                else Evolve(sampleSize, seedOffset);
            }
        }
        public void Evolve(int sampleSize = 10, int seedOffset = 0)
        {
            Console.Write($"SeedOffset: {seedOffset}, ");
            Console.Write($"Begin Testing With: sampleSize: {sampleSize}, ");
            Console.Write($"Testing {S.COST_COUNT * 2} mutations:  ");
            GpuCosts bestFound = new GpuCosts();
            int bestAvgIndex = 0;
            float[] allAvgs = new float[S.COST_COUNT * 2];
            for (int i = 0; i < S.COST_COUNT * 2; i++)
            {
                Console.Write(".");
                GpuCosts testCosts = new GpuCosts(baseCost);

                testCosts.MutatePredictabely(i);

                float avg = ReturnAvgScore(testCosts, sampleSize, seedOffset);
                if (avg > allAvgs[bestAvgIndex])
                {
                    bestAvgIndex = i;
                    bestFound = new GpuCosts(testCosts);
                }
                allAvgs[i] = avg;
            }
            baseCost = new GpuCosts(bestFound);
            Console.WriteLine();
            baseCost.MutatePredictabelyPrint(bestAvgIndex);
            Console.WriteLine("    (avg: " + allAvgs[bestAvgIndex].ToString(CultureInfo.InvariantCulture) + ")");
            baseCost.PrintValues();
        }
        private float ReturnAvgScore(GpuCosts c, int sampleSize = 10, int seedOffset = 0)
        {
            //c.PrintValues();
            int[] results = new int[sampleSize];
            for (int i = 0; i < sampleSize; i++)
            {
                S.SetSeed(i + seedOffset);
                GpuGame game = new GpuGame(c);
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
}
