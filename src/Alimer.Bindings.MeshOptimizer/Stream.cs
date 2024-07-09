// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace MeshOptimizer;

partial struct Stream
{
    public unsafe Stream(void* data, nuint size, nuint stride)
    {
        this.data = data;
        this.size = size;
        this.stride = stride;
    }
}
