# SteamWrap

SteamWrap is a utility for creating shortcuts that launch alternative game versions from Steam. It allows you to configure Steam games to launch through SteamWrap, enabling you to change which version of a game is launched without altering the original game files.

## Features

- Automatically configures Steam LaunchOptions for games to ```SteamWrap.exe %command%``` to allow redirecting to alternative versions 
- Create desktop and Steam shortcuts for alternative game versions
- Launch games through Steam while redirecting to different executables
- When no arguments given, will launch the original version of the game transparently

## Usage
SteamWrap <game-id> [command] [arguments]

### Commands
1. Configure launch options:
   ```
   SteamWrap <game-id> configure
3. Create shortcuts:
   ```
   SteamWrap <game-id> shortcut <type> <exe-path> <shortcut-name> <icon-path>
   ```
   Types: desktop, steam, both

4. Shutdown Steam:
   ```
   SteamWrap shutdown-steam
   ```

### Examples
```
SteamWrap 271590 configure
SteamWrap 271590 shortcut desktop "C:\Games\MyGame.exe" "Old Version of My Game" "C:\Games\CoolIcon.ico"
SteamWrap 271590 configure shortcut both  "C:\Games\MyGame.exe" "Old Version of My Game" "C:\Games\CoolIcon.ico"
```

## Installation
1. Clone this repository or download the latest release.
2. Build the project using Visual Studio 2019 or your preferred C# compiler.
3. Place the compiled SteamWrap executable in a location of your choice.

## Requirements
- .NET Framework 4.8.1
- Windows operating system

## Credits
- Big thanks to CheatFreak for sharing the knowledge about this method, as well as providing an implementation written in Auto Hot Key. 🙏
- Also inspired by [SMAPI](https://smapi.io/) which uses a similar approach for Stardew Valley modding.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Disclaimer

This software is not affiliated with Valve Corporation or Steam. Use at your own risk.
