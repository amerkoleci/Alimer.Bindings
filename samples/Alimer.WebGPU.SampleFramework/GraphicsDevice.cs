// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using static WebGPU;

namespace Alimer.WebGPU.SampleFramework;

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
    public readonly WGPUSwapChain SwapChain;

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
                requiredFeaturesCount = 0,
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
        wgpuDeviceSetDeviceLostCallback(Device, HandleDeviceLost);

        Queue = wgpuDeviceGetQueue(Device);

        // WGPUTextureFormat_BGRA8UnormSrgb on desktop, WGPUTextureFormat_BGRA8Unorm on mobile
        SwapChainFormat = wgpuSurfaceGetPreferredFormat(Surface, Adapter);
        Debug.Assert(SwapChainFormat != WGPUTextureFormat.Undefined);

        WGPUSwapChainDescriptor swapChainDesc = new()
        {
            nextInChain = null,
            usage = WGPUTextureUsage.RenderAttachment,
            format = SwapChainFormat,
            width = window.Extent.width,
            height = window.Extent.height,
            presentMode = WGPUPresentMode.Fifo
        };
        SwapChain = wgpuDeviceCreateSwapChain(Device, Surface, &swapChainDesc);
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

    private static void HandleDeviceLost(WGPUDeviceLostReason reason, sbyte* message, nint pUserData)
    {
        Log.Error($"Device Lost error: reason: {reason} ({Interop.GetString(message)})");
    }

    public void Dispose()
    {
        wgpuSwapChainRelease(SwapChain);
        wgpuDeviceDestroy(Device);
        wgpuDeviceRelease(Device);
        wgpuAdapterRelease(Adapter);
        wgpuInstanceRelease(Instance);
    }

    public void RenderFrame(
        Action<WGPUCommandEncoder, WGPUTextureView> draw,
        [CallerMemberName] string? frameName = null)
    {
        if (SwapChain.IsNull)
            return;

        WGPUTextureView nextTexture = wgpuSwapChainGetCurrentTextureView(SwapChain);
        // Getting the texture may fail, in particular if the window has been resized
        // and thus the target surface changed.
        if (nextTexture.IsNull)
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

            draw(encoder, nextTexture);

            wgpuTextureViewRelease(nextTexture);

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

        // We can tell the swap chain to present the next texture.
        wgpuSwapChainPresent(SwapChain);
    }
}
