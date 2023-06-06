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

        WGPURequestAdapterOptions options = new()
        {
            nextInChain = null,
            compatibleSurface = Surface,
            powerPreference = WGPUPowerPreference.HighPerformance
        };

        // Call to the WebGPU request adapter procedure
        wgpuInstanceRequestAdapter(
            Instance /* equivalent of navigator.gpu */,
            &options,
            OnAdapterRequestEnded,
            IntPtr.Zero
        );
    }

    private void OnAdapterRequestEnded(WGPURequestAdapterStatus status, WGPUAdapter candidateAdapter, sbyte* message, nint pUserData)
    {
        if (status == WGPURequestAdapterStatus.Success)
        {
            WGPUAdapterProperties properties;
            wgpuAdapterGetProperties(candidateAdapter, &properties);
        }
        else
        {
            //Log.Error("Could not get WebGPU adapter: " << message);
            //std::cout << "Could not get WebGPU adapter: " << message << std::endl;
        }
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
