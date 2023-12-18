// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace WebGPU;

public partial struct WGPUVertexAttribute : IEquatable<WGPUVertexAttribute>
{
    /// <summary>
    /// Initializes a new instance of <see cref="WGPUVertexAttribute"/> structure.
    /// </summary>
    /// <param name="format">The format of the input.</param>
    /// <param name="offset">The byte offset of the start of the input.</param>
    /// <param name="shaderLocation">The location for this input. Must match the location in the shader..</param>
    public WGPUVertexAttribute(WGPUVertexFormat format, ulong offset, uint shaderLocation)
    {
        this.format = format;
        this.offset = offset;
        this.shaderLocation = shaderLocation;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is WGPUVertexAttribute other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(WGPUVertexAttribute other) => format == other.format && offset == other.offset && shaderLocation == other.shaderLocation;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(format, offset, shaderLocation);

    /// <inheritdoc/>
    public override readonly string ToString() => $"{{Format={format},Offset={offset},ShaderLocation={shaderLocation}}}";

    /// <summary>
    /// Compares two <see cref="WGPUVertexAttribute"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="WGPUVertexAttribute"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="WGPUVertexAttribute"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    public static bool operator ==(WGPUVertexAttribute left, WGPUVertexAttribute right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="WGPUVertexAttribute"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="WGPUVertexAttribute"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="WGPUVertexAttribute"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    public static bool operator !=(WGPUVertexAttribute left, WGPUVertexAttribute right) => !left.Equals(right);
}
