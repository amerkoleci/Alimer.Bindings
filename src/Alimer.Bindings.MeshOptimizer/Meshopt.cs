// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MeshOptimizer;

public static unsafe partial class Meshopt
{
    #region Dll Loading
    private const DllImportSearchPath DefaultDllImportSearchPath = DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.UserDirectories | DllImportSearchPath.UseDllDirectoryForDependencies;

    public const string LibraryName = "meshoptimizer";
    private const string LibraryNameAlternate = "libmeshoptimizer";
    private const string LibraryNameWindows = "meshoptimizer.dll";
    private const string LibraryNameUnix = "libmeshoptimizer.so";
    private const string LibraryNameMacOS = "libmeshoptimizer.dylib";

    public static event DllImportResolver? ResolveLibrary;

    static Meshopt()
    {
        NativeLibrary.SetDllImportResolver(typeof(Meshopt).Assembly, OnDllImport);
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

    [LibraryImport(LibraryName, EntryPoint = "meshopt_setAllocator")]
    public static partial void SetAllocator(delegate* unmanaged<nuint, void*> allocate, delegate* unmanaged<void*, void> deallocate);

    public static nuint GenerateVertexRemap<TVertex>(Span<uint> destination, nuint indexCount, ReadOnlySpan<TVertex> vertices)
        where TVertex : unmanaged
    {
        fixed (uint* destinationPtr = destination)
        fixed (TVertex* verticesPtr = vertices)
            return GenerateVertexRemap(destinationPtr, null, indexCount, verticesPtr, (nuint)vertices.Length, (nuint)sizeof(TVertex));
    }

    public static nuint GenerateVertexRemap<TVertex>(Span<uint> destination, ReadOnlySpan<uint> indices, ReadOnlySpan<TVertex> vertices)
        where TVertex : unmanaged
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        fixed (TVertex* verticesPtr = vertices)
            return GenerateVertexRemap(destinationPtr, indicesPtr, (nuint)indices.Length, verticesPtr, (nuint)vertices.Length, (nuint)sizeof(TVertex));
    }

    public static nuint GenerateVertexRemapMulti<TVertex>(Span<uint> destination, nuint indexCount, nuint vertexCount, ReadOnlySpan<Stream> streams)
        where TVertex : unmanaged
    {
        fixed (uint* destinationPtr = destination)
        fixed (Stream* streamsPtr = streams)
            return GenerateVertexRemapMulti(destinationPtr, null, indexCount, vertexCount, streamsPtr, (nuint)streams.Length);
    }

    public static nuint GenerateVertexRemapMulti<TVertex>(Span<uint> destination, ReadOnlySpan<uint> indices, nuint vertexCount, ReadOnlySpan<Stream> streams)
        where TVertex : unmanaged
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        fixed (Stream* streamsPtr = streams)
            return GenerateVertexRemapMulti(destinationPtr, indicesPtr, (nuint)indices.Length, vertexCount, streamsPtr, (nuint)streams.Length);
    }

    public static void RemapVertexBuffer<TVertex>(Span<TVertex> destination, ReadOnlySpan<TVertex> vertices, ReadOnlySpan<uint> remap)
        where TVertex : unmanaged
    {
        fixed (TVertex* destinationPtr = destination)
        fixed (TVertex* verticesPtr = vertices)
        fixed (uint* remapPtr = remap)
            RemapVertexBuffer(destinationPtr, verticesPtr, (nuint)vertices.Length, (nuint)sizeof(TVertex), remapPtr);
    }

    public static void RemapIndexBuffer(Span<uint> destination, nuint indexCount, ReadOnlySpan<uint> remap)
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* remapPtr = remap)
            RemapIndexBuffer(destinationPtr, null, indexCount, remapPtr);
    }

    public static void RemapIndexBuffer(Span<uint> destination, ReadOnlySpan<uint> indices, ReadOnlySpan<uint> remap)
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        fixed (uint* remapPtr = remap)
            RemapIndexBuffer(destinationPtr, indicesPtr, (nuint)indices.Length, remapPtr);
    }

    public static void GenerateShadowIndexBuffer<TVertex>(Span<uint> destination, ReadOnlySpan<uint> indices, ReadOnlySpan<TVertex> vertices, nuint vertexSize)
        where TVertex : unmanaged
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        fixed (TVertex* verticesPtr = vertices)
            GenerateShadowIndexBuffer(destinationPtr, indicesPtr, (nuint)indices.Length, verticesPtr, (nuint)vertices.Length, vertexSize, (nuint)sizeof(TVertex));
    }

    public static void GenerateShadowIndexBufferMulti<TVertex>(Span<uint> destination, nuint indexCount, nuint vertexCount, ReadOnlySpan<Stream> streams)
        where TVertex : unmanaged
    {
        fixed (uint* destinationPtr = destination)
        fixed (Stream* streamsPtr = streams)
            GenerateShadowIndexBufferMulti(destinationPtr, null, indexCount, vertexCount, streamsPtr, (nuint)streams.Length);
    }

    public static void GenerateShadowIndexBufferMulti<TVertex>(Span<uint> destination, ReadOnlySpan<uint> indices, nuint vertexCount, ReadOnlySpan<Stream> streams)
        where TVertex : unmanaged
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        fixed (Stream* streamsPtr = streams)
            GenerateShadowIndexBufferMulti(destinationPtr, indicesPtr, (nuint)indices.Length, vertexCount, streamsPtr, (nuint)streams.Length);
    }

    public static void GenerateAdjacencyIndexBuffer(
        Span<uint> destination,
        ReadOnlySpan<uint> indices,
        ReadOnlySpan<float> vertexPositions,
        nuint vertexPositionsStride)
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        fixed (float* vertexPositionsPtr = vertexPositions)
            GenerateAdjacencyIndexBuffer(destinationPtr, indicesPtr, (nuint)indices.Length, vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride);
    }

    public static void GenerateTessellationIndexBuffer(
        Span<uint> destination,
        ReadOnlySpan<uint> indices,
        ReadOnlySpan<float> vertexPositions,
        nuint vertexPositionsStride)
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        fixed (float* vertexPositionsPtr = vertexPositions)
            GenerateTessellationIndexBuffer(destinationPtr, indicesPtr, (nuint)indices.Length, vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride);
    }

    public static void GenerateProvokingIndexBuffer(
        Span<uint> destination,
        Span<uint> reorder,
        ReadOnlySpan<uint> indices,
        nuint vertexCount)
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* reorderPtr = reorder)
        fixed (uint* indicesPtr = indices)
            GenerateProvokingIndexBuffer(destinationPtr, reorderPtr, indicesPtr, (nuint)indices.Length, vertexCount);
    }

    public static void OptimizeVertexCache(
        Span<uint> destination,
        ReadOnlySpan<uint> indices,
        nuint vertexCount)
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
            OptimizeVertexCache(destinationPtr, indicesPtr, (nuint)indices.Length, vertexCount);
    }

    public static void OptimizeVertexCacheStrip(
        Span<uint> destination,
        ReadOnlySpan<uint> indices,
        nuint vertexCount)
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
            OptimizeVertexCacheStrip(destinationPtr, indicesPtr, (nuint)indices.Length, vertexCount);
    }

    public static void OptimizeVertexCacheFifo(
        Span<uint> destination,
        ReadOnlySpan<uint> indices,
        nuint vertexCount,
        uint cacheSize)
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
            OptimizeVertexCacheFifo(destinationPtr, indicesPtr, (nuint)indices.Length, vertexCount, cacheSize);
    }

    public static void OptimizeOverdraw(
        Span<uint> destination,
        ReadOnlySpan<uint> indices,
        ReadOnlySpan<float> vertexPositions,
        nuint vertexPositionsStride,
        float threshold)
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        fixed (float* vertexPositionsPtr = vertexPositions)
            OptimizeOverdraw(destinationPtr, indicesPtr, (nuint)indices.Length, vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride, threshold);
    }

    public static nuint OptimizeVertexFetch<TVertex>(
        Span<uint> destination,
        Span<uint> indices,
        ReadOnlySpan<TVertex> vertices)
        where TVertex : unmanaged
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        fixed (TVertex* verticesPtr = vertices)
            return OptimizeVertexFetch(destinationPtr, indicesPtr, (nuint)indices.Length, verticesPtr, (nuint)vertices.Length, (nuint)sizeof(TVertex));
    }

    public static nuint OptimizeVertexFetchRemap(
        Span<uint> destination,
        ReadOnlySpan<uint> indices,
        nuint vertexCount)
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
            return OptimizeVertexFetchRemap(destinationPtr, indicesPtr, (nuint)indices.Length, vertexCount);
    }

    public static nuint EncodeIndexBuffer(
        Span<byte> buffer,
        ReadOnlySpan<uint> indices)
    {
        fixed (byte* bufferPtr = buffer)
        fixed (uint* indicesPtr = indices)
            return EncodeIndexBuffer(bufferPtr, (nuint)buffer.Length, indicesPtr, (nuint)indices.Length);
    }

    public static int DecodeIndexBuffer<TIndex>(
        Span<TIndex> destination,
        ReadOnlySpan<byte> buffer)
        where TIndex : unmanaged
    {
        fixed (TIndex* destinationPtr = destination)
        fixed (byte* bufferPtr = buffer)
            return DecodeIndexBuffer(destinationPtr, (nuint)buffer.Length, (nuint)sizeof(TIndex), bufferPtr, (nuint)buffer.Length);
    }

    public static nuint EncodeIndexSequence(
        Span<byte> buffer,
        Span<uint> indices
        )
    {
        fixed (byte* bufferPtr = buffer)
        fixed (uint* indicesPtr = indices)
            return EncodeIndexSequence(bufferPtr, (nuint)buffer.Length, indicesPtr, (nuint)indices.Length);
    }

    public static int DecodeIndexSequence<TIndex>(
        Span<TIndex> destination,
        ReadOnlySpan<byte> buffer)
        where TIndex : unmanaged
    {
        fixed (TIndex* destinationPtr = destination)
        fixed (byte* bufferPtr = buffer)
            return DecodeIndexSequence(destinationPtr, (nuint)buffer.Length, (nuint)sizeof(TIndex), bufferPtr, (nuint)buffer.Length);
    }

    public static nuint EncodeVertexBuffer<TVertex>(
        Span<byte> buffer,
        ReadOnlySpan<TVertex> vertices)
        where TVertex : unmanaged
    {
        fixed (byte* bufferPtr = buffer)
        fixed (TVertex* verticesPtr = vertices)
            return EncodeVertexBuffer(bufferPtr, (nuint)buffer.Length, verticesPtr, (nuint)vertices.Length, (nuint)sizeof(TVertex));
    }

    public static nuint EncodeVertexBufferLevel<TVertex>(
        Span<byte> buffer,
        ReadOnlySpan<TVertex> vertices, int level)
        where TVertex : unmanaged
    {
        fixed (byte* bufferPtr = buffer)
        fixed (TVertex* verticesPtr = vertices)
            return EncodeVertexBufferLevel(bufferPtr, (nuint)buffer.Length, verticesPtr, (nuint)vertices.Length, (nuint)sizeof(TVertex), level);
    }

    public static int DecodeVertexBuffer<TVertex>(
        Span<TVertex> destination,
        ReadOnlySpan<byte> buffer)
        where TVertex : unmanaged
    {
        fixed (TVertex* destinationPtr = destination)
        fixed (byte* bufferPtr = buffer)
            return DecodeVertexBuffer(destinationPtr, (nuint)buffer.Length, (nuint)sizeof(TVertex), bufferPtr, (nuint)buffer.Length);
    }

    public static void DecodeFilterOct<T>(Span<T> buffer)
        where T : unmanaged
    {
        fixed (T* bufferPtr = buffer)
            DecodeFilterOct(bufferPtr, (nuint)buffer.Length, (nuint)sizeof(T));
    }

    public static void DecodeFilterQuat<T>(Span<T> buffer)
        where T : unmanaged
    {
        fixed (T* bufferPtr = buffer)
            DecodeFilterQuat(bufferPtr, (nuint)buffer.Length, (nuint)sizeof(T));
    }

    public static void DecodeFilterExp<T>(Span<T> buffer)
        where T : unmanaged
    {
        fixed (T* bufferPtr = buffer)
            DecodeFilterExp(bufferPtr, (nuint)buffer.Length, (nuint)sizeof(T));
    }

    public static void EncodeFilterOct<T>(Span<T> destination, int bits, ReadOnlySpan<float> data)
        where T : unmanaged
    {
        fixed (T* destinationPtr = destination)
        fixed (float* dataPtr = data)
            EncodeFilterOct(destinationPtr, (nuint)destination.Length, (nuint)sizeof(T), bits, dataPtr);
    }

    public static void EncodeFilterQuat<T>(Span<T> destination, int bits, ReadOnlySpan<float> data)
        where T : unmanaged
    {
        fixed (T* destinationPtr = destination)
        fixed (float* dataPtr = data)
            EncodeFilterQuat(destinationPtr, (nuint)destination.Length, (nuint)sizeof(T), bits, dataPtr);
    }

    public static void EncodeFilterExp<T>(Span<T> destination, int bits, ReadOnlySpan<float> data, EncodeExpMode mode)
        where T : unmanaged
    {
        fixed (T* destinationPtr = destination)
        fixed (float* dataPtr = data)
            EncodeFilterExp(destinationPtr, (nuint)destination.Length, (nuint)sizeof(T), bits, dataPtr, mode);
    }

    public static nuint Simplify(
        Span<uint> destination,
        ReadOnlySpan<uint> indices,
        ReadOnlySpan<float> vertexPositions,
        nuint vertexPositionsStride,
        nuint targetIndexCount,
        float targetError,
        SimplificationOptions options,
        out float error)
    {
        Unsafe.SkipInit(out error);

        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        fixed (float* vertexPositionsPtr = vertexPositions)
        fixed (float* errorPtr = &error)
        {
            return Simplify(destinationPtr,
                indicesPtr, (nuint)indices.Length,
                vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride,
                targetIndexCount,
                targetError,
                options,
                errorPtr);
        }
    }

    public static nuint SimplifyWithAttributes(
        Span<uint> destination,
        ReadOnlySpan<uint> indices,
        ReadOnlySpan<float> vertexPositions, nuint vertexPositionsStride,
        ReadOnlySpan<float> vertexAttributes, nuint vertexAttributesStride,
        ReadOnlySpan<float> attributeWeights,
        nuint attributeCount,
        nuint targetIndexCount,
        float targetError,
        SimplificationOptions options,
        out float error)
    {
        Unsafe.SkipInit(out error);

        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        fixed (float* vertexPositionsPtr = vertexPositions)
        fixed (float* vertexAttributesPtr = vertexAttributes)
        fixed (float* attributeWeightsPtr = attributeWeights)
        fixed (float* errorPtr = &error)
        {
            return SimplifyWithAttributes(destinationPtr,
                indicesPtr, (nuint)indices.Length,
                vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride,
                vertexAttributesPtr, vertexAttributesStride,
                attributeWeightsPtr,
                attributeCount,
                null,
                targetIndexCount,
                targetError,
                options,
                errorPtr);
        }
    }

    public static nuint SimplifyWithAttributes(
        Span<uint> destination,
        ReadOnlySpan<uint> indices,
        ReadOnlySpan<float> vertexPositions, nuint vertexPositionsStride,
        ReadOnlySpan<float> vertexAttributes, nuint vertexAttributesStride,
        ReadOnlySpan<float> attributeWeights,
        nuint attributeCount,
        ReadOnlySpan<byte> vertexLock,
        nuint targetIndexCount,
        float targetError,
        SimplificationOptions options,
        out float error)
    {
        Unsafe.SkipInit(out error);

        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        fixed (float* vertexPositionsPtr = vertexPositions)
        fixed (float* vertexAttributesPtr = vertexAttributes)
        fixed (float* attributeWeightsPtr = attributeWeights)
        fixed (byte* vertexLockPtr = vertexLock)
        fixed (float* errorPtr = &error)
        {
            return SimplifyWithAttributes(destinationPtr,
                indicesPtr, (nuint)indices.Length,
                vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride,
                vertexAttributesPtr, vertexAttributesStride,
                attributeWeightsPtr,
                attributeCount,
                vertexLockPtr,
                targetIndexCount,
                targetError,
                options,
                errorPtr);
        }
    }

    public static nuint SimplifySloppy(
        Span<uint> destination,
        ReadOnlySpan<uint> indices,
        ReadOnlySpan<float> vertexPositions,
        nuint vertexPositionsStride,
        nuint targetIndexCount,
        float targetError,
        out float error)
    {
        Unsafe.SkipInit(out error);

        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        fixed (float* vertexPositionsPtr = vertexPositions)
        fixed (float* errorPtr = &error)
        {
            return SimplifySloppy(destinationPtr,
                indicesPtr, (nuint)indices.Length,
                vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride,
                targetIndexCount,
                targetError,
                errorPtr);
        }
    }

    public static nuint SimplifyPoints(
        Span<uint> destination,
        ReadOnlySpan<float> vertexPositions, nuint vertexPositionsStride,
        ReadOnlySpan<float> vertexColors, nuint vertexColorsStride,
        float colorWeight,
        nuint targetVertexCount)
    {
        fixed (uint* destinationPtr = destination)
        fixed (float* vertexPositionsPtr = vertexPositions)
        fixed (float* vertexColorsPtr = vertexColors)
        {
            return SimplifyPoints(destinationPtr,
                vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride,
                vertexColorsPtr, vertexColorsStride,
                colorWeight,
                targetVertexCount);
        }
    }

    public static float SimplifyScale(ReadOnlySpan<float> vertexPositions, nuint vertexPositionsStride)
    {
        fixed (float* vertexPositionsPtr = vertexPositions)
        {
            return SimplifyScale(vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride);
        }
    }

    public static nuint Stripify(
        Span<uint> destination,
        ReadOnlySpan<uint> indices,
        nuint vertexCount,
        uint restartIndex)
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        {
            return Stripify(destinationPtr,
                indicesPtr, (nuint)indices.Length,
                vertexCount,
                restartIndex);
        }
    }

    public static nuint Unstripify(Span<uint> destination, ReadOnlySpan<uint> indices, uint restartIndex)
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
            return Unstripify(destinationPtr, indicesPtr, (nuint)indices.Length, restartIndex);
    }

    public static VertexCacheStatistics AnalyzeVertexCache(
        ReadOnlySpan<uint> indices, nuint vertexCount,
        uint cacheSize, uint warpSize, uint primGroupSize)
    {
        fixed (uint* indicesPtr = indices)
            return AnalyzeVertexCache(indicesPtr, (nuint)indices.Length,
                vertexCount, cacheSize, warpSize, primGroupSize);
    }

    public static OverdrawStatistics AnalyzeOverdraw(ReadOnlySpan<uint> indices, ReadOnlySpan<float> vertexPositions, nuint vertexPositionsStride)
    {
        fixed (uint* indicesPtr = indices)
        fixed (float* vertexPositionsPtr = vertexPositions)
            return AnalyzeOverdraw(indicesPtr, (nuint)indices.Length,
                vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride);
    }

    public static VertexFetchStatistics AnalyzeVertexFetch(ReadOnlySpan<uint> indices, nuint vertexCount, nuint vertexSize)
    {
        fixed (uint* indicesPtr = indices)
            return AnalyzeVertexFetch(indicesPtr, (nuint)indices.Length, vertexCount, vertexSize);
    }

    public static nuint BuildMeshlets(
        Span<Meshlet> meshlets,
        Span<uint> meshletVertices,
        Span<byte> meshletTriangles,
        ReadOnlySpan<uint> indices,
        ReadOnlySpan<float> vertexPositions, nuint vertexPositionsStride,
        nuint maxVertices, nuint maxTriangles, float coneWeight)
    {
        fixed (Meshlet* meshletsPtr = meshlets)
        fixed (uint* meshletVerticesPtr = meshletVertices)
        fixed (byte* meshletTrianglesPtr = meshletTriangles)
        fixed (uint* indicesPtr = indices)
        fixed (float* vertexPositionsPtr = vertexPositions)
            return BuildMeshlets(meshletsPtr, meshletVerticesPtr, meshletTrianglesPtr,
                indicesPtr, (nuint)indices.Length,
                vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride,
                maxVertices, maxTriangles, coneWeight);
    }

    public static nuint BuildMeshletsScan(
        Span<Meshlet> meshlets,
        Span<uint> meshletVertices,
        Span<byte> meshletTriangles,
        ReadOnlySpan<uint> indices,
        nuint vertexCount,
        nuint maxVertices,
        nuint maxTriangles)
    {
        fixed (Meshlet* meshletsPtr = meshlets)
        fixed (uint* meshletVerticesPtr = meshletVertices)
        fixed (byte* meshletTrianglesPtr = meshletTriangles)
        fixed (uint* indicesPtr = indices)
            return BuildMeshletsScan(meshletsPtr, meshletVerticesPtr, meshletTrianglesPtr,
                indicesPtr, (nuint)indices.Length,
                vertexCount,
                maxVertices,
                maxTriangles);
    }

    public static void OptimizeMeshlet(
        Span<uint> meshletVertices,
        Span<byte> meshletTriangles,
        nuint triangleCount,
        nuint vertexCount)
    {
        fixed (uint* meshletVerticesPtr = meshletVertices)
        fixed (byte* meshletTrianglesPtr = meshletTriangles)
            OptimizeMeshlet(meshletVerticesPtr, meshletTrianglesPtr, triangleCount, vertexCount);
    }

    public static Bounds ComputeClusterBounds(
        ReadOnlySpan<uint> indices,
        ReadOnlySpan<float> vertexPositions, nuint vertexPositionsStride)
    {
        fixed (uint* indicesPtr = indices)
        fixed (float* vertexPositionsPtr = vertexPositions)
            return ComputeClusterBounds(indicesPtr, (nuint)indices.Length,
                vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride
                );
    }

    public static Bounds ComputeMeshletBounds(
        ReadOnlySpan<uint> meshletVertices,
        ReadOnlySpan<byte> meshletTriangles,
        ReadOnlySpan<float> vertexPositions, nuint vertexPositionsStride)
    {
        fixed (uint* meshletVerticesPtr = meshletVertices)
        fixed (byte* meshletTrianglesPtr = meshletTriangles)
        fixed (float* vertexPositionsPtr = vertexPositions)
            return ComputeMeshletBounds(meshletVerticesPtr,
                meshletTrianglesPtr, (nuint)meshletTriangles.Length,
                vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride
                );
    }

    public static void SpatialSortRemap(
        Span<uint> destination,
        ReadOnlySpan<float> vertexPositions, nuint vertexPositionsStride)
    {
        fixed (uint* destinationPtr = destination)
        fixed (float* vertexPositionsPtr = vertexPositions)
            SpatialSortRemap(destinationPtr,
                vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride
                );
    }

    public static void SpatialSortTriangles(
        Span<uint> destination,
        ReadOnlySpan<uint> indices,
        ReadOnlySpan<float> vertexPositions, nuint vertexPositionsStride)
    {
        fixed (uint* destinationPtr = destination)
        fixed (uint* indicesPtr = indices)
        fixed (float* vertexPositionsPtr = vertexPositions)
            SpatialSortTriangles(destinationPtr,
                indicesPtr, (nuint)indices.Length,
                vertexPositionsPtr, (nuint)vertexPositions.Length, vertexPositionsStride
                );
    }
}
