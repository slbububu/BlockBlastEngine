using console_blockBlast_AI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlockBlast
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
    public class Costs
    {
        public float[] arrayOfValues;
        
        /// <summary> Most evolved configuration </summary>
        public Costs()
        {
            //default values
            arrayOfValues = (float[])S.defaultArrayOfCosts.Clone();
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
                if(predictNumber >= arrayOfValues.Length / 2) mult = S.MUTATIONMULT;
                else mult = 1f / S.MUTATIONMULT;

            int indexToMutate = predictNumber % (arrayOfValues.Length / 2);
            arrayOfValues[indexToMutate] *= mult;
        }
        public void MutatePredictabelyPrint(int predictNumber)
        {
            float mult;
            if (predictNumber >= arrayOfValues.Length) mult = S.MUTATIONMULT;
            else mult = 1f / S.MUTATIONMULT;

            int indexToMutate = predictNumber % (arrayOfValues.Length);
            arrayOfValues[indexToMutate] *= mult;

            Console.Write("Multiplied " + arrayOfValues[indexToMutate] );
            Console.Write(" by " + mult.ToString(CultureInfo.InvariantCulture));
        }
        public void PrintValues()
        {
            foreach (float f in arrayOfValues)
            {
                Console.Write(f.ToString(CultureInfo.InvariantCulture) + ", ");
            }
            Console.WriteLine();
        }
    }
}
