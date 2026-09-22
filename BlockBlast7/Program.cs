//hodnoty
using BlockBlast7;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System.Collections.Specialized;
using System.Data;
using System.Numerics;

/*
GpuGame game = new GpuGame(1337);
Console.WriteLine(game.PlayAndReturnMoveCountWithDebugDraw());
Console.ReadLine();
*/

// 1. Create the ILGPU Context
using Context context = Context.Create(builder => builder.Default().Cuda().CPU());
// 2. Select a Device (GPU)
Device device = context.GetPreferredDevice(false);
using Accelerator accelerator = device.CreateAccelerator(context);
Console.WriteLine($"Running on: {accelerator.Name}\n");
// 3. Initialize Evolution class with the GPU
Evolution evo = new Evolution(accelerator);
// 4. CALL IT!

evo.EvolveRepeatedly(howManyTimes: -1, sampleSize: 1000, seedOffset: 0);

while (true) Console.ReadLine();
