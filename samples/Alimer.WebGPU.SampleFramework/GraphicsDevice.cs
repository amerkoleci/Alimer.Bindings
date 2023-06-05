// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Alimer.Bindings.WebGPU;
using static Alimer.Bindings.WebGPU.WebGPU;
using static Alimer.WebGPU.SampleFramework.GLFW;

namespace Alimer.WebGPU.SampleFramework;

public unsafe sealed class GraphicsDevice : IDisposable
{
    public readonly WGPUInstance Instance;

    public GraphicsDevice(string applicationName, bool enableValidation, Window window)
    {
        
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

    private static readonly Lazy<bool> s_isSupported = new(CheckIsSupported);

    public static bool IsSupported() => s_isSupported.Value;

    private static bool CheckIsSupported()
    {
        try
        {
            if (!Initialize())
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }
}
