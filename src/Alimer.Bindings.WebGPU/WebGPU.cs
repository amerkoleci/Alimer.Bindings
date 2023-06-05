// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;

namespace Alimer.Bindings.WebGPU;

public static unsafe partial class WebGPU
{
    private static readonly ILibraryLoader _loader = GetPlatformLoader();
    private delegate delegate* unmanaged<void> LoadFunction(nint context, string name);

    private static nint s_wgpuModule = 0;

    public static bool Initialize()
    {
        if (OperatingSystem.IsWindows())
        {
            s_wgpuModule = _loader.LoadNativeLibrary("wgpu_native.dll");
            if (s_wgpuModule == 0)
            {
                return false;
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            s_wgpuModule = _loader.LoadNativeLibrary("libwgpu_native.dylib");

            if (s_wgpuModule == 0)
            {
                return false;
            }
        }
        else
        {
            s_wgpuModule = _loader.LoadNativeLibrary("libwgpu_native.so");

            if (s_wgpuModule == 0)
            {
                return false;
            }
        }

        GenLoadCommands();
        return true;
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
        nint LoadNativeLibrary(string name);
        void FreeNativeLibrary(nint handle);

        IntPtr LoadFunctionPointer(nint handle, string name);
    }

    private class SystemNativeLibraryLoader : ILibraryLoader
    {
        public nint LoadNativeLibrary(string name)
        {
            if (NativeLibrary.TryLoad(name, out nint lib))
            {
                return lib;
            }

            return 0;
        }

        public void FreeNativeLibrary(nint handle)
        {
            NativeLibrary.Free(handle);
        }

        public nint LoadFunctionPointer(nint handle, string name)
        {
            if (NativeLibrary.TryGetExport(handle, name, out nint ptr))
            {
                return ptr;
            }

            return 0;
        }
    }
}
