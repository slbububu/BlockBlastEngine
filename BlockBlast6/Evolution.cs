using BlockBlast4;
using ILGPU;
using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace console_blockBlast_AI
{
    public static class GpuKernels
    {
        // This runs on the GPU. No Console.Write allowed here!
        public static void EvolutionKernel(
            Index2D index, 
            ArrayView2D<int, Stride2D.DenseX> resultsView, // ...the view MUST be DenseX too!
            GpuCosts baseCost, 
            int seedOffset)
        {
            int mutationIndex = index.X;
            int gameIndex = index.Y;

            // 1. Give this specific thread its own local copy of the costs
            GpuCosts localCosts = baseCost;

            // 2. Apply the mutation based on its X coordinate
            localCosts.MutatePredictabely(mutationIndex);

            // 3. Generate a unique, non-zero seed based on the Y coordinate
            uint seed = (uint)(gameIndex + seedOffset + 1);

            // 4. Create the game on the GPU thread's stack
            GpuGame game = new GpuGame(localCosts, seed);

            // 5. Play the game and save the score to the global grid
            resultsView[index] = game.PlayAndReturnMoveCount();
        }
    }
    public class Evolution
    {
        public GpuCosts baseCost = new GpuCosts();

        private Accelerator accelerator;
        private Action<Index2D, ArrayView2D<int, Stride2D.DenseX>, GpuCosts, int> kernelLauncher;

        // Constructor requires the GPU accelerator to setup the kernel
        public Evolution(Accelerator gpuAccelerator)
        {
            this.accelerator = gpuAccelerator;

            // Pre-compile the kernel so it's lightning fast to call later
            this.kernelLauncher = accelerator.LoadAutoGroupedStreamKernel<
                Index2D,
                ArrayView2D<int, Stride2D.DenseX>,
                GpuCosts,
                int>(GpuKernels.EvolutionKernel);
        }
        public void EvolveRepeatedly(int howManyTimes, int sampleSize = 10, int seedOffset = 0)
        {
            Console.WriteLine("################################################");
            Console.Write($"Base costs: ");
            baseCost.PrintValues();
            Console.WriteLine($"Running on: {accelerator.Name}");
            Console.WriteLine($"SeedOffset: {seedOffset}");
            Console.WriteLine($"Begin Testing With: sampleSize: {sampleSize}");
            Console.WriteLine($"Testing {S.MUTATIONCOUNT} mutations");
            Console.WriteLine($"Testing with {S.BLOCKCOUNT} block count (standard is 3)");
            Console.WriteLine("################################################");
            Console.WriteLine();

            for (int i = 0; i < howManyTimes; i++)
            {
                Console.WriteLine($"---------------------------------- {i + 1}/{howManyTimes} ----------------------------------");

                // You can use your S class or Environment.TickCount for random seed offsets
                int currentOffset = (seedOffset == -1) ? Environment.TickCount : seedOffset;

                Evolve(sampleSize, currentOffset);
            }
        }

        public void Evolve(int sampleSize = 10, int seedOffset = 0)
        {
            //potom abych mohl zvysovat volne sample size muzu to posilat v kouscich (5 samples at a time)

            // 1. ALLOCATE MEMORY ON GPU
            // Create a 2D array: [Mutations][Games]
            var index2D = new Index2D(S.MUTATIONCOUNT, sampleSize);
            using var d_results = accelerator.Allocate2DDenseX<int>(new Index2D(index2D.X,index2D.Y));
            // 2. LAUNCH THE KERNEL
            // This runs all (numMutations * sampleSize) games AT THE SAME TIME
            kernelLauncher(index2D, d_results.View, baseCost, seedOffset);
            // 3. WAIT FOR GPU AND COPY RESULTS TO CPU
            accelerator.Synchronize();
            int[,] hostResults = d_results.GetAsArray2D();
            // 4. FIND THE BEST MUTATION ON THE CPU
            int bestAvgIndex = 0;
            float bestAvgScore = -1f;
            float[] allAvgs = new float[S.MUTATIONCOUNT];

            for (int m = 0; m < S.MUTATIONCOUNT; m++)
            {
                float totalScore = 0;
                for (int g = 0; g < sampleSize; g++)
                {
                    totalScore += hostResults[m, g];
                }

                float avg = totalScore / sampleSize;
                allAvgs[m] = avg;

                if (avg > bestAvgScore)
                {
                    bestAvgScore = avg;
                    bestAvgIndex = m;
                }
            }

            if(bestAvgScore <= 0)
            {
                Console.WriteLine("ERROR: Best avg found is 0, so the program isnt optimized enough to run this configuration");
                return;
            }

            // 5. APPLY THE BEST MUTATION TO OUR BASE COSTS
            unsafe
            {
                float mult = (bestAvgIndex >= S.COST_COUNT) ? (1f / S.MUTATIONMULT) : S.MUTATIONMULT;

                float changedValue = baseCost.arrayOfValues[bestAvgIndex % S.COST_COUNT];

                Console.WriteLine($"Multiplied: {changedValue}" +
                    $" by {mult}" +
                    $" => {mult * changedValue}" +
                    $"    (avg: {bestAvgScore.ToString(CultureInfo.InvariantCulture)})");
            }
            baseCost.MutatePredictabely(bestAvgIndex);
            baseCost.PrintValues(); // If you have a host-side print function
        }
    }
}
