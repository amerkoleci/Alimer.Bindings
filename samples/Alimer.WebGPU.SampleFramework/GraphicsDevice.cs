// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static WebGPU;

namespace Alimer.WebGPU.SampleFramework;

public unsafe sealed class GraphicsDevice : IDisposable
{
    public readonly WGPUInstance Instance;
    public readonly WGPUSurface Surface;

    public GraphicsDevice(bool enableValidation, Window window)
    {
        WGPUInstanceDescriptor instanceDescriptor = new()
        {
            nextInChain = null
        };
        Instance = wgpuCreateInstance(&instanceDescriptor);

        Surface = window.CreateSurface(Instance);
    }

    public void Dispose()
    {
        // Don't release anything until the GPU is completely idle.
        WaitIdle();
    }

    public void WaitIdle()
    {
    }

    #region Private Methods
    #endregion
}
