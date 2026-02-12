// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static unsafe partial class Cgltf
{
    #region Dll Loading
    private const DllImportSearchPath DefaultDllImportSearchPath = DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.UserDirectories | DllImportSearchPath.UseDllDirectoryForDependencies;

    public const string LibraryName = "cgltf";
    private const string LibraryNameAlternate = "libcgltf";
    private const string LibraryNameWindows = "cgltf.dll";
    private const string LibraryNameUnix = "libcgltf.so";
    private const string LibraryNameMacOS = "libcgltf.dylib";

    public static event DllImportResolver? ResolveLibrary;

    static Cgltf()
    {
        NativeLibrary.SetDllImportResolver(typeof(Cgltf).Assembly, OnDllImport);
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

    [LibraryImport(LibraryName, EntryPoint = "cgltf_parse")]
    public static partial cgltf_result cgltf_parse(cgltf_options* options, void* data, nuint size, out cgltf_data* out_data);

    public static cgltf_result cgltf_parse(cgltf_options* options, Span<byte> data, out cgltf_data* out_data)
    {
        fixed (byte* dataPtr = data)
        {
            return cgltf_parse(options, dataPtr, (nuint)data.Length, out out_data);
        }
    }

    public static cgltf_result cgltf_parse(cgltf_options* options, Span<byte> data, cgltf_data** out_data)
    {
        fixed (byte* dataPtr = data)
        {
            return cgltf_parse(options, dataPtr, (nuint)data.Length, out_data);
        }
    }

    [LibraryImport(LibraryName, EntryPoint = "cgltf_parse_file")]
    public static partial cgltf_result cgltf_parse_file(cgltf_options* options, ReadOnlySpan<byte> path, cgltf_data** out_data);

    [LibraryImport(LibraryName, EntryPoint = "cgltf_parse_file", StringMarshalling = StringMarshalling.Utf8)]
    public static partial cgltf_result cgltf_parse_file(cgltf_options* options, string path, cgltf_data** out_data);

    [LibraryImport(LibraryName, EntryPoint = "cgltf_parse_file")]
    public static partial cgltf_result cgltf_parse_file(cgltf_options* options, ReadOnlySpan<byte> path, cgltf_data* out_data);

    [LibraryImport(LibraryName, EntryPoint = "cgltf_parse_file", StringMarshalling = StringMarshalling.Utf8)]
    public static partial cgltf_result cgltf_parse_file(cgltf_options* options, string path, out cgltf_data* out_data);

    [LibraryImport(LibraryName, EntryPoint = "cgltf_load_buffers")]
    public static partial cgltf_result cgltf_load_buffers(cgltf_options* options, cgltf_data* data, ReadOnlySpan<byte> gltf_path);

    [LibraryImport(LibraryName, EntryPoint = "cgltf_load_buffers", StringMarshalling = StringMarshalling.Utf8)]
    public static partial cgltf_result cgltf_load_buffers(cgltf_options* options, cgltf_data* data, string gltf_path);

    [LibraryImport(LibraryName, EntryPoint = "cgltf_load_buffer_base64")]
    public static partial cgltf_result cgltf_load_buffer_base64(cgltf_options* options, nuint size, ReadOnlySpan<byte> base64, void** out_data);

    [LibraryImport(LibraryName, EntryPoint = "cgltf_load_buffer_base64", StringMarshalling = StringMarshalling.Utf8)]
    public static partial cgltf_result cgltf_load_buffer_base64(cgltf_options* options, nuint size, string base64, void** out_data);

    [LibraryImport(LibraryName, EntryPoint = "cgltf_decode_string")]
    public static partial nuint cgltf_decode_string(ReadOnlySpan<byte> @string);

    [LibraryImport(LibraryName, EntryPoint = "cgltf_decode_uri")]
    public static partial nuint cgltf_decode_uri(ReadOnlySpan<byte> uri);


    [LibraryImport(LibraryName, EntryPoint = "cgltf_decode_string", StringMarshalling = StringMarshalling.Utf8)]
    public static partial nuint cgltf_decode_string(string @string);

    [LibraryImport(LibraryName, EntryPoint = "cgltf_decode_uri", StringMarshalling = StringMarshalling.Utf8)]
    public static partial nuint cgltf_decode_uri(string uri);


    [LibraryImport(LibraryName, EntryPoint = "cgltf_write_file")]
    public static partial cgltf_result cgltf_write_file(cgltf_options* options, ReadOnlySpan<byte> path, cgltf_data* data);

    [LibraryImport(LibraryName, EntryPoint = "cgltf_write_file", StringMarshalling = StringMarshalling.Utf8)]
    public static partial cgltf_result cgltf_write_file(cgltf_options* options, string path, cgltf_data* data);

    public static nuint cgltf_write(cgltf_options* options, cgltf_data* data)
    {
        return cgltf_write(options, (byte*)null, 0u, data);
    }

    public static nuint cgltf_write(cgltf_options* options, Span<byte> buffer, cgltf_data* data)
    {
        fixed (byte* bufferPtr = buffer)
        {
            return cgltf_write(options, bufferPtr, (nuint)buffer.Length, data);
        }
    }

    public static nuint cgltf_write(cgltf_options* options, Span<byte> buffer, nuint size, cgltf_data* data)
    {
        fixed (byte* bufferPtr = buffer)
        {
            return cgltf_write(options, bufferPtr, size, data);
        }
    }
}
