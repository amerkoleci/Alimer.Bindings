// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using SDL;
using static SDL.SDL;

namespace Alimer.WebGPU.Samples;

public abstract class Application : IDisposable
{
    private bool _closeRequested = false;
    protected readonly GraphicsDevice _graphicsDevice;

    protected unsafe Application()
    {
        if (SDL_Init(SDL_InitFlags.Video) != 0)
        {
            var error = SDL_GetErrorString();
            throw new Exception($"Failed to start SDL2: {error}");
        }

        SDL_SetLogOutputFunction(Log_SDL);

        // Create main window.
        MainWindow = new Window(Name, 1280, 720, WindowFlags.Resizable);

        // Create graphics device
        _graphicsDevice = new GraphicsDevice(MainWindow);
    }

    public abstract string Name { get; }

    public Window MainWindow { get; }

    public virtual void Dispose()
    {
        _graphicsDevice.Dispose();
        GC.SuppressFinalize(this);
    }

    protected virtual void Initialize()
    {

    }

    protected virtual void OnTick()
    {
    }

    public unsafe void Run()
    {
        Initialize();
        MainWindow.Show();

        bool running = true;

        while (running && !_closeRequested)
        {
            SDL_Event evt;
            while (SDL_PollEvent(&evt))
            {
                if (evt.type == SDL_EventType.Quit)
                {
                    running = false;
                    break;
                }

                if (evt.type == SDL_EventType.WindowCloseRequested && evt.window.windowID == MainWindow.Id)
                {
                    running = false;
                    break;
                }
                else if (evt.type >= SDL_EventType.WindowFirst && evt.type <= SDL_EventType.WindowLast)
                {
                    HandleWindowEvent(evt);
                }
            }

            if (!running)
                break;

            OnTick();
        }
    }

    private void HandleWindowEvent(in SDL_Event evt)
    {
        switch ((SDL_EventType)evt.window.type)
        {
            case SDL_EventType.WindowResized:
                //_minimized = false;
                HandleResize(evt);
                break;
        }
    }

    private void HandleResize(in SDL_Event evt)
    {
        //if (MainWindow.ClientSize.width != evt.window.data1 ||
        //    MainWindow.ClientSize.height != evt.window.data2)
        {
            _graphicsDevice.Resize((uint)evt.window.data1, (uint)evt.window.data2);
            OnSizeChanged(evt.window.data1, evt.window.data2);
        }
    }

    protected virtual void OnSizeChanged(int width, int height)
    {
    }

    protected virtual void OnDraw(int width, int height)
    {

    }

    //[UnmanagedCallersOnly]
    private static void Log_SDL(SDL_LogCategory category, SDL_LogPriority priority, string description)
    {
        if (priority >= SDL_LogPriority.Error)
        {
            Log.Error($"[{priority}] SDL: {description}");
        }
        else
        {
            Log.Info($"[{priority}] SDL: {description}");
        }
    }
}
