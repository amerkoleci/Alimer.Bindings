// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

//#define USE_FUNCTION_POINTERS
using System.Reflection;
using System.Runtime.InteropServices;

namespace WebGPU;

public delegate void WGPULogCallback(WGPULogLevel level, string message, nint userdata = 0);

public delegate void WGPUErrorCallback(WGPUErrorType type, string message);

public static unsafe partial class WebGPU
{
    private const string LibraryName = "wgpu_native";

#if USE_FUNCTION_POINTERS
    private static IntPtr s_wgpuModule;
#endif

    static WebGPU()
    {
#if !USE_FUNCTION_POINTERS
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), OnDllImport);
#else
        if (OperatingSystem.IsWindows())
        {
            s_wgpuModule = NativeLibrary.Load("wgpu_native.dll");

        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            s_wgpuModule = NativeLibrary.Load("libwgpu_native.dylib");
        }
        else
        {
            if (!NativeLibrary.TryLoad("libwgpu_native.so", out s_wgpuModule))
            {
                s_wgpuModule = NativeLibrary.Load("runtimes/linux-x64/native/libwgpu_native.so");
            }
        }

        if (s_wgpuModule == IntPtr.Zero)
        {
            throw new NotSupportedException("WebGPU is not supported");
        }

        GenLoadCommands();
#endif
    }

#if !USE_FUNCTION_POINTERS
    public static event DllImportResolver? ResolveLibrary;

    private static nint OnDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (TryResolveLibrary(libraryName, assembly, searchPath, out nint nativeLibrary))
        {
            return nativeLibrary;
        }

        if (libraryName.Equals(LibraryName) && TryResolveWGPU(assembly, searchPath, out nativeLibrary))
        {
            return nativeLibrary;
        }

        return IntPtr.Zero;
    }

    private static bool TryResolveWGPU(Assembly assembly, DllImportSearchPath? searchPath, out IntPtr nativeLibrary)
    {
        if (OperatingSystem.IsWindows())
        {
            if (NativeLibrary.TryLoad("wgpu_native.dll", assembly, searchPath, out nativeLibrary))
            {
                return true;
            }
        }
        else
        {
            if (OperatingSystem.IsLinux())
            {
                if (NativeLibrary.TryLoad("libwgpu_native.so", assembly, searchPath, out nativeLibrary))
                {
                    return true;
                }
            }
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
            {
                if (NativeLibrary.TryLoad("libwgpu_native.dylib", assembly, searchPath, out nativeLibrary))
                {
                    return true;
                }
            }

            if (NativeLibrary.TryLoad("libwgpu_native", assembly, searchPath, out nativeLibrary))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath, out nint nativeLibrary)
    {
        var resolveLibrary = ResolveLibrary;

        if (resolveLibrary != null)
        {
            var resolvers = resolveLibrary.GetInvocationList();

            foreach (DllImportResolver resolver in resolvers)
            {
                nativeLibrary = resolver(libraryName, assembly, searchPath);

                if (nativeLibrary != 0)
                {
                    return true;
                }
            }
        }

        nativeLibrary = 0;
        return false;
    }
#else
    private static IntPtr LoadFunctionPointer(string name)
    {
        return NativeLibrary.GetExport(s_wgpuModule, name);
    }
#endif

    private static WGPULogCallback? s_logCallback;

    public static void wgpuSetLogCallback(WGPULogCallback callback, nint userdata = 0)
    {
        s_logCallback = callback;
        wgpuSetLogCallback(callback != null ? &NativeLogCallback : null, userdata.ToPointer());
    }

    public static ReadOnlySpan<WGPUFeatureName> wgpuAdapterEnumerateFeatures(WGPUAdapter adapter)
    {
        nuint count = wgpuAdapterEnumerateFeatures(adapter, null);

        ReadOnlySpan<WGPUFeatureName> features = new WGPUFeatureName[(int)count];
        fixed (WGPUFeatureName* pFeatures = features)
        {
            wgpuAdapterEnumerateFeatures(adapter, pFeatures);
        }

        return features;
    }

    public static void wgpuQueueSubmit(WGPUQueue queue, WGPUCommandBuffer commandBuffer)
    {
        wgpuQueueSubmit(queue, 1u, &commandBuffer);
    }

    public static void wgpuQueueSubmit(WGPUQueue queue, ReadOnlySpan<WGPUCommandBuffer> commandBuffers)
    {
        fixed (WGPUCommandBuffer* pCommandBuffers = commandBuffers)
        {
            wgpuQueueSubmit(queue, (nuint)commandBuffers.Length, pCommandBuffers);
        }
    }

    public static void wgpuQueueSubmit(WGPUQueue queue, WGPUCommandBuffer[] commandBuffers)
    {
        fixed (WGPUCommandBuffer* pCommandBuffers = commandBuffers)
        {
            wgpuQueueSubmit(queue, (nuint)commandBuffers.LongLength, pCommandBuffers);
        }
    }

    public static void wgpuQueueWriteBuffer<T>(WGPUQueue queue, WGPUBuffer buffer, ref T data, ulong bufferOffset, nuint size)
        where T : unmanaged
    {
        fixed (void* dataPointer = &data)
        {
            wgpuQueueWriteBuffer(queue, buffer, bufferOffset, dataPointer, size);
        }
    }

    public static void wgpuQueueWriteBuffer<T>(WGPUQueue queue, WGPUBuffer buffer, ReadOnlySpan<T> data, ulong bufferOffset = 0)
        where T : unmanaged
    {
        fixed (void* dataPointer = data)
        {
            wgpuQueueWriteBuffer(queue, buffer, bufferOffset, dataPointer, (nuint)(data.Length * sizeof(T)));
        }
    }

    public static void wgpuQueueWriteBuffer<T>(WGPUQueue queue, WGPUBuffer buffer, T[] data, ulong bufferOffset = 0)
        where T : unmanaged
    {
        fixed (void* dataPointer = data)
        {
            wgpuQueueWriteBuffer(queue, buffer, bufferOffset, dataPointer, (nuint)(data.Length * sizeof(T)));
        }
    }

    public static void wgpuQueueWriteTexture<T>(WGPUQueue queue, WGPUImageCopyTexture* destination, ref T data, nuint dataSize, WGPUTextureDataLayout* dataLayout, WGPUExtent3D* writeSize)
        where T : unmanaged
    {
        fixed (void* dataPointer = &data)
        {
            wgpuQueueWriteTexture(queue, destination, dataPointer, dataSize, dataLayout, writeSize);
        }
    }

    public static void wgpuQueueWriteTexture<T>(WGPUQueue queue, WGPUImageCopyTexture* destination, ReadOnlySpan<T> data, nuint dataSize, WGPUTextureDataLayout* dataLayout, WGPUExtent3D* writeSize)
        where T : unmanaged
    {
        fixed (void* dataPointer = data)
        {
            wgpuQueueWriteTexture(queue, destination, dataPointer, dataSize, dataLayout, writeSize);
        }
    }

    public static void wgpuQueueWriteTexture<T>(WGPUQueue queue, WGPUImageCopyTexture* destination, T[] data, nuint dataSize, WGPUTextureDataLayout* dataLayout, WGPUExtent3D* writeSize)
        where T : unmanaged
    {
        fixed (void* dataPointer = data)
        {
            wgpuQueueWriteTexture(queue, destination, dataPointer, dataSize, dataLayout, writeSize);
        }
    }

    public static WGPUCommandEncoder wgpuDeviceCreateCommandEncoder(WGPUDevice device, string? label = default, WGPUChainedStruct* nextInChain = default)
    {
        fixed (byte* pLabel = label.GetUtf8Span())
        {
            WGPUCommandEncoderDescriptor descriptor = new()
            {
                nextInChain = nextInChain,
                label = pLabel
            };

            return wgpuDeviceCreateCommandEncoder(device, &descriptor);
        }
    }

    public static WGPUCommandBuffer wgpuCommandEncoderFinish(WGPUCommandEncoder commandEncoder, string? label = default, WGPUChainedStruct* nextInChain = default)
    {
        fixed (byte* pLabel = label.GetUtf8Span())
        {
            WGPUCommandBufferDescriptor descriptor = new()
            {
                nextInChain = nextInChain,
                label = pLabel
            };

            return wgpuCommandEncoderFinish(commandEncoder, &descriptor);
        }
    }

    public static WGPUShaderModule wgpuDeviceCreateShaderModule(WGPUDevice device, ReadOnlySpan<byte> wgslShaderSource)
    {
        fixed (byte* pShaderSource = wgslShaderSource)
        {
            // Use the extension mechanism to load a WGSL shader source code
            WGPUShaderModuleWGSLDescriptor shaderCodeDesc = new();
            shaderCodeDesc.chain.next = null;
            shaderCodeDesc.chain.sType = WGPUSType.ShaderModuleWGSLDescriptor;
            shaderCodeDesc.code = pShaderSource;

            WGPUShaderModuleDescriptor shaderDesc = new()
            {
                nextInChain = &shaderCodeDesc.chain,
                hintCount = 0,
                hints = null
            };

            return wgpuDeviceCreateShaderModule(device, &shaderDesc);
        }
    }

    public static WGPUShaderModule wgpuDeviceCreateShaderModule(WGPUDevice device, string wgslShaderSource)
    {
        return wgpuDeviceCreateShaderModule(device, wgslShaderSource.GetUtf8Span());
    }

    public static WGPUBuffer wgpuDeviceCreateBuffer(WGPUDevice device, WGPUBufferUsage usage, ulong size, bool mappedAtCreation = false)
    {
        WGPUBufferDescriptor descriptor = new()
        {
            nextInChain = null,
            usage = usage,
            size = size,
            mappedAtCreation = mappedAtCreation
        };
        return wgpuDeviceCreateBuffer(device, &descriptor);
    }

    public static WGPUBuffer wgpuDeviceCreateBuffer(WGPUDevice device, WGPUBufferUsage usage, int size, bool mappedAtCreation = false)
    {
        WGPUBufferDescriptor descriptor = new()
        {
            nextInChain = null,
            usage = usage,
            size = (ulong)size,
            mappedAtCreation = mappedAtCreation
        };
        return wgpuDeviceCreateBuffer(device, &descriptor);
    }

    public static WGPUBuffer wgpuDeviceCreateBuffer<T>(WGPUDevice device, WGPUQueue queue, Span<T> data, WGPUBufferUsage usage, bool mappedAtCreation = false)
        where T : unmanaged
    {
        WGPUBufferDescriptor descriptor = new()
        {
            nextInChain = null,
            usage = usage | WGPUBufferUsage.CopyDst,
            size = (ulong)(sizeof(T) * data.Length),
            mappedAtCreation = mappedAtCreation
        };

        WGPUBuffer buffer = wgpuDeviceCreateBuffer(device, &descriptor);

        fixed (void* dataPointer = data)
        {
            wgpuQueueWriteBuffer(queue, buffer, 0, dataPointer, (nuint)descriptor.size);
        }

        return buffer;
    }

    #region Native Callbacks
    [UnmanagedCallersOnly]
    private static void NativeLogCallback(WGPULogLevel level, byte* pMessage, void* userData)
    {
        if (s_logCallback != null)
        {
            string message = Interop.GetString(pMessage)!;
            s_logCallback(level, message, (nint)userData);
        }
    }
    #endregion
}
