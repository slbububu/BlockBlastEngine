using console_blockBlast_AI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BlockBlast4
{
    public unsafe struct GpuCosts
    {
        // You define the size in ONE place (your S class)
        //public fixed float arrayOfValues[] = S.defaultArrayOfCosts;
        public fixed float arrayOfValues[S.COST_COUNT];// = {88.57806f, 0.3504938f, 1.3995218f, 12.418425f, 57.876907f, 107.811005f};
        public GpuCosts()
        {
            //default values
            for (int i = 0; i < S.COST_COUNT; i++)
                arrayOfValues[i] = S.defaultArrayOfCosts[i];
        }
        public GpuCosts(GpuCosts gc) // Constructors need at least one parameter
        {
            for (int i = 0; i <S.COST_COUNT; i++)
                arrayOfValues[i] = gc.arrayOfValues[i]; 
        }
        public float GetValue(int index)
        {
            // On GPU, we have to be careful with bounds
            if (index < 0 || index >= S.COST_COUNT) return 0;
            return arrayOfValues[index];
        }
        public void SetValue(int index, float val)
        {
            if (index >= 0 && index < S.COST_COUNT)
                arrayOfValues[index] = val;
        }
        public void MutatePredictabely(int predictNumber)
        {
            // Now this logic works regardless of whether COST_COUNT is 6 or 600
            float mult = (predictNumber >= S.COST_COUNT) ? (1f / S.MUTATIONMULT) : S.MUTATIONMULT;
            int indexToMutate = predictNumber % S.COST_COUNT;

            arrayOfValues[indexToMutate] *= mult;
        }
        public void MutatePredictabelyPrint(int predictNumber)
        {
            float mult;
            if (predictNumber >= S.COST_COUNT) mult = S.MUTATIONMULT;
            else mult = 1f / S.MUTATIONMULT;

            int indexToMutate = predictNumber % (S.COST_COUNT);
            arrayOfValues[indexToMutate] *= mult;

            Console.Write("Multiplied " + arrayOfValues[indexToMutate]);
            Console.Write(" by " + mult.ToString(CultureInfo.InvariantCulture));
        }
        public void PrintValues()
        {
            for (int i = 0; i < S.COST_COUNT; i++)
                Console.Write(arrayOfValues[i].ToString(CultureInfo.InvariantCulture) + ", ");

            Console.WriteLine();
        }
    }
}
