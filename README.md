# CalicoNET

Cross-platform Chip8 emulator written in C#.

### Progress

- [x] Graphics
- [x] Sound
- [x] Input
- [x] All instructions
- [x] User configurable window size
- [x] User configurable clock speed

## Getting Started

### Dependencies

* .NET Core SDK
* [SDL2]("https://www.libsdl.org")

### Usage

* Clone the repository

```
git clone https://github.com/Xiperiz/calico-net
```

* Compile the program

```
msbuild
```

* Launch a ROM

```
calico-net <path-to-rom> <args>
```

### Command line arguments

* -no_sound - disables 'beep' sound.
* -clock_speed:x - sets clock speed to X hz
* -window_size:x:y - sets window size to X by Y

The arguments with values need to have a format specified above (-arg:val), below is an example with all of the
arguments used together:

```
calico-net SpaceInvaders.ch8 -no_sound -clock_speed:600 -window_size:1280:640
```

You can omit any argument and the default will be used, below are default values for each argument:

* -no_sound - false
* -clock_speed - 600hz
* -window_size - 640 x 320

Keep in mind there are no checks for the values, if you put ridiculous values then expect unexpected behaviour!

## License

This project is licensed under the [GNU AGPLv3] License - see the [LICENSE.md](LICENSE.md) file for details.