// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WebGPU;
using static WebGPU.WebGPU;

namespace Alimer.WebGPU.Samples;

public unsafe sealed class GraphicsDevice : IDisposable
{
    public readonly WGPUInstance Instance;
    public readonly WGPUSurface Surface;
    public WGPUAdapter Adapter;
    public WGPUAdapterProperties AdapterProperties;
    public WGPUSupportedLimits AdapterLimits;
    public WGPUDevice Device;
    public readonly WGPUQueue Queue;

    public GraphicsDevice(Window window, bool vsync = true)
    {
        VSync = vsync;

        wgpuSetLogCallback(LogCallback);

        WGPUInstanceExtras extras = new()
        {
#if DEBUG
            flags = WGPUInstanceFlags.Validation
#endif
        };

        WGPUInstanceDescriptor instanceDescriptor = new()
        {
            nextInChain = (WGPUChainedStruct*)&extras
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
        WGPUAdapter result = WGPUAdapter.Null;
        wgpuInstanceRequestAdapter(
            Instance /* equivalent of navigator.gpu */,
            &options,
            &OnAdapterRequestEnded,
            new nint(&result)
        );
        Adapter = result;
        wgpuAdapterGetProperties(Adapter, out WGPUAdapterProperties properties);

        WGPUSupportedLimits limits;
        wgpuAdapterGetLimits(Adapter, &limits);

        AdapterProperties = properties;
        AdapterLimits = limits;

        fixed (sbyte* pDeviceName = "My Device".GetUtf8Span())
        {
            WGPUDeviceDescriptor deviceDesc = new()
            {
                nextInChain = null,
                label = pDeviceName,
                requiredFeatureCount = 0,
                requiredLimits = null
            };
            deviceDesc.defaultQueue.nextInChain = null;
            //deviceDesc.defaultQueue.label = "The default queue";

            WGPUDevice device = WGPUDevice.Null;
            wgpuAdapterRequestDevice(
                Adapter,
                &deviceDesc,
                &OnDeviceRequestEnded,
                new nint(&device)
            );
            Device = device;
        }

        wgpuDeviceSetUncapturedErrorCallback(Device, HandleUncapturedErrorCallback);

        Queue = wgpuDeviceGetQueue(Device);

        // WGPUTextureFormat_BGRA8UnormSrgb on desktop, WGPUTextureFormat_BGRA8Unorm on mobile
        SwapChainFormat = wgpuSurfaceGetPreferredFormat(Surface, Adapter);
        Debug.Assert(SwapChainFormat != WGPUTextureFormat.Undefined);

        Resize(window.ClientSize.width, window.ClientSize.height);
    }

    public WGPUTextureFormat SwapChainFormat { get; }
    public uint Width { get; private set; }
    public uint Height { get; private set; }
    public bool VSync { get; set; }

    public void Resize(uint width, uint height)
    {
        Width = width;
        Height = height;

        WGPUTextureFormat viewFormat = SwapChainFormat;
        WGPUSurfaceConfiguration surfaceConfiguration = new()
        {
            nextInChain = null,
            device = Device,
            format = SwapChainFormat,
            usage = WGPUTextureUsage.RenderAttachment,
            viewFormatCount = 1,
            viewFormats = &viewFormat,
            alphaMode = WGPUCompositeAlphaMode.Auto,
            width = width,
            height = height,
            presentMode = VSync ? WGPUPresentMode.Fifo : WGPUPresentMode.Immediate,
        };
        wgpuSurfaceConfigure(Surface, &surfaceConfiguration);
        Log.Info("SwapChain created");
    }

    [UnmanagedCallersOnly]
    private static void OnAdapterRequestEnded(WGPURequestAdapterStatus status, WGPUAdapter candidateAdapter, sbyte* message, nint pUserData)
    {
        if (status == WGPURequestAdapterStatus.Success)
        {
            *(WGPUAdapter*)pUserData = candidateAdapter;
        }
        else
        {
            Log.Error("Could not get WebGPU adapter: " + Interop.GetString(message));
        }
    }

    [UnmanagedCallersOnly]
    private static void OnDeviceRequestEnded(WGPURequestDeviceStatus status, WGPUDevice device, sbyte* message, nint pUserData)
    {
        if (status == WGPURequestDeviceStatus.Success)
        {
            *(WGPUDevice*)pUserData = device;
        }
        else
        {
            Log.Error("Could not get WebGPU device: " + Interop.GetString(message));
        }
    }

    private static void LogCallback(WGPULogLevel level, string message, nint userdata = 0)
    {
        switch (level)
        {
            case WGPULogLevel.Error:
                Log.Error(message);
                break;
            case WGPULogLevel.Warn:
                Log.Warn(message);
                break;
            case WGPULogLevel.Info:
            case WGPULogLevel.Debug:
            case WGPULogLevel.Trace:
                Log.Info(message);
                break;
        }
    }

    private static void HandleUncapturedErrorCallback(WGPUErrorType type, string message)
    {
        Log.Error($"Uncaptured device error: type: {type} ({message})");
    }

    public void Dispose()
    {
        wgpuDeviceRelease(Device);
        wgpuDeviceRelease(Device);
        wgpuSurfaceRelease(Surface);
        wgpuAdapterRelease(Adapter);
        wgpuInstanceRelease(Instance);
    }

    public void RenderFrame(
        Action<WGPUCommandEncoder, WGPUTexture> draw,
        [CallerMemberName] string? frameName = null)
    {
        if (Surface.IsNull)
            return;

        WGPUSurfaceTexture surfaceTexture = default;
        wgpuSurfaceGetCurrentTexture(Surface, &surfaceTexture);

        // Getting the texture may fail, in particular if the window has been resized
        // and thus the target surface changed.
        if (surfaceTexture.status == WGPUSurfaceGetCurrentTextureStatus.Timeout)
        {
            Log.Error("Cannot acquire next swap chain texture");
            return;
        }

        if (surfaceTexture.status == WGPUSurfaceGetCurrentTextureStatus.Outdated)
        {
            Log.Warn("Surface texture is outdated, reconfigure the surface!");
            return;
        }

        WGPUCommandEncoder encoder = wgpuDeviceCreateCommandEncoder(Device, "Main Command Encoder");
        wgpuCommandEncoderPushDebugGroup(encoder, frameName);
        draw(encoder, surfaceTexture.texture);
        wgpuCommandEncoderPopDebugGroup(encoder);

        WGPUCommandBuffer command = wgpuCommandEncoderFinish(encoder, "Command Buffer");
        wgpuQueueSubmit(Queue, command);

        wgpuCommandEncoderRelease(encoder);

        // We can tell the surface to present the next texture.
        wgpuSurfacePresent(Surface);
    }
}
