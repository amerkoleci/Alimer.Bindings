// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using SDL;
using static SDL.SDL;

namespace Alimer.WebGPU.Samples;

public abstract class Application : IDisposable
{
    private bool _closeRequested = false;

    protected unsafe Application()
    {
        if (SDL_Init(SDL_InitFlags.Video) != 0)
        {
            var error = SDL_GetErrorString();
            throw new Exception($"Failed to start SDL2: {error}");
        }

        SDL_LogSetOutputFunction(Log_SDL);

        // Create main window.
        MainWindow = new Window(Name, 1280, 720);
    }

    public abstract string Name { get; }

    public Window MainWindow { get; }

    public virtual void Dispose()
    {
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
            }

            if (!running)
                break;

            OnTick();
        }
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
