using console_blockBlast_AI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlockBlast
{
    public class Costs
    {
        public float[] arrayOfValues;

        static float MUTATIONMULT = 1.1f;//play around with
        
        /// <summary> Most evolved configuration </summary>
        public Costs()
        {
            this.arrayOfValues = new float[] {
                //50, 1, 3, 20, 27, 81

                //default values
                88.57806f, 0.3504938f, 1.3995218f, 12.418425f, 57.876907f, 107.811005f
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
                if(predictNumber >= arrayOfValues.Length / 2) mult = MUTATIONMULT;
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
