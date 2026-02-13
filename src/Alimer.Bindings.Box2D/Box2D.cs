// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static unsafe partial class Box2D
{
    #region Dll Loading
    private const DllImportSearchPath DefaultDllImportSearchPath = DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.UserDirectories | DllImportSearchPath.UseDllDirectoryForDependencies;

    public const string LibraryName = "box2d";
    private const string LibraryNameAlternate = "libbox2d";
    private const string LibraryNameWindows = "box2d.dll";
    private const string LibraryNameUnix = "libbox2d.so";
    private const string LibraryNameMacOS = "libbox2d.dylib";

    public static event DllImportResolver? ResolveLibrary;

    static Box2D()
    {
        NativeLibrary.SetDllImportResolver(typeof(Box2D).Assembly, OnDllImport);
    }

    private static nint OnDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != LibraryName)
            return IntPtr.Zero;

        IntPtr nativeLibrary = IntPtr.Zero;
        DllImportResolver? resolver = ResolveLibrary;
        if (resolver != null)
        {
            nativeLibrary = resolver(libraryName, assembly, searchPath);
        }

        if (nativeLibrary != IntPtr.Zero)
        {
            return nativeLibrary;
        }

        if (OperatingSystem.IsWindows())
        {
            if (NativeLibrary.TryLoad(LibraryNameUnix, assembly, DefaultDllImportSearchPath, out nativeLibrary))
            {
                return nativeLibrary;
            }
        }
        else if (OperatingSystem.IsLinux())
        {
            if (NativeLibrary.TryLoad(LibraryNameUnix, assembly, DefaultDllImportSearchPath, out nativeLibrary))
            {
                return nativeLibrary;
            }
        }
        else if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
        {
            if (NativeLibrary.TryLoad(LibraryNameMacOS, assembly, DefaultDllImportSearchPath, out nativeLibrary))
            {
                return nativeLibrary;
            }
        }
        else
        {
            if (NativeLibrary.TryLoad(LibraryName, assembly, DefaultDllImportSearchPath, out nativeLibrary))
            {
                return nativeLibrary;
            }

            if (NativeLibrary.TryLoad(LibraryNameAlternate, assembly, DefaultDllImportSearchPath, out nativeLibrary))
            {
                return nativeLibrary;
            }
        }

        return IntPtr.Zero;
    }
    #endregion

    /// <summary>
    /// Get the angle in radians in the range [-pi, pi]
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    public static float b2Rot_GetAngle(in b2Rot q) => MathF.Atan2(q.s, q.c);
}
