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

public class Interpreter
{
    private const int MemorySize = 4096;
    private const int GeneralRegisterCount = 16;
    private const int KeyCount = 16;
    private const int MemoryBinaryStartAddress = 0x200;

    private static readonly byte[] FontSet =
    {
        0xF0, 0x90, 0x90, 0x90, 0xF0,
        0x20, 0x60, 0x20, 0x20, 0x70,
        0xF0, 0x10, 0xF0, 0x80, 0xF0,
        0xF0, 0x10, 0xF0, 0x10, 0xF0,
        0x90, 0x90, 0xF0, 0x10, 0x10,
        0xF0, 0x80, 0xF0, 0x10, 0xF0,
        0xF0, 0x80, 0xF0, 0x90, 0xF0,
        0xF0, 0x10, 0x20, 0x40, 0x40,
        0xF0, 0x90, 0xF0, 0x90, 0xF0,
        0xF0, 0x90, 0xF0, 0x10, 0xF0,
        0xF0, 0x90, 0xF0, 0x90, 0x90,
        0xE0, 0x90, 0xE0, 0x90, 0xE0,
        0xF0, 0x80, 0x80, 0x80, 0xF0,
        0xE0, 0x90, 0x90, 0x90, 0xE0,
        0xF0, 0x80, 0xF0, 0x80, 0xF0,
        0xF0, 0x80, 0xF0, 0x80, 0x80
    };

    private readonly byte[] _memory = new byte[MemorySize];
    private readonly Stack<ushort> _stack = new();

    private readonly byte[] _generalRegisters = new byte[GeneralRegisterCount];
    private ushort _registerI;
    private ushort _programCounter = MemoryBinaryStartAddress;

    private ushort _currentOpcode;

    private byte _delayTimer;
    private byte _soundTimer;

    private readonly bool[] _keypadStatus = new bool[KeyCount];

    public bool DrawFlag;

    public readonly FrameBuffer FrameBuffer = new();

    public Interpreter(byte[] romFile)
    {
        if (romFile.Length is > MemorySize - MemoryBinaryStartAddress or 0)
            throw new ArgumentException("ROM size too big or too small");

        /* Load font set into the memory */
        for (var i = 0; i < FontSet.Length; i++) _memory[0x50 + i] = FontSet[i];

        /* Load the binary into the memory */
        for (var i = 0; i < romFile.Length; i++) _memory[i + MemoryBinaryStartAddress] = romFile[i];
    }

    public void HandleKeyStatus(SDL_Keycode keycode, bool status)
    {
        switch (keycode)
        {
            case SDL_Keycode.SDLK_1:
                _keypadStatus[0] = status;
                break;

            case SDL_Keycode.SDLK_2:
                _keypadStatus[1] = status;
                break;

            case SDL_Keycode.SDLK_3:
                _keypadStatus[2] = status;
                break;

            case SDL_Keycode.SDLK_4:
                _keypadStatus[3] = status;
                break;

            case SDL_Keycode.SDLK_q:
                _keypadStatus[4] = status;
                break;

            case SDL_Keycode.SDLK_w:
                _keypadStatus[5] = status;
                break;

            case SDL_Keycode.SDLK_e:
                _keypadStatus[6] = status;
                break;

            case SDL_Keycode.SDLK_r:
                _keypadStatus[7] = status;
                break;

            case SDL_Keycode.SDLK_a:
                _keypadStatus[8] = status;
                break;

            case SDL_Keycode.SDLK_s:
                _keypadStatus[9] = status;
                break;

            case SDL_Keycode.SDLK_d:
                _keypadStatus[10] = status;
                break;

            case SDL_Keycode.SDLK_f:
                _keypadStatus[11] = status;
                break;

            case SDL_Keycode.SDLK_z:
                _keypadStatus[12] = status;
                break;

            case SDL_Keycode.SDLK_x:
                _keypadStatus[13] = status;
                break;

            case SDL_Keycode.SDLK_c:
                _keypadStatus[14] = status;
                break;

            case SDL_Keycode.SDLK_v:
                _keypadStatus[15] = status;
                break;
        }
    }

    public void TickTimers()
    {
        if (_delayTimer > 0) _delayTimer--;
        if (_soundTimer > 0) _soundTimer--;
    }

    public bool ShouldPlaySound() => _soundTimer != 0;

    public void ExecuteNextInstruction()
    {
        FetchOpcode();

        switch (GetIdentifierFromOpcode())
        {
            case 0x0:
                switch (_currentOpcode)
                {
                    case 0x00EE:
                        FunctionReturn();
                        break;

                    case 0x00E0:
                        FrameBuffer.Clear();
                        DrawFlag = true;
                        break;

                    default:
                        FunctionCall(GetNNNFromOpcode());
                        break;
                }

                break;

            case 0x1:
                _programCounter = GetNNNFromOpcode();
                break;

            case 0x2:
                FunctionCall(GetNNNFromOpcode());
                break;

            case 0x3:
                if (_generalRegisters[GetXFromOpcode()] == GetNNFromOpcode()) _programCounter += 2;

                break;

            case 0x4:
                if (_generalRegisters[GetXFromOpcode()] != GetNNFromOpcode()) _programCounter += 2;

                break;

            case 0x5:
                if (_generalRegisters[GetXFromOpcode()] == _generalRegisters[GetYFromOpcode()]) _programCounter += 2;
                break;

            case 0x6:
                _generalRegisters[GetXFromOpcode()] = GetNNFromOpcode();
                break;

            case 0x7:
                _generalRegisters[GetXFromOpcode()] += GetNNFromOpcode();
                break;

            case 0x8:
                switch (GetNFromOpcode())
                {
                    case 0x0:
                        _generalRegisters[GetXFromOpcode()] = _generalRegisters[GetYFromOpcode()];
                        break;

                    case 0x1:
                        _generalRegisters[GetXFromOpcode()] |= _generalRegisters[GetYFromOpcode()];
                        break;

                    case 0x2:
                        _generalRegisters[GetXFromOpcode()] &= _generalRegisters[GetYFromOpcode()];
                        break;

                    case 0x3:
                        _generalRegisters[GetXFromOpcode()] ^= _generalRegisters[GetYFromOpcode()];
                        break;

                    case 0x4:
                    {
                        var res = (ushort) (_generalRegisters[GetXFromOpcode()] + _generalRegisters[GetYFromOpcode()]);

                        _generalRegisters[GetXFromOpcode()] = (byte) res;
                        _generalRegisters[0xF] = (byte) (res > 0xFF ? 0x1 : 0x0);
                    }
                        break;

                    case 0x5:
                    {
                        var res = (short) (_generalRegisters[GetXFromOpcode()] - _generalRegisters[GetYFromOpcode()]);

                        _generalRegisters[GetXFromOpcode()] = (byte) (res % 0x100);
                        _generalRegisters[0xF] = (byte) (res >= 0x00 ? 0x1 : 0x0);
                    }
                        break;

                    case 0x6:
                        _generalRegisters[0xF] = (byte) ((_generalRegisters[GetXFromOpcode()] & 1) == 1 ? 0x1 : 0x00);
                        _generalRegisters[GetXFromOpcode()] >>= 1;
                        break;

                    case 0x7:
                    {
                        {
                            var res =
                                (short) (_generalRegisters[GetYFromOpcode()] - _generalRegisters[GetXFromOpcode()]);

                            _generalRegisters[GetXFromOpcode()] = (byte) (res % 0x100);
                            _generalRegisters[0xF] = (byte) (res >= 0x00 ? 0x1 : 0x0);
                        }
                    }
                        break;

                    case 0xE:
                        _generalRegisters[0xF] =
                            (byte) ((_generalRegisters[GetXFromOpcode()] & 0b10000000) == 0b10000000 ? 0x1 : 0x0);
                        _generalRegisters[GetXFromOpcode()] <<= 1;
                        break;

                    default:
                        throw new ArgumentException("ROM contains invalid instructions");
                }

                break;

            case 0x9:
                if (_generalRegisters[GetXFromOpcode()] != _generalRegisters[GetYFromOpcode()]) _programCounter += 2;
                break;

            case 0xA:
                _registerI = GetNNNFromOpcode();
                break;

            case 0xB:
                _programCounter = (byte) (GetNNNFromOpcode() + _generalRegisters[0]);
                break;

            case 0xC:
            {
                var r = new Random();

                _generalRegisters[GetXFromOpcode()] =
                    (byte) ((r.Next(byte.MinValue, byte.MaxValue) % 0x100) & GetNNFromOpcode());
            }
                break;

            case 0xD:
                Draw(GetXFromOpcode(), GetYFromOpcode(), GetNFromOpcode());
                break;

            case 0xE:
                switch (GetNNFromOpcode())
                {
                    case 0x9E:
                        if (_keypadStatus[_generalRegisters[GetXFromOpcode()]]) _programCounter += 2;
                        break;

                    case 0xA1:
                        if (!_keypadStatus[_generalRegisters[GetXFromOpcode()]]) _programCounter += 2;
                        break;

                    default:
                        throw new ArgumentException("ROM contains invalid instructions");
                }

                break;

            case 0xF:
                switch (GetNNFromOpcode())
                {
                    case 0x07:
                        _generalRegisters[GetXFromOpcode()] = _delayTimer;
                        break;

                    case 0x0A:
                    {
                        var keyPressed = false;

                        for (var i = 0; i < 16; i++)
                            if (_keypadStatus[i])
                            {
                                _generalRegisters[GetXFromOpcode()] = (byte) i;
                                keyPressed = true;
                            }

                        // If not pressed, stay on this instruction
                        if (!keyPressed) _programCounter -= 2;
                    }
                        break;

                    case 0x15:
                        _delayTimer = _generalRegisters[GetXFromOpcode()];
                        break;

                    case 0x18:
                        _soundTimer = _generalRegisters[GetXFromOpcode()];
                        break;

                    case 0x1E:
                        _registerI += _generalRegisters[GetXFromOpcode()];
                        break;

                    case 0x29:
                        _registerI = (byte) (_generalRegisters[GetXFromOpcode()] * 5);
                        break;

                    case 0x33:
                    {
                        var regX = _generalRegisters[GetXFromOpcode()];

                        _memory[_registerI] = (byte) (regX / 100);
                        _memory[_registerI + 1] = (byte) (regX / 10 % 10);
                        _memory[_registerI + 2] = (byte) (regX % 10);
                    }
                        break;

                    case 0x55:
                        for (var i = 0; i <= GetXFromOpcode(); i++) _memory[_registerI + i] = _generalRegisters[i];
                        break;

                    case 0x65:
                        for (var i = 0; i <= GetXFromOpcode(); i++) _generalRegisters[i] = _memory[_registerI + i];
                        break;

                    default:
                        throw new ArgumentException("ROM contains invalid instructions");
                }

                break;

            default:
                throw new ArgumentException("ROM contains invalid instructions");
        }
    }

    private void Draw(byte x, byte y, ushort height)
    {
        var xCord = _generalRegisters[x];
        var yCord = _generalRegisters[y];

        var pixelFlipped = false;

        for (var diffY = 0; diffY < height; diffY++)
        {
            var r = _memory[_registerI + diffY];

            for (var diffX = 0; diffX < 8; diffX++)
                if ((r & (1 << (7 - diffX))) != 0)
                {
                    FrameBuffer.FlipPixel(xCord + diffX, yCord + diffY);
                    if (!FrameBuffer.GetPixelFrom2DCords(xCord + diffX, yCord + diffY)) pixelFlipped = true;
                }
        }

        DrawFlag = true;
        _generalRegisters[0xF] = (byte) (pixelFlipped ? 0x1 : 0x0);
    }

    private void FetchOpcode()
    {
        _currentOpcode = (ushort) ((_memory[_programCounter] << 8) | _memory[_programCounter + 1]);
        _programCounter += 2;
    }

    private void FunctionCall(ushort address)
    {
        _stack.Push(_programCounter);
        _programCounter = address;
    }

    private void FunctionReturn() => _programCounter = _stack.Pop();

    private byte GetIdentifierFromOpcode() => (byte) ((_currentOpcode & 0xF000) >> 12);

    private byte GetXFromOpcode() => (byte) ((_currentOpcode & 0x0F00) >> 8);

    private byte GetYFromOpcode() => (byte) ((_currentOpcode & 0x00F0) >> 4);

    private ushort GetNNNFromOpcode() => (ushort) (_currentOpcode & 0x0FFF);

    private byte GetNNFromOpcode() => (byte) (_currentOpcode & 0x00FF);

    private byte GetNFromOpcode() => (byte) (_currentOpcode & 0x000F);
}