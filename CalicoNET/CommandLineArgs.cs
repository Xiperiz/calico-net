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

public class CommandLineArgs
{
    private CommandLineArgs()
    {
    }

    public int ClockSpeed { get; private set; } = 600;

    public bool SoundEnabled { get; private set; } = true;

    public int WindowSizeX { get; private set; } = 640;
    public int WindowSizeY { get; private set; } = 320;

    public static CommandLineArgs Parse(string[] args)
    {
        var parsed = new CommandLineArgs();

        foreach (var arg in args)
        {
            var argTokens = arg.Split(":");

            switch (argTokens[0])
            {
                case "-no_sound":
                    if (argTokens.Length == 1)
                        parsed.SoundEnabled = false;
                    else
                        throw new ArgumentException($"Invalid command line argument: {arg}");

                    break;

                case "-clock_speed":
                    if (argTokens.Length != 2) throw new ArgumentException($"Invalid command line argument: {arg}");

                    try
                    {
                        parsed.ClockSpeed = int.Parse(argTokens[1]);
                    }
                    catch (Exception e) when (e is FormatException or OverflowException)
                    {
                        throw new ArgumentException($"Invalid command line argument: {arg}");
                    }

                    break;

                case "-window_size":
                    if (argTokens.Length != 3) throw new ArgumentException($"Invalid command line argument: {arg}");

                    try
                    {
                        parsed.WindowSizeX = int.Parse(argTokens[1]);
                        parsed.WindowSizeY = int.Parse(argTokens[2]);
                    }
                    catch (Exception e) when (e is FormatException or OverflowException)
                    {
                        throw new ArgumentException($"Invalid command line argument: {arg}");
                    }

                    break;

                default:
                    throw new ArgumentException($"Invalid command line argument: {arg}");
            }
        }

        return parsed;
    }
}