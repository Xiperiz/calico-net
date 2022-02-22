/*
    CalicoNET - Cross platform CHIP8 emulator.
    Copyright (C) 2022 Xiperiz

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using static SDL2.SDL;

namespace CalicoNET;

public class Emulator
{
    private const int AudioSampleNumber = 0;
    private readonly CommandLineArgs _parsedArgs;

    private readonly Interpreter _interpreter;

    private IntPtr _window;
    private IntPtr _renderer;
    private IntPtr _frameTexture;

    private readonly IntPtr _audioSpec = IntPtr.Zero;

    public Emulator(CommandLineArgs parsedArgs, string romPath)
    {
        _parsedArgs = parsedArgs;

        _interpreter = new Interpreter(File.ReadAllBytes(romPath));

        InitSDL();
    }

    ~Emulator()
    {
        SDL_DestroyTexture(_frameTexture);
        SDL_DestroyRenderer(_renderer);
        SDL_DestroyWindow(_window);
        SDL_Quit();
    }

    public void Run()
    {
        var windowOpen = true;
        while (windowOpen)
        {
            var start = SDL_GetPerformanceCounter();

            while (SDL_PollEvent(out var sdlEvent) != 0)
                switch (sdlEvent.type)
                {
                    case SDL_EventType.SDL_QUIT:
                        windowOpen = false;
                        break;

                    case SDL_EventType.SDL_KEYUP:
                        _interpreter.HandleKeyStatus(sdlEvent.key.keysym.sym, false);
                        break;

                    case SDL_EventType.SDL_KEYDOWN:
                        _interpreter.HandleKeyStatus(sdlEvent.key.keysym.sym, true);
                        break;
                }

            /* Ex. 600 hz clock speed (600hz / 60fps = 10 instructions per second) */
            for (var i = 0; i < _parsedArgs.ClockSpeed / 60; i++) _interpreter.ExecuteNextInstruction();

            if (_parsedArgs.SoundEnabled && _interpreter.ShouldPlaySound())
            {
                SDL_PauseAudio(0);
                SDL_Delay(10);
                SDL_PauseAudio(1);
            }

            _interpreter.TickTimers();

            if (_interpreter.DrawFlag)
            {
                if (SDL_UpdateTexture(_frameTexture, IntPtr.Zero, _interpreter.FrameBuffer.GetSDLPixelArray(),
                        64 * sizeof(int)) != 0 ||
                    SDL_RenderClear(_renderer) != 0 ||
                    SDL_RenderCopy(_renderer, _frameTexture, IntPtr.Zero, IntPtr.Zero) != 0
                   )
                    throw new SDLException();

                SDL_RenderPresent(_renderer);

                _interpreter.DrawFlag = false;
            }

            var end = SDL_GetPerformanceCounter();
            var elapsed = (end - start) / (SDL_GetPerformanceFrequency() * 1000f);

            SDL_Delay((uint) MathF.Floor(16.666f - elapsed));
        }
    }

    private static unsafe void AudioCallback(IntPtr userData, IntPtr rawBuffer, int bytes)
    {
        var buffer = (short*) rawBuffer.ToPointer();
        var length = bytes / 2;
        var sampleNumber = userData.ToInt32();

        for (var i = 0; i < length; i++, sampleNumber++)
        {
            var time = sampleNumber / 44100.0f;
            buffer[i] = (short) (28000 * MathF.Sin(2.0f * MathF.PI * 441.0f * time));
        }
    }

    private void InitSDL()
    {
        if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_AUDIO) != 0) throw new SDLException();

        var audioSpecRequest = new SDL_AudioSpec
        {
            freq = 44100,
            format = AUDIO_S16SYS,
            channels = 1,
            samples = 2048,
            callback = AudioCallback,
            userdata = new IntPtr(AudioSampleNumber)
        };

        if (SDL_OpenAudio(ref audioSpecRequest, _audioSpec) != 0) throw new SDLException();

        _window = SDL_CreateWindow("CalicoNET", SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED,
            _parsedArgs.WindowSizeX, _parsedArgs.WindowSizeY, SDL_WindowFlags.SDL_WINDOW_SHOWN);
        if (_window == IntPtr.Zero) throw new SDLException();

        _renderer = SDL_CreateRenderer(_window, 0, SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
        if (_renderer == IntPtr.Zero) throw new SDLException();

        _frameTexture = SDL_CreateTexture(_renderer, SDL_PIXELFORMAT_ARGB8888,
            (int) SDL_TextureAccess.SDL_TEXTUREACCESS_STATIC, 64, 32);
        if (_frameTexture == IntPtr.Zero) throw new SDLException();
    }
}