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

            for (int i = 0; i < howManyTimes; i++)
            {
                Console.WriteLine($"---------------------------------- ({i + 1}/{howManyTimes}) ----------------------------------");
                if (seedOffset == -1) Evolve(sampleSize, S.rand.Next());
                else Evolve(sampleSize, seedOffset);
            }
        }
        public void Evolve(int sampleSize = 10, int seedOffset = 0)
        {
            Console.Write($"SeedOffset: {seedOffset}, ");
            Console.Write($"Begin Testing With: sampleSize: {sampleSize}, ");
            Console.Write($"Testing {baseCost.arrayOfValues.Length * 2} mutations:  ");
            Costs bestFound = new Costs();
            int bestAvgIndex = 0;
            float[] allAvgs = new float[baseCost.arrayOfValues.Length * 2];
            for (int i = 0; i < baseCost.arrayOfValues.Length * 2; i++)
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
            Console.WriteLine("    avg: " + allAvgs[bestAvgIndex].ToString(CultureInfo.InvariantCulture));
            baseCost.PrintValues();
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
        public float[] arrayOfValues;

        static float MUTATIONMULT = 1.1f;//play around with

        /// <summary> Most evolved configuration </summary>
        public Costs()
        {
            this.arrayOfValues = new float[] {
                //1, 3, 20, 27, 81, 50
                0.3504938f, 1.3995218f, 12.418425f, 57.876907f, 107.811005f, 88.57806f
            };
        }
        public Costs(float[] arrayOfValues)
        {
            this.arrayOfValues = arrayOfValues;
            Console.WriteLine(arrayOfValues.Length);
        }
        public Costs(Costs costs)
        {
            arrayOfValues = (float[])costs.arrayOfValues.Clone();
        }
        public void MutatePredictabely(int predictNumber)
        {
            float mult;
            if (predictNumber >= arrayOfValues.Length / 2) mult = MUTATIONMULT;
            else mult = 1f / MUTATIONMULT;

            int indexToMutate = predictNumber % (arrayOfValues.Length / 2);
            arrayOfValues[indexToMutate] *= mult;
        }
        public void MutatePredictabelyPrint(int predictNumber)
        {
            float mult;
            if (predictNumber >= arrayOfValues.Length) mult = MUTATIONMULT;
            else mult = 1f / MUTATIONMULT;

            int indexToMutate = predictNumber % (arrayOfValues.Length);
            arrayOfValues[indexToMutate] *= mult;

            Console.Write("Multiplied " + arrayOfValues[indexToMutate]);
            Console.Write(" by " + mult.ToString(CultureInfo.InvariantCulture));
        }
        public void PrintValues()
        {
            bool first = true;
            foreach (float f in arrayOfValues)
            {
                if (!first) Console.Write(", ");
                Console.Write(f.ToString(CultureInfo.InvariantCulture));
                first = false;
            }
            Console.WriteLine();
        }
    }
}
