// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace WebGPU;

public partial struct WGPUColor : IEquatable<WGPUColor>
{
    /// <summary>
    /// Initializes a new instance of <see cref="WGPUOrigin3D"/> structure.
    /// </summary>
    /// <param name="red">The red component of the color.</param>
    /// <param name="green">The green component of the color.</param>
    /// <param name="blue">The blue component of the color.</param>
    /// <param name="alpha">The alpha component of the color.</param>
    public WGPUColor(double red, double green, double blue, double alpha = 1.0)
    {
        r = red;
        g = green;
        b = blue;
        a = alpha;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is WGPUColor other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(WGPUColor other) => r == other.r && g == other.g && b == other.b && a == other.a;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(r, g, b, a);

    /// <inheritdoc/>
    public override readonly string ToString() => $"{{Red={r},Green={g},Blue={b},Alpha={a}}}";

    /// <summary>
    /// Compares two <see cref="WGPUColor"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="WGPUColor"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="WGPUColor"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    public static bool operator ==(WGPUColor left, WGPUColor right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="WGPUColor"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="WGPUColor"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="WGPUOrigin3D"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    public static bool operator !=(WGPUColor left, WGPUColor right) => !left.Equals(right);
}
