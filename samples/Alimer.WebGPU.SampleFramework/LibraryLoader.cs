// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Reflection;
using System.Runtime.InteropServices;

namespace Alimer.WebGPU.SampleFramework;

public static class LibraryLoader
{
    private static string GetOSPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "win";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "linux";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "osx";

        throw new ArgumentException("Unsupported OS platform.");
    }

    private static string GetArchitecture()
    {
        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X86: return "x86";
            case Architecture.X64: return "x64";
            case Architecture.Arm: return "arm";
            case Architecture.Arm64: return "arm64";
        }

        throw new ArgumentException("Unsupported architecture.");
    }

    public static IntPtr LoadLibrary(string libraryName)
    {
        string libraryPath = GetNativeAssemblyPath(libraryName);

        IntPtr handle = LoadPlatformLibrary(libraryPath);
        if (handle == IntPtr.Zero)
            throw new DllNotFoundException($"Unable to load library '{libraryName}'.");

        return handle;

        static string GetNativeAssemblyPath(string libraryName)
        {
            string osPlatform = GetOSPlatform();
            string architecture = GetArchitecture();

            string assemblyLocation = Assembly.GetExecutingAssembly() != null ? Assembly.GetExecutingAssembly().Location : typeof(LibraryLoader).Assembly.Location;
            assemblyLocation = Path.GetDirectoryName(assemblyLocation);

            string[] paths = new[]
            {
                Path.Combine(assemblyLocation, libraryName),
                Path.Combine(assemblyLocation, "runtimes", osPlatform, "native", libraryName),
                Path.Combine(assemblyLocation, "runtimes", $"{osPlatform}-{architecture}", "native", libraryName),
                Path.Combine(assemblyLocation, "native", $"{osPlatform}-{architecture}", libraryName),
            };

            foreach (string path in paths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return libraryName;
        }
    }
    public static nint LoadSymbol(IntPtr library, string name)
    {
        return NativeLibrary.GetExport(library, name);
    }

    public static nint TryLoadSymbol(IntPtr library, string name)
    {
        NativeLibrary.TryGetExport(library, name, out nint address);
        return address;
    }

    public static TDelegate LoadFunction<TDelegate>(IntPtr library, string name)
    {
        IntPtr symbol = NativeLibrary.GetExport(library, name);

        return Marshal.GetDelegateForFunctionPointer<TDelegate>(symbol);
    }

    public static TDelegate? TryLoadFunction<TDelegate>(IntPtr library, string name)
    {
        if (!NativeLibrary.TryGetExport(library, name, out IntPtr address))
        {
            return default;
        }

        return Marshal.GetDelegateForFunctionPointer<TDelegate>(address);
    }

    private static IntPtr LoadPlatformLibrary(string libraryName)
    {
        return NativeLibrary.Load(libraryName);
    }
}
