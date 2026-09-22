using ILGPU;
using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace BlockBlast7
{
    public static class GpuKernels
    {
        public static void EvolutionKernel(
            Index2D index, 
            ArrayView2D<int, Stride2D.DenseX> resultsView,
            GpuCosts baseCost,
            int seedOffset)
        {
            int mutationIndex = index.X;
            int gameIndex = index.Y;
            
            GpuCosts localCosts = baseCost;                     // 1. Give this specific thread its own local copy of the costs
            localCosts.MutatePredictabely(mutationIndex);       // 2. Apply the mutation based on its X coordinate
            uint seed = (uint)(gameIndex + seedOffset);         // 3. Generate a unique, non-zero seed based on the Y coordinate
            GpuGame game = new GpuGame(localCosts, seed);       // 4. Create the game on the GPU thread's stack
            resultsView[index] = game.PlayAndReturnMoveCount(); // 5. Play the game and save the score to the global grid
        }
    }
    public class Evolution
    {
        public GpuCosts baseCost = new GpuCosts();

        private Accelerator accelerator;
        private Action<Index2D, ArrayView2D<int, Stride2D.DenseX>, GpuCosts, int> kernelLauncher;

        public Evolution(Accelerator gpuAccelerator)
        {
            this.accelerator = gpuAccelerator;

            this.kernelLauncher = accelerator.LoadAutoGroupedStreamKernel<
                Index2D,
                ArrayView2D<int, Stride2D.DenseX>,
                GpuCosts,
                int>(GpuKernels.EvolutionKernel);
        }
        /// <summary> pokud nastavis howManyTimes na -1 evoluce bude probihat do nekonecna | pokud nastavis seed offset na 0 tak bude random </summary>
        public void EvolveRepeatedly(int sampleSize = 10, int howManyTimes = -1, int seedOffset = 0)
        {
            seedOffset = (seedOffset == 0) ? Environment.TickCount : seedOffset;

            Console.WriteLine("################################################");
            Console.WriteLine($"Running on: {accelerator.Name}");
            Console.Write($"Base costs: ");
            baseCost.PrintValues();
            Console.WriteLine($"SeedOffset: {seedOffset}");
            Console.WriteLine($"Begin Testing With: sampleSize: {sampleSize}");
            Console.WriteLine($"Testing {S.MUTATIONCOUNT} mutations");
            Console.WriteLine($"Testing with {S.BLOCKCOUNT} block count (standard is 3)");
            if (S.WIDTH == 8 && S.HEIGHT == 8 && S.MUTATIONCOUNT == 24 && S.BLOCKCOUNT == 3) {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.WriteLine("VALID");
                Console.BackgroundColor = ConsoleColor.Black;
            } else {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("INVALID");
                Console.BackgroundColor = ConsoleColor.Black;
            }
            Console.WriteLine("################################################");
            Console.WriteLine();

            for (int i = 0; i != howManyTimes; i++)
            {
                Console.WriteLine($"---------------------------------- {i + 1}/{howManyTimes} ----------------------------------");

                Evolve(sampleSize, seedOffset);

                string filePath = "BlockCount" + S.BLOCKCOUNT + ".txt";
                string content = $"Mutation {i}\n" + baseCost.GetStringOfCosts();

                // This creates the file if it doesn't exist, or overwrites it if it does.
                File.WriteAllText(filePath, content);
            }

            Console.WriteLine("Finished " + howManyTimes + " tests");
        }
        public void Evolve(int sampleSize = 10, int seedOffset = 1)
        {
            float[] mutationTotalScores = new float[S.MUTATIONCOUNT];

            using var d_results = accelerator.Allocate2DDenseX<int>(new Index2D(S.MUTATIONCOUNT, sampleSize));
            var launchIndex = new Index2D(S.MUTATIONCOUNT, sampleSize);

            kernelLauncher(launchIndex, d_results.View, baseCost, seedOffset);
            accelerator.Synchronize();

            int[,] hostBatch = d_results.GetAsArray2D();

            for (int m = 0; m < S.MUTATIONCOUNT; m++)
            {
                for (int s = 0; s < sampleSize; s++)
                {
                    mutationTotalScores[m] += hostBatch[m, s];
                }
            }

            int bestAvgIndex = 0;
            float bestAvgScore = -1f;

            for (int m = 0; m < S.MUTATIONCOUNT; m++)
            {
                float avg = mutationTotalScores[m] / sampleSize;
                if (avg > bestAvgScore)
                {
                    bestAvgScore = avg;
                    bestAvgIndex = m;
                }
            }

            if (bestAvgScore <= 0)
            {
                Console.WriteLine("ERROR: Best avg found is 0");
                return;
            }

            unsafe
            {
                float mult = (bestAvgIndex >= S.COST_COUNT) ? (1f / S.MUTATIONMULT) : S.MUTATIONMULT;

                float changedValue = baseCost.arrayOfValues[bestAvgIndex % S.COST_COUNT];

                Console.WriteLine($"Multiplied: {changedValue.ToString(CultureInfo.InvariantCulture)}" +
                    $" by {mult.ToString(CultureInfo.InvariantCulture)}" +
                    $" => {(mult * changedValue).ToString(CultureInfo.InvariantCulture)}" +
                    $"    (avg: {bestAvgScore.ToString(CultureInfo.InvariantCulture)})");
            }
            baseCost.MutatePredictabely(bestAvgIndex);
            baseCost.PrintValues();
        }
    }
}