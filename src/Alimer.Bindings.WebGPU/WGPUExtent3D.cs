// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Alimer.Bindings.WebGPU;

/// <summary>
/// Structure specifying a three-dimensional extent.
/// </summary>
public partial struct WGPUExtent3D : IEquatable<WGPUExtent3D>
{
    /// <summary>
    /// An <see cref="WGPUExtent3D"/> with all of its components set to zero.
    /// </summary>
    public static readonly WGPUExtent3D Zero = default;

    /// <summary>
    /// Initializes a new instance of <see cref="WGPUExtent3D"/> structure.
    /// </summary>
    /// <param name="width">The width component of the extent.</param>
    /// <param name="height">The height component of the extent.</param>
    /// <param name="depthOrArrayLayers">The depth or array layers component of the extent.</param>
    public WGPUExtent3D(uint width, uint height, uint depthOrArrayLayers = 1u)
    {
        this.width = width;
        this.height = height;
        this.depthOrArrayLayers = depthOrArrayLayers;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="WGPUExtent3D"/> structure.
    /// </summary>
    /// <param name="width">The width component of the extent.</param>
    /// <param name="height">The height component of the extent.</param>
    /// <param name="depth">The depth component of the extent.</param>
    public WGPUExtent3D(int width, int height, int depthOrArrayLayers = 1)
    {
        this.width = (uint)width;
        this.height = (uint)height;
        this.depthOrArrayLayers = (uint)depthOrArrayLayers;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is WGPUExtent3D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(WGPUExtent3D other) => width == other.width && height == other.height && depthOrArrayLayers == other.depthOrArrayLayers;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(width, height, depthOrArrayLayers);

    /// <inheritdoc/>
    public override readonly string ToString() => $"{{Width={width},Height={height},DepthOrArrayLayers={depthOrArrayLayers}}}";

    /// <summary>
    /// Compares two <see cref="WGPUExtent3D"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="WGPUExtent3D"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="WGPUExtent3D"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    public static bool operator ==(WGPUExtent3D left, WGPUExtent3D right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="WGPUExtent3D"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="WGPUExtent3D"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="WGPUExtent3D"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    public static bool operator !=(WGPUExtent3D left, WGPUExtent3D right) => !left.Equals(right);
}
