<img src="https://github.com/qjimbo/SteamWrap/assets/21266513/5a4ad25c-0701-42db-881a-6f11edb3dc76"  width="300px"></img>
# SteamWrap
SteamWrap is a utility for launching alternative game versions from Steam. It works by configuring Steam to launch SteamWrap instead of the game exe. By default SteamWrap will launch the default game as normal, but with an additional argument it will instead launch an alternative exe, which is typically an older version of a game. The benefit of this is each child process preserves the Steam authentication, allowing you to run old versions without having to modify them to run outside of steam.

## Features

- Automatically configures Steam LaunchOptions for games to ```SteamWrap.exe %command%``` to allow SteamWrap to redirect to alternative versions 
- Create desktop and Steam shortcuts for alternative game versions
- Launch games through Steam while redirecting to different executables
- When no arguments given, will launch the original version of the game transparently

## Usage
SteamWrap <game-id> [command] [arguments]

### Commands
1. Enable SteamWrap - specify which game by steam game id and it will set launchoptions to run SteamWrap when the game is started.
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
This project is licensed under the Apache 2.0 license. See the [LICENSE](LICENSE) file for details.

## Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Disclaimer

This software is not affiliated with Valve Corporation or Steam. Use at your own risk.
