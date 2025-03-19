// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace WebGPU;

partial struct WGPULimits
{
    public static WGPULimits Default => new()
    {
        maxTextureDimension1D = 8192,
        maxTextureDimension2D = 8192,
        maxTextureDimension3D = 2048,
        maxTextureArrayLayers = 256,
        maxBindGroups = 4,
        maxBindGroupsPlusVertexBuffers = 24,
        maxBindingsPerBindGroup = 1000,
        maxDynamicUniformBuffersPerPipelineLayout = 8,
        maxDynamicStorageBuffersPerPipelineLayout = 4,
        maxSampledTexturesPerShaderStage = 16,
        maxSamplersPerShaderStage = 16,
        maxStorageBuffersPerShaderStage = 8,
        maxStorageTexturesPerShaderStage = 4,
        maxUniformBuffersPerShaderStage = 12,
        maxUniformBufferBindingSize = 16 * 1024,
        maxStorageBufferBindingSize = 128 * 1024 * 1024,
        minUniformBufferOffsetAlignment = 256,
        minStorageBufferOffsetAlignment = 256,
        maxVertexBuffers = 8,
        maxBufferSize = 256 * 1024 * 1024,
        maxVertexAttributes = 16,
        maxVertexBufferArrayStride = 2048,
        maxInterStageShaderVariables = 16,
        maxComputeWorkgroupStorageSize = 16384,
        maxComputeInvocationsPerWorkgroup = 256,
        maxComputeWorkgroupSizeX = 256,
        maxComputeWorkgroupSizeY = 256,
        maxComputeWorkgroupSizeZ = 64,
        maxComputeWorkgroupsPerDimension = 65535,
        maxColorAttachments = 8,
        maxColorAttachmentBytesPerSample = 32
    };
}
