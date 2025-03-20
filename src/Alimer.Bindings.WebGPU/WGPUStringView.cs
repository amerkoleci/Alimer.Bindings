using System.Diagnostics.CodeAnalysis;

namespace WebGPU;

public unsafe partial struct WGPUStringView : IEquatable<WGPUStringView>
{
    /// <summary>
    /// An <see cref="WGPUStringView"/> with empty string data.
    /// </summary>
    public static WGPUStringView Empty => new(null, 0);

    /// <summary>
    /// Initializes a new instance of <see cref="WGPUStringView"/> structure.
    /// </summary>
    /// <param name="data">The data pointer of utf-8 encoded string.</param>
    /// <param name="length">The length of the string.</param>
    public WGPUStringView(byte* data, nuint length)
    {
        this.data = data;
        this.length = length;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="WGPUStringView"/> structure.
    /// </summary>
    /// <param name="data">The data pointer of utf-8 encoded string.</param>
    /// <param name="length">The length of the string.</param>
    public WGPUStringView(byte* data, int length)
    {
        this.data = data;
        this.length = (nuint)length;
    }

    /// <inheritdoc/>
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is WGPUStringView other && Equals(other);

    /// <inheritdoc/>
    public readonly bool Equals(WGPUStringView other) => data == other.data && length == other.length;

    /// <summary>
    /// Compares two <see cref="WGPUStringView"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="WGPUStringView"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="WGPUStringView"/> on the right hand of the operand.</param>
    /// <returns>true if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
    public static bool operator ==(WGPUStringView left, WGPUStringView right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="WGPUStringView"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="WGPUStringView"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="WGPUStringView"/> on the right hand of the operand.</param>
    /// <returns>true if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.</returns>
    public static bool operator !=(WGPUStringView left, WGPUStringView right) => !left.Equals(right);

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        var hash = new HashCode();
        hash.AddBytes(new ReadOnlySpan<byte>(data, (int)length));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public override readonly string ToString() => Interop.GetString(data, (int)length)!;


    public static implicit operator string(WGPUStringView view) => view.ToString();
}

