/*
 * Tools > NuGet Package Manager > Package Manager Console
 * 
 * Defaul project: "jmeno tvyho projectu "GPU test""
 * 
 * Install-Package ILGPU
 * Install-Package ILGPU.Algorithms
*/

/*
 * Pokud ti to nejde spustit, kvuli unsafe keyword
 * najed na nej a v te zarovce dej allow unsafe, nebo neco takoveho
*/

using ILGPU;
using ILGPU.Runtime;
using System;

RunGpuMath();

// This is the function that runs on the GPU
static void MyMathKernel(
    Index1D index,              // The current thread index (0, 1, 2...)
    ArrayView<int> a,           // Input array A
    ArrayView<int> b,           // Input array B
    ArrayView<int> result)      // Output array
{
    // Each thread calculates one element
    result[index] = a[index] + b[index];
}

void RunGpuMath()
{
    // 1. Initialize ILGPU Context and Accelerator
    using var context = Context.CreateDefault();
    using var accelerator = context.GetPreferredDevice(preferCPU: false).CreateAccelerator(context);

    Console.WriteLine($"Running on: {accelerator.Name}");

    // 2. Prepare data on the Host (CPU)
    int size = 1024;
    int[] hostA = new int[size];
    int[] hostB = new int[size];
    for (int i = 0; i < size; i++) { hostA[i] = i; hostB[i] = i * 2; }

    // 3. Allocate and Copy data to the Device (GPU)
    using var deviceA = accelerator.Allocate1D(hostA);
    using var deviceB = accelerator.Allocate1D(hostB);
    using var deviceResult = accelerator.Allocate1D<int>(size);

    // 4. Load the Kernel
    // This JIT-compiles your C# function into GPU code
    var kernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<int>, ArrayView<int>, ArrayView<int>>(MyMathKernel);

    // 5. Launch the Kernel
    // We tell it to run 'size' times (one per array element)
    kernel((int)deviceResult.Length, deviceA.View, deviceB.View, deviceResult.View);

    // 6. Wait for GPU to finish and copy data back to CPU
    accelerator.Synchronize();
    int[] finalResult = deviceResult.GetAsArray1D();

    // Verify
    Console.WriteLine($"Result[10]: {finalResult[10]} (Expected: 30)");
    for(int i = 0; i < size; i++)
    {
        Console.WriteLine($"Result[{i}] = {finalResult[i]}");
    }
}