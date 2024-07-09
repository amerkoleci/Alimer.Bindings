// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using WebGPU;
using static WebGPU.WebGPU;
using System.Runtime.InteropServices;
using SDL;
using static SDL.SDL;

namespace Alimer.WebGPU.Samples;
[Flags]
public enum WindowFlags
{
    None = 0,
    Fullscreen = 1 << 0,
    Borderless = 1 << 1,
    Resizable = 1 << 2,
    Minimized = 1 << 3,
    Maximized = 1 << 4,
}

public sealed unsafe partial class Window
{
    private readonly SDL_Window _window;

    public unsafe Window(string title, int width, int height, WindowFlags flags = WindowFlags.None)
    {
        Title = title;

        SDL_WindowFlags sdl_flags = SDL_WindowFlags.HighPixelDensity | SDL_WindowFlags.Vulkan | SDL_WindowFlags.Hidden;
        if ((flags & WindowFlags.Fullscreen) != WindowFlags.None)
        {
            sdl_flags |= SDL_WindowFlags.Fullscreen;
        }
        else
        {
            if ((flags & WindowFlags.Borderless) != WindowFlags.None)
            {
                sdl_flags |= SDL_WindowFlags.Borderless;
            }

            if ((flags & WindowFlags.Resizable) != WindowFlags.None)
            {
                sdl_flags |= SDL_WindowFlags.Resizable;
            }

            if ((flags & WindowFlags.Minimized) != WindowFlags.None)
            {
                sdl_flags |= SDL_WindowFlags.Minimized;
            }

            if ((flags & WindowFlags.Maximized) != WindowFlags.None)
            {
                sdl_flags |= SDL_WindowFlags.Maximized;
            }
        }

        _window = SDL_CreateWindow(title, width, height, sdl_flags);
        if (_window.IsNull)
        {
            throw new Exception("SDL: failed to create window");
        }

        _ = SDL_SetWindowPosition(_window, SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED);
        Id = SDL_GetWindowID(_window);
    }

    public string Title { get; }

    public SDL_WindowID Id { get; }

    public WGPUExtent3D ClientSize
    {
        get
        {
            SDL_GetWindowSize(_window, out int width, out int height);
            return new(width, height);
        }
    }
    public WGPUExtent3D DrawableSize
    {
        get
        {
            SDL_GetWindowSizeInPixels(_window, out int width, out int height);
            return new(width, height);
        }
    }

    public void Show()
    {
        _ = SDL_ShowWindow(_window);
    }

    public WGPUSurface CreateSurface(WGPUInstance instance, bool useWayland = false)
    {
        if (OperatingSystem.IsWindows())
        {
            WGPUSurfaceDescriptorFromWindowsHWND chain = new()
            {
                hwnd = (void*)SDL_GetProperty(SDL_GetWindowProperties(_window), "SDL.window.win32.hwnd"),
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
            NSWindow ns_window = new(SDL_GetProperty(SDL_GetWindowProperties(_window), "SDL.window.cocoa.window"));
            CAMetalLayer metal_layer = CAMetalLayer.New();
            ns_window.contentView.wantsLayer = true;
            ns_window.contentView.layer = metal_layer.Handle;

            WGPUSurfaceDescriptorFromMetalLayer chain = new()
            {
                layer = metal_layer.Handle.ToPointer(),
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
                    display = (void*)SDL_GetProperty(SDL_GetWindowProperties(_window), "SDL.window.wayland.display"),
                    surface = (void*)SDL_GetProperty(SDL_GetWindowProperties(_window), "SDL.window.wayland.surface"),
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
                    display = (void*)SDL_GetProperty(SDL_GetWindowProperties(_window), "SDL.window.x11.display"),
                    window = (ulong)SDL_GetProperty(SDL_GetWindowProperties(_window), "SDL.window.x11.window"),
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

    [LibraryImport("kernel32")]
    private static partial void* GetModuleHandleW(ushort* lpModuleName);
}
