// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
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
    public readonly WGPUTextureFormat SwapChainFormat;

    public GraphicsDevice(Window window)
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

            wgpuAdapterRequestDevice(
                Adapter,
                &deviceDesc,
                OnDeviceRequestEnded,
                IntPtr.Zero
            );
        }

        wgpuDeviceSetUncapturedErrorCallback(Device, HandleUncapturedErrorCallback);

        Queue = wgpuDeviceGetQueue(Device);

        // WGPUTextureFormat_BGRA8UnormSrgb on desktop, WGPUTextureFormat_BGRA8Unorm on mobile
        SwapChainFormat = wgpuSurfaceGetPreferredFormat(Surface, Adapter);
        Debug.Assert(SwapChainFormat != WGPUTextureFormat.Undefined);

        WGPUTextureFormat viewFormat = SwapChainFormat;

        WGPUSurfaceConfiguration surfaceConfiguration = new()
        {
            nextInChain = null,
            device = Device,
            format = SwapChainFormat,
            usage = WGPUTextureUsage.RenderAttachment,
            viewFormatCount = 1,
            viewFormats = &viewFormat,
            width = window.ClientSize.width,
            height = window.ClientSize.height,
            presentMode = WGPUPresentMode.Fifo
        };
        wgpuSurfaceConfigure(Surface, &surfaceConfiguration);
        Log.Info("SwapChain created");
    }

    private void OnAdapterRequestEnded(WGPURequestAdapterStatus status, WGPUAdapter candidateAdapter, sbyte* message, nint pUserData)
    {
        if (status == WGPURequestAdapterStatus.Success)
        {
            Adapter = candidateAdapter;
            WGPUAdapterProperties properties;
            wgpuAdapterGetProperties(candidateAdapter, &properties);

            WGPUSupportedLimits limits;
            wgpuAdapterGetLimits(candidateAdapter, &limits);

            AdapterProperties = properties;
            AdapterLimits = limits;
        }
        else
        {
            Log.Error("Could not get WebGPU adapter: " + Interop.GetString(message));
        }
    }

    private void OnDeviceRequestEnded(WGPURequestDeviceStatus status, WGPUDevice device, sbyte* message, nint pUserData)
    {
        if (status == WGPURequestDeviceStatus.Success)
        {
            Device = device;
        }
        else
        {
            Log.Error("Could not get WebGPU device: " + Interop.GetString(message));
        }
    }

    private static void HandleUncapturedErrorCallback(WGPUErrorType type, sbyte* pMessage, nint pUserData)
    {
        string message = Interop.GetString(pMessage);
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

        fixed (sbyte* pLabel = "Command Encoder".GetUtf8Span())
        {
            WGPUCommandEncoderDescriptor commandEncoderDesc = new()
            {
                nextInChain = null,
                label = pLabel
            };
            WGPUCommandEncoder encoder = wgpuDeviceCreateCommandEncoder(Device, &commandEncoderDesc);

            draw(encoder, surfaceTexture.texture);

            //wgpuTextureViewRelease(nextTexture);

            fixed (sbyte* pBufferLabel = "Command Buffer".GetUtf8Span())
            {
                WGPUCommandBufferDescriptor cmdBufferDescriptor = new()
                {
                    nextInChain = null,
                    label = pBufferLabel
                };

                WGPUCommandBuffer command = wgpuCommandEncoderFinish(encoder, &cmdBufferDescriptor);
                wgpuQueueSubmit(Queue, 1, &command);
            }

            //wgpuCommandEncoderRelease(encoder);
        }

        // We can tell the surface to present the next texture.
        wgpuSurfacePresent(Surface);
    }
}
