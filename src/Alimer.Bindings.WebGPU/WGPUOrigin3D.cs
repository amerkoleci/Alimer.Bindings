// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace WebGPU;

public partial struct WGPUOrigin3D : IEquatable<WGPUOrigin3D>
{
    /// <summary>
    /// An <see cref="WGPUOrigin3D"/> with all of its components set to zero.
    /// </summary>
    public static WGPUOrigin3D Zero => new();

    /// <summary>
    /// Initializes a new instance of <see cref="WGPUOrigin3D"/> structure.
    /// </summary>
    /// <param name="x">The x component of the origin.</param>
    /// <param name="y">The y component of the origin.</param>
    /// <param name="z">The z component of the origin.</param>
    public WGPUOrigin3D(uint x, uint y, uint z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="WGPUOrigin3D"/> structure.
    /// </summary>
    /// <param name="x">The x component of the origin.</param>
    /// <param name="y">The y component of the origin.</param>
    /// <param name="z">The z component of the origin.</param>
    public WGPUOrigin3D(int x, int y, int z)
    {
        this.x = (uint)x;
        this.y = (uint)y;
        this.z = (uint)z;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is WGPUOrigin3D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(WGPUOrigin3D other) => x == other.x && y == other.y && z == other.z;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(x, y, x);

    /// <inheritdoc/>
    public override readonly string ToString() => $"{{X={x},Y={y},Z={z}}}";

    /// <summary>
    /// Compares two <see cref="WGPUOrigin3D"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="WGPUOrigin3D"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="WGPUOrigin3D"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    public static bool operator ==(WGPUOrigin3D left, WGPUOrigin3D right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="WGPUOrigin3D"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="WGPUOrigin3D"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="WGPUOrigin3D"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    public static bool operator !=(WGPUOrigin3D left, WGPUOrigin3D right) => !left.Equals(right);
}
