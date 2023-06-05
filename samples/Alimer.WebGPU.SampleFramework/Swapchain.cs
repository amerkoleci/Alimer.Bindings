// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Alimer.Bindings.WebGPU;
using static Alimer.Bindings.WebGPU.WebGPU;

namespace Alimer.WebGPU.SampleFramework;

public sealed unsafe class Swapchain : IDisposable
{
    public readonly GraphicsDevice Device;
    public readonly Window Window;
    private readonly WGPUSurface _surface;

    public WGPUSwapChain Handle;
    public WGPUExtent3D Extent { get; }

    public Swapchain(GraphicsDevice device, WGPUSurface surface, Window window)
    {
        Device = device;
        _surface = surface;
        Window = window;
    }

    public void Dispose()
    {
        
    }
}
