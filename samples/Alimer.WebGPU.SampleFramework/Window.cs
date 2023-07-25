// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using WebGPU;
using static WebGPU.WebGPU;
using static Alimer.WebGPU.SampleFramework.GLFW;
using System.Runtime.InteropServices;

namespace Alimer.WebGPU.SampleFramework;

[Flags]
public enum WindowFlags
{
    None = 0,
    Fullscreen = 1 << 0,
    FullscreenDesktop = 1 << 1,
    Hidden = 1 << 2,
    Borderless = 1 << 3,
    Resizable = 1 << 4,
    Minimized = 1 << 5,
    Maximized = 1 << 6,
}

public sealed unsafe class Window
{
    private readonly GLFWwindow _window;

    public unsafe Window(string title, int width, int height, WindowFlags flags = WindowFlags.None)
    {
        Title = title;

        bool fullscreen = false;
        GLFWmonitor monitor = GLFWmonitor.Null;
        if ((flags & WindowFlags.Fullscreen) != WindowFlags.None)
        {
            monitor = glfwGetPrimaryMonitor();
            fullscreen = true;
        }

        if ((flags & WindowFlags.FullscreenDesktop) != WindowFlags.None)
        {
            monitor = glfwGetPrimaryMonitor();
            //auto mode = glfwGetVideoMode(monitor);
            //
            //glfwWindowHint(GLFW_RED_BITS, mode->redBits);
            //glfwWindowHint(GLFW_GREEN_BITS, mode->greenBits);
            //glfwWindowHint(GLFW_BLUE_BITS, mode->blueBits);
            //glfwWindowHint(GLFW_REFRESH_RATE, mode->refreshRate);

            glfwWindowHint(WindowHintBool.Decorated, false);
            fullscreen = true;
        }

        if (!fullscreen)
        {
            if ((flags & WindowFlags.Borderless) != WindowFlags.None)
            {
                glfwWindowHint(WindowHintBool.Decorated, false);
            }
            else
            {
                glfwWindowHint(WindowHintBool.Decorated, true);
            }

            if ((flags & WindowFlags.Resizable) != WindowFlags.None)
            {
                glfwWindowHint(WindowHintBool.Resizable, true);
            }

            if ((flags & WindowFlags.Hidden) != WindowFlags.None)
            {
                glfwWindowHint(WindowHintBool.Visible, false);
            }

            if ((flags & WindowFlags.Minimized) != WindowFlags.None)
            {
                glfwWindowHint(WindowHintBool.Iconified, true);
            }

            if ((flags & WindowFlags.Maximized) != WindowFlags.None)
            {
                glfwWindowHint(WindowHintBool.Maximized, true);
            }
        }

        _window = glfwCreateWindow(width, height, title, monitor, GLFWwindow.Null);

        glfwGetWindowSize(_window, out width, out height);
        Extent = new WGPUExtent3D(width, height);
    }

    public string Title { get; }
    public WGPUExtent3D Extent { get; }

    public bool ShoudClose => glfwWindowShouldClose(_window);

    public WGPUSurface CreateSurface(WGPUInstance instance, bool useWayland = false)
    {
        if (OperatingSystem.IsWindows())
        {
            WGPUSurfaceDescriptorFromWindowsHWND chain = new()
            {
                hwnd = glfwGetWin32Window(_window),
                hinstance = GetModuleHandleW(null),
                chain = new WGPUChainedStruct()
                {
                    sType = WGPUSType.SurfaceDescriptorFromWindowsHWND
                }
            };
            WGPUSurfaceDescriptor descriptor = new()
            {
                nextInChain = (WGPUChainedStruct*)&chain
            };
            return wgpuInstanceCreateSurface(instance, &descriptor);
        }
        else if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
        {
            NSWindow ns_window = new(glfwGetCocoaWindow(_window));
            CAMetalLayer metal_layer = CAMetalLayer.New();
            ns_window.contentView.wantsLayer = true;
            ns_window.contentView.layer = metal_layer.Handle;

            WGPUSurfaceDescriptorFromMetalLayer chain = new()
            {
                layer = metal_layer.Handle,
                chain = new WGPUChainedStruct()
                {
                    sType = WGPUSType.SurfaceDescriptorFromMetalLayer
                }
            };
            WGPUSurfaceDescriptor descriptor = new()
            {
                nextInChain = (WGPUChainedStruct*)&chain
            };
            return wgpuInstanceCreateSurface(instance, &descriptor);
        }
        else if (OperatingSystem.IsLinux())
        {
            if (useWayland)
            {
                WGPUSurfaceDescriptorFromWaylandSurface chain = new()
                {
                    display = glfwGetWaylandDisplay(),
                    surface = glfwGetWaylandWindow(_window),
                    chain = new WGPUChainedStruct()
                    {
                        sType = WGPUSType.SurfaceDescriptorFromWaylandSurface
                    }
                };
                WGPUSurfaceDescriptor descriptor = new()
                {
                    nextInChain = (WGPUChainedStruct*)&chain
                };
                return wgpuInstanceCreateSurface(instance, &descriptor);
            }
            else
            {
                WGPUSurfaceDescriptorFromXlibWindow chain = new()
                {
                    display = glfwGetX11Display(),
                    window = glfwGetX11Window(_window),
                    chain = new WGPUChainedStruct()
                    {
                        sType = WGPUSType.SurfaceDescriptorFromXlibWindow
                    }
                };
                WGPUSurfaceDescriptor descriptor = new()
                {
                    nextInChain = (WGPUChainedStruct*)&chain
                };
                return wgpuInstanceCreateSurface(instance, &descriptor);
            }
        }

        return WGPUSurface.Null;
    }

    [DllImport("kernel32", ExactSpelling = true)]
    private static extern nint GetModuleHandleW(ushort* lpModuleName);
}
