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

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No ROM provided");
            return;
        }

        var romPath = args[0];

        CommandLineArgs parsedArgs;
        try
        {
            parsedArgs = CommandLineArgs.Parse(args[1..]);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine($"Unable to parse command line arguments: {e.Message}");
            return;
        }

        try
        {
            var emulator = new Emulator(parsedArgs, romPath);

            emulator.Run();
        }
        catch (SDLException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (IOException)
        {
            Console.WriteLine("Unable to read provided ROM file");
        }
        catch (ArgumentException e)
        {
            Console.WriteLine("Invalid ROM file provided: " + e.Message);
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Stack underflow, possibly corrupted ROM");
        }
    }
}