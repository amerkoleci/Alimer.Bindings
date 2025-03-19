namespace WebGPU;

public unsafe partial struct WGPUStringView
{
    public WGPUStringView(byte* data, nuint length)
    {
        this.data = data;
        this.length = length;
    }

    public WGPUStringView(byte* data, int length)
    {
        this.data = data;
        this.length = (nuint)length;
    }

    public override string ToString() => Interop.GetString(data, (int)length)!;

    public static implicit operator string(WGPUStringView view) => view.ToString();

}

