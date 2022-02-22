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

namespace CalicoNET;

public class FrameBuffer
{
    private const int ResolutionX = 64;
    private const int ResolutionY = 32;

    private readonly byte[] _rawBuffer = new byte[ResolutionX * ResolutionY * sizeof(uint)];

    public bool GetPixelFrom2DCords(int x, int y) =>
        _rawBuffer[CalculateArrayIndexFrom2DCords(x, y, ResolutionX, ResolutionY) * sizeof(uint)] != 0x00;

    public void FlipPixel(int x, int y)
    {
        var index = CalculateArrayIndexFrom2DCords(x, y, ResolutionX, ResolutionY) * sizeof(uint);

        var originalPixelStatus = _rawBuffer[index] == 0xFF;

        _rawBuffer[index] = (byte) (originalPixelStatus ? 0x00 : 0xFF);
        _rawBuffer[index + 1] = (byte) (originalPixelStatus ? 0x00 : 0xFF);
        _rawBuffer[index + 2] = (byte) (originalPixelStatus ? 0x00 : 0xFF);
        _rawBuffer[index + 3] = (byte) (originalPixelStatus ? 0x00 : 0xFF);
    }

    public void Clear()
    {
        for (var i = 0; i < _rawBuffer.Length; i++) _rawBuffer[i] = 0x00;
    }

    public unsafe IntPtr GetSDLPixelArray()
    {
        fixed (byte* p = _rawBuffer)
        {
            var ptr = (IntPtr) p;

            return ptr;
        }
    }

    private static int CalculateArrayIndexFrom2DCords(int x, int y, int w, int h) => y % h * w + x % w;
}