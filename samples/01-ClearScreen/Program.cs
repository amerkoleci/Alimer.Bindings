// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

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
        
        private float _green = 0.0f;
        public override string Name => "01-ClearScreen";

        protected override void Initialize()
        {
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnTick()
        {
            _graphicsDevice.RenderFrame(OnDraw);
        }

        private void OnDraw(WGPUCommandEncoder encoder, WGPUTexture target)
        {
            float g = _green + 0.001f;
            if (g > 1.0f)
                g = 0.0f;
            _green = g;

            WGPUTextureView targetView = wgpuTextureCreateView(target, null);

            WGPURenderPassColorAttachment renderPassColorAttachment = new();
            // The attachment is tighed to the view returned by the swap chain, so that
            // the render pass draws directly on screen.
            renderPassColorAttachment.view = targetView;
            // Not relevant here because we do not use multi-sampling
            renderPassColorAttachment.resolveTarget = WGPUTextureView.Null;
            renderPassColorAttachment.loadOp = WGPULoadOp.Clear;
            renderPassColorAttachment.storeOp = WGPUStoreOp.Store;
            renderPassColorAttachment.clearValue = new WGPUColor(1.0f, _green, 0.0f, 1.0f);

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

            wgpuRenderPassEncoderEnd(renderPass);

            wgpuTextureViewReference(targetView);
        }
    }
}
