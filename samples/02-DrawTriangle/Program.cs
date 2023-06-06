// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Alimer.WebGPU.SampleFramework;

namespace DrawTriangle;

public static unsafe class Program
{
#if DEBUG
    private static bool EnableValidationLayers = true;
#else
	private static bool EnableValidationLayers = false;
#endif

    public static void Main()
    {
        using TestApp testApp = new TestApp();
        testApp.Run();
    }

    class TestApp : Application
    {
        private GraphicsDevice _graphicsDevice;
        public override string Name => "02-DrawTriangle";

        private WGPUPipelineLayout _pipelineLayout;
        private WGPURenderPipeline _pipeline;
        private WGPUBuffer _vertexBuffer;

        protected override void Initialize()
        {
            _graphicsDevice = new GraphicsDevice(EnableValidationLayers, MainWindow);

        }

        public override void Dispose()
        {
            _graphicsDevice.WaitIdle();

            //vkDestroyPipelineLayout(_graphicsDevice, _pipelineLayout);
            //vkDestroyPipeline(_graphicsDevice, _pipeline);
            //vkDestroyBuffer(_graphicsDevice, _vertexBuffer);
            //vkFreeMemory(_graphicsDevice, _vertexBufferMemory);

            _graphicsDevice.Dispose();

            base.Dispose();
        }

        protected override void OnTick()
        {
            //_graphicsDevice!.RenderFrame(OnDraw);
        }

        //private void OnDraw(VkCommandBuffer commandBuffer, VkFramebuffer framebuffer, VkExtent2D size)
        //{
        //    VkClearValue clearValue = new VkClearValue(0.0f, 0.0f, 0.2f, 1.0f);

        //    // Begin the render pass.
        //    VkRenderPassBeginInfo renderPassBeginInfo = new()
        //    {
        //        sType = VkStructureType.RenderPassBeginInfo,
        //        renderPass = _graphicsDevice.Swapchain.RenderPass,
        //        framebuffer = framebuffer,
        //        renderArea = new VkRect2D(size),
        //        clearValueCount = 1,
        //        pClearValues = &clearValue
        //    };

        //    vkCmdBeginRenderPass(commandBuffer, &renderPassBeginInfo, VkSubpassContents.Inline);

        //    // Update dynamic viewport state
        //    // Flip coordinate to map DirectX coordinate system.
        //    VkViewport viewport = new()
        //    {
        //        x = 0.0f,
        //        y = MainWindow.Extent.height,
        //        width = MainWindow.Extent.width,
        //        height = -MainWindow.Extent.height,
        //        minDepth = 0.0f,
        //        maxDepth = 1.0f
        //    };
        //    vkCmdSetViewport(commandBuffer, viewport);

        //    // Update dynamic scissor state
        //    VkRect2D scissor = new(MainWindow.Extent);
        //    vkCmdSetScissor(commandBuffer, scissor);

        //    // Bind the rendering pipeline
        //    vkCmdBindPipeline(commandBuffer, VkPipelineBindPoint.Graphics, _pipeline);

        //    // Bind triangle vertex buffer (contains position and colors)
        //    vkCmdBindVertexBuffer(commandBuffer, 0, _vertexBuffer);

        //    // Draw non indexed
        //    vkCmdDraw(commandBuffer, 3, 1, 0, 0);

        //    vkCmdEndRenderPass(commandBuffer);
        //}

        //private void CreateShaderModule(string name, out VkShaderModule shaderModule)
        //{
        //    byte[] vertexBytecode = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Assets", $"{name}.spv"));
        //    _graphicsDevice.CreateShaderModule(vertexBytecode, out shaderModule).CheckResult();
        //}
    }
}
