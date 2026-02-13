// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using WebGPU;
using static WebGPU.WebGPU;

namespace Alimer.WebGPU.Samples;

public static unsafe class Program
{
    public static void Main()
    {
        using TestApp testApp = new();
        testApp.Run();
    }

    class TestApp : Application
    {
        public override string Name => "03-DrawIndexedQuad";

        private WGPUPipelineLayout _pipelineLayout;
        private WGPURenderPipeline _pipeline;
        private WGPUBuffer _vertexBuffer;
        private WGPUBuffer _indexBuffer;

        protected override void Initialize()
        {
            WGPUPipelineLayoutDescriptor layoutDesc = new()
            {
                nextInChain = null,
                bindGroupLayoutCount = 0,
                bindGroupLayouts = null
            };
            _pipelineLayout = wgpuDeviceCreatePipelineLayout(_graphicsDevice.Device, &layoutDesc);

            string shaderSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Assets", $"quad.wgsl"));
            WGPUShaderModule shaderModule = wgpuDeviceCreateShaderModule(_graphicsDevice.Device, shaderSource);

            // Vertex fetch
            WGPUVertexAttribute* vertexAttributes = stackalloc WGPUVertexAttribute[2] {
                new WGPUVertexAttribute(WGPUVertexFormat.Float32x3, 0, 0),
                new WGPUVertexAttribute(WGPUVertexFormat.Float32x4, 12, 1)
            };

            WGPUVertexBufferLayout vertexBufferLayout = new()
            {
                attributeCount = 2,
                attributes = vertexAttributes,
                arrayStride = (ulong)VertexPositionColor.SizeInBytes,
                stepMode = WGPUVertexStepMode.Vertex
            };

            ReadOnlySpan<byte> vertexEntryPoint = "vertexMain".GetUtf8Span();
            ReadOnlySpan<byte> fragmentEntryPoint = "fragmentMain".GetUtf8Span();
            fixed (byte* pVertexEntryPoint = vertexEntryPoint)
            fixed (byte* pFragmentEntryPoint = fragmentEntryPoint)
            {
                WGPURenderPipelineDescriptor pipelineDesc = new();
                pipelineDesc.layout = _pipelineLayout;

                pipelineDesc.vertex.bufferCount = 1;
                pipelineDesc.vertex.buffers = &vertexBufferLayout;

                // Vertex shader
                pipelineDesc.vertex.module = shaderModule;
                pipelineDesc.vertex.entryPoint = new WGPUStringView(pVertexEntryPoint, vertexEntryPoint.Length);
                pipelineDesc.vertex.constantCount = 0;
                pipelineDesc.vertex.constants = null;

                // Primitive assembly and rasterization
                // Each sequence of 3 vertices is considered as a triangle
                pipelineDesc.primitive.topology = WGPUPrimitiveTopology.TriangleList;
                // We'll see later how to specify the order in which vertices should be
                // connected. When not specified, vertices are considered sequentially.
                pipelineDesc.primitive.stripIndexFormat = WGPUIndexFormat.Undefined;
                // The face orientation is defined by assuming that when looking
                // from the front of the face, its corner vertices are enumerated
                // in the counter-clockwise (CCW) order.
                pipelineDesc.primitive.frontFace = WGPUFrontFace.CCW;
                // But the face orientation does not matter much because we do not
                // cull (i.e. "hide") the faces pointing away from us (which is often
                // used for optimization).
                pipelineDesc.primitive.cullMode = WGPUCullMode.None;

                // Fragment shader
                WGPUFragmentState fragmentState = new()
                {
                    nextInChain = null,
                    module = shaderModule,
                    entryPoint = new WGPUStringView(pFragmentEntryPoint, fragmentEntryPoint.Length),
                    constantCount = 0,
                    constants = null
                };
                pipelineDesc.fragment = &fragmentState;

                // Configure blend state
                WGPUBlendState blendState = new();
                // Usual alpha blending for the color:
                blendState.color.srcFactor = WGPUBlendFactor.SrcAlpha;
                blendState.color.dstFactor = WGPUBlendFactor.OneMinusSrcAlpha;
                blendState.color.operation = WGPUBlendOperation.Add;
                // We leave the target alpha untouched:
                blendState.alpha.srcFactor = WGPUBlendFactor.Zero;
                blendState.alpha.dstFactor = WGPUBlendFactor.One;
                blendState.alpha.operation = WGPUBlendOperation.Add;

                WGPUColorTargetState colorTarget = new()
                {
                    nextInChain = null,
                    format = _graphicsDevice.SwapChainFormat,
                    blend = &blendState,
                    writeMask = WGPUColorWriteMask.All // We could write to only some of the color channels.
                };

                // We have only one target because our render pass has only one output color
                // attachment.
                fragmentState.targetCount = 1;
                fragmentState.targets = &colorTarget;

                // Depth and stencil tests are not used here
                pipelineDesc.depthStencil = null;

                // Multi-sampling
                // Samples per pixel
                pipelineDesc.multisample.count = 1;
                // Default value for the mask, meaning "all bits on"
                pipelineDesc.multisample.mask = ~0u;
                // Default value as well (irrelevant for count = 1 anyways)
                pipelineDesc.multisample.alphaToCoverageEnabled = false;

                _pipeline = wgpuDeviceCreateRenderPipeline(_graphicsDevice.Device, &pipelineDesc);
            }

            wgpuShaderModuleRelease(shaderModule);

            Span<VertexPositionColor> vertexData = [
                new(new Vector3(-0.5f, 0.5f, 0.5f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),
                new(new Vector3(0.5f, 0.5f, 0.5f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f)),
                new(new Vector3(0.5f, -0.5f, 0.5f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
                new(new Vector3(-0.5f, -0.5f, 0.5f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f))
            ];
            _vertexBuffer = wgpuDeviceCreateBuffer(_graphicsDevice.Device, _graphicsDevice.Queue, vertexData,  WGPUBufferUsage.Vertex);

            // Index buffer
            Span<ushort> indices = [
                0,
                1,
                2,    // first triangle
                0,
                2,
                3,    // second triangle
            ];
            _indexBuffer = wgpuDeviceCreateBuffer(_graphicsDevice.Device, _graphicsDevice.Queue, indices, WGPUBufferUsage.Index | WGPUBufferUsage.CopyDst);
        }

        public override void Dispose()
        {
            wgpuPipelineLayoutRelease(_pipelineLayout);
            wgpuRenderPipelineRelease(_pipeline);
            wgpuBufferDestroy(_vertexBuffer);
            wgpuBufferRelease(_vertexBuffer);
            wgpuBufferDestroy(_indexBuffer);
            wgpuBufferRelease(_indexBuffer);

            base.Dispose();
        }

        protected override void OnTick()
        {
            _graphicsDevice.RenderFrame(OnDraw);
        }

        private void OnDraw(WGPUCommandEncoder encoder, WGPUTexture target, WGPUTextureView textureView)
        {
            WGPURenderPassColorAttachment renderPassColorAttachment = new()
            {
                // The attachment is tighed to the view returned by the swap chain, so that
                // the render pass draws directly on screen.
                view = textureView,
                // Not relevant here because we do not use multi-sampling
                resolveTarget = WGPUTextureView.Null,
                loadOp = WGPULoadOp.Clear,
                storeOp = WGPUStoreOp.Store,
                depthSlice = WGPU_DEPTH_SLICE_UNDEFINED,
                clearValue = new WGPUColor(0.0, 0.0, 0.0, 1.0)
            };

            // Describe a render pass, which targets the texture view
            WGPURenderPassDescriptor renderPassDesc = new()
            {
                nextInChain = null,
                colorAttachmentCount = 1,
                colorAttachments = &renderPassColorAttachment,
                // No depth buffer for now
                depthStencilAttachment = null,

                // We do not use timers for now neither
                timestampWrites = null
            };

            // Create a render pass. We end it immediately because we use its built-in
            // mechanism for clearing the screen when it begins (see descriptor).
            WGPURenderPassEncoder renderPass = wgpuCommandEncoderBeginRenderPass(encoder, &renderPassDesc);

            wgpuRenderPassEncoderSetPipeline(renderPass, _pipeline);
            wgpuRenderPassEncoderSetVertexBuffer(renderPass, 0, _vertexBuffer);
            wgpuRenderPassEncoderSetIndexBuffer(renderPass, _indexBuffer, WGPUIndexFormat.Uint16);

            wgpuRenderPassEncoderDrawIndexed(renderPass, 6);

            wgpuRenderPassEncoderEnd(renderPass);
            wgpuRenderPassEncoderRelease(renderPass);
        }
    }
}
