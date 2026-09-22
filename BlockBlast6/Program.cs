//hodnoty
using BlockBlast;
using BlockBlast4;
using console_blockBlast_AI;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System.Collections.Specialized;
using System.Data;
using System.Numerics;

//optimalizovany gpu code

/*
Game game = new Game();
game.PlayWithUI();
*/

GpuGame game = new GpuGame();
Console.WriteLine(game.PlayAndReturnMoveCount());

// 1. Create the ILGPU Context
// This manages all the background stuff for compiling your kernels
using Context context = Context.Create(builder => builder.Default().Cuda().CPU());

// 2. Select a Device (GPU)
// This picks your best NVIDIA card, or falls back to CPU if no GPU is found
Device device = context.GetPreferredDevice(false);
using Accelerator accelerator = device.CreateAccelerator(context);

Console.WriteLine($"Running on: {accelerator.Name}");
Console.WriteLine();

// 3. Initialize your Evolution class with the GPU
Evolution evo = new Evolution(accelerator);

// 4. CALL IT!
evo.EvolveRepeatedly(howManyTimes: 10, sampleSize: 1, seedOffset: 0);

Console.WriteLine("Evolution complete! Press any key to exit.");
Console.ReadKey();
