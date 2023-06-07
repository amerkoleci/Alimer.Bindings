// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;

public static unsafe partial class WebGPU
{
    private const string LibName = "wgpu_native";
    private static readonly ILibraryLoader _loader = GetPlatformLoader();

    private static IntPtr s_wgpuModule;

    static WebGPU()
    {
#if NET6_0_OR_GREATER && USE_PINVOKE
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), OnDllImport);
#else
        if (OperatingSystem.IsWindows())
        {
            s_wgpuModule = _loader.LoadNativeLibrary("wgpu_native.dll");

        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            s_wgpuModule = _loader.LoadNativeLibrary("libwgpu_native.dylib");
        }
        else
        {
            s_wgpuModule = _loader.LoadNativeLibrary("libwgpu_native.so");
        }

        if (s_wgpuModule == IntPtr.Zero)
        {
            throw new NotSupportedException("WebGPU is not supported");
        }

        GenLoadCommands();
#endif
    }

#if NET6_0_OR_GREATER && USE_PINVOKE
    public static event DllImportResolver? ResolveLibrary;

    private static nint OnDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (TryResolveLibrary(libraryName, assembly, searchPath, out nint nativeLibrary))
        {
            return nativeLibrary;
        }

        if (libraryName.Equals(LibName) && TryResolveWGPU(assembly, searchPath, out nativeLibrary))
        {
            return nativeLibrary;
        }

        return IntPtr.Zero;
    }

    private static bool TryResolveWGPU(Assembly assembly, DllImportSearchPath? searchPath, out IntPtr nativeLibrary)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (NativeLibrary.TryLoad("wgpu_native.dll", assembly, searchPath, out nativeLibrary))
            {
                return true;
            }
        }
        else
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (NativeLibrary.TryLoad("libwgpu_native.so", assembly, searchPath, out nativeLibrary))
                {
                    return true;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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

#endif
    public static void wgpuQueueWriteBuffer<T>(WGPUQueue queue, WGPUBuffer buffer, ref T data, ulong bufferOffset, nuint size)
        where T : unmanaged
    {
        fixed (void* dataPointer = &data)
        {
            wgpuQueueWriteBuffer_ptr(queue, buffer, bufferOffset, dataPointer, size);
        }
    }

    public static void wgpuQueueWriteBuffer<T>(WGPUQueue queue, WGPUBuffer buffer, ReadOnlySpan<T> data, ulong bufferOffset = 0)
        where T : unmanaged
    {
        fixed (void* dataPointer = data)
        {
            wgpuQueueWriteBuffer_ptr(queue, buffer, bufferOffset, dataPointer, (nuint)(data.Length * sizeof(T)));
        }
    }

    public static void wgpuQueueWriteBuffer<T>(WGPUQueue queue, WGPUBuffer buffer, T[] data, ulong bufferOffset = 0)
        where T : unmanaged
    {
        fixed (void* dataPointer = data)
        {
            wgpuQueueWriteBuffer_ptr(queue, buffer, bufferOffset, dataPointer, (nuint)(data.Length * sizeof(T)));
        }
    }

    public static void wgpuQueueWriteTexture<T>(WGPUQueue queue, WGPUImageCopyTexture* destination, ref T data, nuint dataSize, WGPUTextureDataLayout* dataLayout, WGPUExtent3D* writeSize)
        where T : unmanaged
    {
        fixed (void* dataPointer = &data)
        {
            wgpuQueueWriteTexture_ptr(queue, destination, dataPointer, dataSize, dataLayout, writeSize);
        }
    }

    public static void wgpuQueueWriteTexture<T>(WGPUQueue queue, WGPUImageCopyTexture* destination, ReadOnlySpan<T> data, nuint dataSize, WGPUTextureDataLayout* dataLayout, WGPUExtent3D* writeSize)
        where T : unmanaged
    {
        fixed (void* dataPointer = data)
        {
            wgpuQueueWriteTexture_ptr(queue, destination, dataPointer, dataSize, dataLayout, writeSize);
        }
    }

    public static void wgpuQueueWriteTexture<T>(WGPUQueue queue, WGPUImageCopyTexture* destination, T[] data, nuint dataSize, WGPUTextureDataLayout* dataLayout, WGPUExtent3D* writeSize)
        where T : unmanaged
    {
        fixed (void* dataPointer = data)
        {
            wgpuQueueWriteTexture_ptr(queue, destination, dataPointer, dataSize, dataLayout, writeSize);
        }
    }

    //public static void wgpuAdapterGetProperties(WGPUAdapter adapter, out WGPUAdapterProperties properties)
    //{
    //    properties = default;
    //    wgpuAdapterGetProperties_ptr(adapter, &properties);
    //}

    public static WGPUShaderModule wgpuDeviceCreateShaderModule(WGPUDevice device, ReadOnlySpan<sbyte> wgslShaderSource)
    {
        fixed (sbyte* pShaderSource = wgslShaderSource)
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
        return wgpuDeviceCreateBuffer_ptr(device, &descriptor);
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
        return wgpuDeviceCreateBuffer_ptr(device, &descriptor);
    }

    private static IntPtr LoadFunctionPointer(string name)
    {
        IntPtr addr = _loader.LoadFunctionPointer(s_wgpuModule, name);
        if (addr == IntPtr.Zero)
        {
            throw new InvalidOperationException($"No function was found with the name {name}.");
        }

        return addr;
    }

    private static TDelegate LoadCallbackThrow<TDelegate>(string name) where TDelegate : notnull
    {
        IntPtr addr = _loader.LoadFunctionPointer(s_wgpuModule, name);
        if (addr == IntPtr.Zero)
        {
            throw new InvalidOperationException($"No function was found with the name {name}.");
        }

        return Marshal.GetDelegateForFunctionPointer<TDelegate>(addr);
    }

    private static ILibraryLoader GetPlatformLoader()
    {
        return new SystemNativeLibraryLoader();
    }

    interface ILibraryLoader
    {
        IntPtr LoadNativeLibrary(string name);
        void FreeNativeLibrary(IntPtr handle);

        IntPtr LoadFunctionPointer(IntPtr handle, string name);
    }

    private class SystemNativeLibraryLoader : ILibraryLoader
    {
        public IntPtr LoadNativeLibrary(string name)
        {
            if (NativeLibrary.TryLoad(name, out IntPtr lib))
            {
                return lib;
            }

            return IntPtr.Zero;
        }

        public void FreeNativeLibrary(IntPtr handle)
        {
            NativeLibrary.Free(handle);
        }

        public nint LoadFunctionPointer(IntPtr handle, string name)
        {
            if (NativeLibrary.TryGetExport(handle, name, out IntPtr ptr))
            {
                return ptr;
            }

            return 0;
        }
    }
}
