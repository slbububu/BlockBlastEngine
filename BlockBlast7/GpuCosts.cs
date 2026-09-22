using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BlockBlast7
{
    public enum CostIndex : int //enums are turned into costs at compile time
    {
        empty0neibor = 0,
        empty1neibor = 1,
        empty2neiborNext = 2,
        empty2neiborOpposite = 3,
        empty3neibor = 4,
        empty4neibor = 5,
        full0neibor = 6,
        full1neibor = 7,
        full2neiborNext = 8,
        full2neiborOpposite = 9,
        full3neibor = 10,
        full4neibor = 11,
    }
    public unsafe struct GpuCosts
    {
        public fixed float arrayOfValues[S.COST_COUNT];
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
        public void MutatePredictabely(int predictNumber)
        {
            float mult = (predictNumber >= S.COST_COUNT) ? (1f / S.MUTATIONMULT) : S.MUTATIONMULT;
            int indexToMutate = predictNumber % S.COST_COUNT;

            arrayOfValues[indexToMutate] *= mult;
        }
        public void PrintValues()
        {
            Console.WriteLine(GetStringOfCosts());
        }
        public string GetStringOfCosts()
        {
            string ret = "";

            for (int i = 0; i < S.COST_COUNT; i++)
            {
                ret += arrayOfValues[i].ToString(CultureInfo.InvariantCulture);
                ret += "f";
                if(i < S.COST_COUNT - 1) ret += ", ";
            }
            return ret;
        }
    }
}
