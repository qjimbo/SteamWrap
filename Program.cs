using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace SteamWrap
{
    class Program
    {
        private static readonly Dictionary<string, CommandDefinition> Commands = new Dictionary<string, CommandDefinition>
        {
            {"configure", new CommandDefinition(CommandType.Configure, 0)},
            {"shortcut", new CommandDefinition(CommandType.Shortcut, 4)},
            {"shutdown-steam", new CommandDefinition(CommandType.ShutdownSteam, 0)}
        };

        static void Main(string[] args)
        {
            Console.WriteLine("SteamWrap - https://github.com/qjimbo/SteamWrap/");
            Console.WriteLine("Command Line Detected: " + Environment.CommandLine);

            if (args.Length == 0)
            {
                DisplayHelpInformation();
                Console.WriteLine("\nPress Enter to exit...");
                Console.ReadLine();
                return;
            }

            string gameId = null;
            int startIndex = 0;

            // Check if the first argument is a potential game ID (not a known command)
            if (args.Length > 0 && !Commands.ContainsKey(args[0].ToLower()))
            {
                gameId = args[0];
                startIndex = 1;
            }

            var parsedCommands = ParseCommands(args.Skip(startIndex).ToArray());

            if (parsedCommands.Count == 0)
            {
                // Assume Steam launch mode
                SteamLaunchGameMode(args);
            }
            else
            {
                SteamClient.Stop();
                foreach (var command in parsedCommands)
                {
                    ExecuteCommand(gameId, command);
                }
                SteamClient.Restart();
            }
            Console.WriteLine("\nPress Enter to exit...");
            Console.ReadLine();
        }

        private static List<ParsedCommand> ParseCommands(string[] args)
        {
            var parsedCommands = new List<ParsedCommand>();
            int i = 0;

            while (i < args.Length)
            {
                if (Commands.TryGetValue(args[i].ToLower(), out var commandDef))
                {
                    if (i + commandDef.ArgCount < args.Length)
                    {
                        parsedCommands.Add(new ParsedCommand
                        {
                            Type = commandDef.Type,
                            Args = args.Skip(i + 1).Take(commandDef.ArgCount).ToArray()
                        });
                        i += 1 + commandDef.ArgCount;
                    }
                    else
                    {
                        Console.WriteLine($"Insufficient arguments for {args[i]}. Skipping.");
                        i++;
                    }
                }
                else
                {
                    Console.WriteLine($"Unknown command: {args[i]}. Skipping.");
                    i++;
                }
            }

            return parsedCommands;
        }

        private static void ExecuteCommand(string gameId, ParsedCommand command)
        {
            switch (command.Type)
            {
                case CommandType.Configure:
                    if (string.IsNullOrEmpty(gameId))
                    {
                        Console.WriteLine("Error: Game ID is required for configure command.");
                        return;
                    }
                    ConfigureLaunchOptions(gameId);
                    break;
                case CommandType.Shortcut:
                    if (string.IsNullOrEmpty(gameId))
                    {
                        Console.WriteLine("Error: Game ID is required for shortcut command.");
                        return;
                    }
                    CreateShortcut(gameId, command.Args);
                    break;
                case CommandType.ShutdownSteam:
                    Console.WriteLine("Shutting Down Steam");
                    SteamClient.Stop();
                    break;
            }
        }

        private static void SteamLaunchGameMode(string[] args)
        {
            if (args.Length >= 1)
            {
                string originalExePath = args[0].Trim('"');
                string launchExePath = args.Length > 1 ? args[1].Trim('"') : null;
                

                if (launchExePath != null && File.Exists(launchExePath))
                {
                    string[] gameArgs = args.Length > 2 ? args.Skip(2).ToArray() : null;

                    Console.WriteLine($"Launching Alternative Executable: {launchExePath}");
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = launchExePath,
                        Arguments = gameArgs != null ? string.Join(" ", gameArgs) : "",
                        UseShellExecute = false
                    };
                    Process.Start(startInfo);
                }
                else if (File.Exists(originalExePath))
                {
                    string[] gameArgs = args.Length > 1 ? args.Skip(1).ToArray() : null;

                    Console.WriteLine($"Launching Original Executable: {originalExePath}");
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = originalExePath,
                        Arguments = gameArgs != null ? string.Join(" ", gameArgs) : "",
                        UseShellExecute = false
                    };
                    Process.Start(startInfo);
                }
                else
                {
                    Console.WriteLine($"Executable not found: {originalExePath}");
                }
            }
            else
            {
                Console.WriteLine("Invalid arguments for game launch.");
            }
        }

        private static void DisplayHelpInformation()
        {
            Console.WriteLine("\nSteamWrap Usage Instructions:");
            Console.WriteLine("------------------------------");
            Console.WriteLine("1. Configure launch options:");
            Console.WriteLine("   SteamWrap <game-id> configure");
            Console.WriteLine("\n2. Create shortcuts:");
            Console.WriteLine("   SteamWrap <game-id> shortcut <type> <exe-path> <shortcut-name> <icon-path>");
            Console.WriteLine("   Types: desktop, steam, both");
            Console.WriteLine("\n3. Shutdown Steam:");
            Console.WriteLine("   SteamWrap shutdown-steam");
            Console.WriteLine("\n4. Launch game (used by Steam):");
            Console.WriteLine("   SteamWrap <original-exe> [custom-exe] [args]");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("SteamWrap 271590 configure");
            Console.WriteLine("SteamWrap 271590 shortcut desktop \"C:\\Games\\GTA5.exe\" \"Grand Theft Auto V\" \"C:\\Games\\GTA5.ico\"");
            Console.WriteLine("SteamWrap 271590 configure shortcut both \"C:\\Games\\GTA5.exe\" \"GTA V\" \"C:\\Games\\GTA5.ico\"");
        }

        private static void ConfigureLaunchOptions(string gameId)
        {
            string currentExePath = Process.GetCurrentProcess().MainModule.FileName;
            string newLaunchOptions = $"\"{currentExePath}\" %command%";
            SetSteamLaunchOptions(gameId, newLaunchOptions);
            Console.WriteLine($"Launch options set for game {gameId}");
        }

        private static void CreateShortcut(string gameId, string[] args)
        {
            string shortcutType = args[0].ToLower();
            string exePath = args[1].Trim('"');
            string shortcutName = args[2].Trim('"');
            string iconPath = args[3].Trim('"');
            string steamPath = Path.Combine(GetSteamInstallationFolder(), "steam.exe");


            switch (shortcutType)
            {
                case "desktop":
                    CreateDesktopShortcut(shortcutName, iconPath, steamPath, $"-applaunch {gameId} \"" + exePath + "\"");
                    break;
                case "steam":
                    AddNonSteamGame(shortcutName, iconPath, steamPath, $"-applaunch {gameId} \"" + exePath + "\"");
                    break;
                case "both":
                    CreateDesktopShortcut(shortcutName, iconPath, steamPath, $"-applaunch {gameId} \"" + exePath + "\"");
                    AddNonSteamGame(shortcutName, iconPath, steamPath, $"-applaunch {gameId} \"" + exePath + "\"");
                    break;
                default:
                    Console.WriteLine("Invalid shortcut type. Use 'desktop', 'steam', or 'both'.");
                    break;
            }
        }

        private static void SetSteamLaunchOptions(string gameId, string newLaunchOptions)
        {
            string filePath = Path.Combine(GetSteamConfigFolder(), "localconfig.vdf");
            if (!File.Exists(filePath))
            {
                throw new Exception("localconfig.vdf file not found in the first user ID folder.");
            }

            Console.WriteLine("Reading " + filePath + "...");
            var result = VdfRead.ParseVdf(filePath);
            var gameKey = VdfRead.FindGameKey(result, gameId);

            var launchOptionsItem = gameKey.Nested?.FirstOrDefault(item => item.Key == "LaunchOptions");
            if (launchOptionsItem != null)
            {
                // Update existing LaunchOptions
                launchOptionsItem.Value = newLaunchOptions;
            }
            else
            {
                // Create new LaunchOptions
                gameKey.Nested.Add(new VdfItem
                {
                    Key = "LaunchOptions",
                    Value = newLaunchOptions
                });
            }

            Console.WriteLine("Writing " + filePath + "...");
            VdfWrite.VdfWriteItem(gameKey, filePath, filePath);
        }

        private static void CreateDesktopShortcut(string shortcutName, string shortcutIcon, string shortcutTarget, string shortcutArgsString)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = Path.Combine(desktopPath, $"{shortcutName}.lnk");

            var shell = new IWshRuntimeLibrary.WshShell();
            var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = shortcutTarget;
            shortcut.Arguments = shortcutArgsString;
            shortcut.WorkingDirectory = Path.GetDirectoryName(shortcutTarget);
            shortcut.Description = "";
            shortcut.IconLocation = shortcutIcon;
            shortcut.Save();

            Console.WriteLine($"Desktop shortcut created: {shortcutPath}");
        }

        private static void AddNonSteamGame(string shortcutName, string shortcutIcon, string shortcutPath, string shortcutArgsString)
        {
            string steamPath = GetSteamInstallationFolder();
            string shortcutsVdfPath = Path.Combine(GetSteamConfigFolder(), "shortcuts.vdf");

            VdfWriteBinary.VdfWriteBinaryShortcut(shortcutsVdfPath, shortcutName, shortcutIcon, shortcutPath, shortcutArgsString);

            Console.WriteLine($"Non-Steam game added: {shortcutName}");
        }

        public static string GetSteamConfigFolder()
        {
            string steamInstallPath = GetSteamInstallationFolder();
            if (string.IsNullOrEmpty(steamInstallPath))
            {
                throw new Exception("Steam installation folder not found.");
            }

            string userdataPath = Path.Combine(steamInstallPath, "userdata");
            if (!Directory.Exists(userdataPath))
            {
                throw new Exception("Userdata folder not found in the Steam installation directory.");
            }

            string[] userIdFolders = Directory.GetDirectories(userdataPath);
            if (userIdFolders.Length == 0)
            {
                throw new Exception("No user ID folders found in the userdata directory.");
            }

            string firstUserIdFolder = userIdFolders[0];
            string configPath = Path.Combine(firstUserIdFolder, "config");

            return configPath;
        }

        private static string GetSteamInstallationFolder()
        {
            // Check default installation path
            string defaultPath = @"C:\Program Files (x86)\Steam";
            if (Directory.Exists(defaultPath))
            {
                return defaultPath;
            }

            // Check registry for installation path
            string registryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam";
            string registryValue = "InstallPath";
            string installPath = (string)Registry.GetValue(registryKey, registryValue, null);

            if (installPath != null && Directory.Exists(installPath))
            {
                return installPath;
            }

            return null;
        }
    }

    enum CommandType
    {
        Configure,
        Shortcut,
        ShutdownSteam
    }

    class CommandDefinition
    {
        public CommandType Type { get; }
        public int ArgCount { get; }

        public CommandDefinition(CommandType type, int argCount)
        {
            Type = type;
            ArgCount = argCount;
        }
    }

    class ParsedCommand
    {
        public CommandType Type { get; set; }
        public string[] Args { get; set; }
    }
}