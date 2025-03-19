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
    public readonly Window Window;
    public readonly WGPUInstance Instance;
    public readonly WGPUSurface Surface;
    public WGPUAdapter Adapter;
    public WGPUAdapterInfo AdapterInfo;
    public WGPULimits AdapterLimits;
    public WGPUDevice Device;
    public readonly WGPUQueue Queue;

    public GraphicsDevice(Window window, bool vsync = true)
    {
        Window = window;
        VSync = vsync;

        wgpuSetLogCallback(LogCallback);

        WGPUInstanceExtras extras = new()
        {
#if DEBUG
            flags = WGPUInstanceFlag.Validation
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
            new WGPURequestAdapterCallbackInfo()
            {
                callback = &OnAdapterRequestEnded,
                userdata1 = &result,
                userdata2 = null
            }
        );
        Adapter = result;
        wgpuAdapterGetInfo(Adapter, out WGPUAdapterInfo adapterInfo);

        WGPULimits limits;
        wgpuAdapterGetLimits(Adapter, &limits);

        AdapterInfo = adapterInfo;
        AdapterLimits = limits;

        ReadOnlySpan<byte> deviceName = "My Device".GetUtf8Span();
        fixed (byte* pDeviceName = deviceName)
        {
            WGPUDeviceDescriptor deviceDesc = new()
            {
                nextInChain = null,
                label = new WGPUStringView(pDeviceName, deviceName.Length),
                requiredFeatureCount = 0,
                requiredLimits = null,
                uncapturedErrorCallbackInfo = new WGPUUncapturedErrorCallbackInfo()
                {
                    callback = &HandleUncapturedErrorCallback,
                    userdata1 = null,
                    userdata2 = null
                }
            };
            deviceDesc.defaultQueue.nextInChain = null;
            //deviceDesc.defaultQueue.label = "The default queue";

            WGPUDevice device = WGPUDevice.Null;
            wgpuAdapterRequestDevice(
                Adapter,
                &deviceDesc,
                new WGPURequestDeviceCallbackInfo()
                {
                    callback = &OnDeviceRequestEnded,
                    userdata1 = &device,
                    userdata2 = null
                }
            );
            Device = device;
        }

        Queue = wgpuDeviceGetQueue(Device);

        Resize(window.ClientSize.width, window.ClientSize.height);
    }

    public WGPUTextureFormat SwapChainFormat { get; private set; }
    public uint Width { get; private set; }
    public uint Height { get; private set; }
    public bool VSync { get; set; } = true;

    public void Resize(uint width, uint height)
    {
        Width = width;
        Height = height;

        // WGPUTextureFormat_BGRA8UnormSrgb on desktop, WGPUTextureFormat_BGRA8Unorm on mobile
        wgpuSurfaceGetCapabilities(Surface, Adapter, out WGPUSurfaceCapabilities capabilities);
        SwapChainFormat = capabilities.formats[0];
        Debug.Assert(SwapChainFormat != WGPUTextureFormat.Undefined);

        WGPUSurfaceConfiguration surfaceConfiguration = new()
        {
            device = Device,
            format = SwapChainFormat,
            usage = WGPUTextureUsage.RenderAttachment,
            alphaMode = capabilities.alphaModes[0],
            width = width,
            height = height,
            presentMode = VSync ? WGPUPresentMode.Fifo : WGPUPresentMode.Immediate,
        };
        wgpuSurfaceConfigure(Surface, &surfaceConfiguration);
        Log.Info("SwapChain created");
    }

    [UnmanagedCallersOnly]
    private static void OnAdapterRequestEnded(WGPURequestAdapterStatus status, WGPUAdapter candidateAdapter, WGPUStringView message, void* pUserData1, void* pUserData2)
    {
        if (status == WGPURequestAdapterStatus.Success)
        {
            *(WGPUAdapter*)pUserData1 = candidateAdapter;
        }
        else
        {
            Log.Error("Could not get WebGPU adapter: " + message.ToString());
        }
    }

    [UnmanagedCallersOnly]
    private static void OnDeviceRequestEnded(WGPURequestDeviceStatus status, WGPUDevice device, WGPUStringView message, void* pUserData1, void* pUserData2)
    {
        if (status == WGPURequestDeviceStatus.Success)
        {
            *(WGPUDevice*)pUserData1 = device;
        }
        else
        {
            Log.Error("Could not get WebGPU device: " + message.ToString());
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

    [UnmanagedCallersOnly]
    private static void HandleUncapturedErrorCallback(WGPUDevice* device, WGPUErrorType type, WGPUStringView message, void* userData1, void* userData2)
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
        Action<WGPUCommandEncoder, WGPUTexture, WGPUTextureView> draw,
        [CallerMemberName] string? frameName = null)
    {
        if (Surface.IsNull)
            return;

        WGPUSurfaceTexture surfaceTexture = default;
        wgpuSurfaceGetCurrentTexture(Surface, &surfaceTexture);

        switch (surfaceTexture.status)
        {
            case WGPUSurfaceGetCurrentTextureStatus.SuccessOptimal:
                // All good, could check for `surface_texture.optimal` here.
                break;
            case WGPUSurfaceGetCurrentTextureStatus.SuccessSuboptimal:
                // All good, could check for `surface_texture.suboptimal` here.
                Log.Warn("Surface texture is suboptimal");
                break;
            case WGPUSurfaceGetCurrentTextureStatus.Timeout:
            case WGPUSurfaceGetCurrentTextureStatus.Outdated:
            case WGPUSurfaceGetCurrentTextureStatus.Lost:
                // Skip this frame, and re-configure surface.
                if (surfaceTexture.texture.IsNotNull)
                {
                    wgpuTextureRelease(surfaceTexture.texture);
                }

                Resize(Window.ClientSize.width, Window.ClientSize.height);
                return;

            case WGPUSurfaceGetCurrentTextureStatus.OutOfMemory:
            case WGPUSurfaceGetCurrentTextureStatus.DeviceLost:
                // Fatal error
                Log.Error($"{nameof(wgpuSurfaceGetCurrentTexture)} status = {surfaceTexture.status}");
                throw new Exception();
        }
        Debug.Assert(surfaceTexture.texture.IsNotNull);

        WGPUTextureView textureView = wgpuTextureCreateView(surfaceTexture.texture, null);
        Debug.Assert(textureView.IsNotNull);

        WGPUCommandEncoder commandEncoder = wgpuDeviceCreateCommandEncoder(Device, "Main Command Encoder");
        wgpuCommandEncoderPushDebugGroup(commandEncoder, frameName);
        draw(commandEncoder, surfaceTexture.texture, textureView);
        wgpuCommandEncoderPopDebugGroup(commandEncoder);

        WGPUCommandBuffer commandBuffer = wgpuCommandEncoderFinish(commandEncoder, "Command Buffer");
        wgpuQueueSubmit(Queue, commandBuffer);
        // We can tell the surface to present the next texture.
        wgpuSurfacePresent(Surface);

        wgpuCommandBufferRelease(commandBuffer);
        wgpuCommandEncoderRelease(commandEncoder);
        wgpuTextureViewRelease(textureView);
        wgpuTextureRelease(surfaceTexture.texture);
    }
}
