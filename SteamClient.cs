using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SteamWrap
{
    class SteamClient
    {
        private static string steamPath;

        public static void Stop()
        {
            var steamProcess = Process.GetProcessesByName("Steam").FirstOrDefault();
            if (steamProcess != null)
            {
                steamPath = steamProcess.MainModule.FileName;

                Console.WriteLine("Steam client running, sending shutdown signal...");
                // Send Shutdown Command
                ProcessStartInfo shutdownSteamProcessStartInfo = new ProcessStartInfo
                {
                    FileName = steamPath,
                    Arguments = "-shutdown",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                using (Process shutdownSteamProcess = new Process { StartInfo = shutdownSteamProcessStartInfo })
                {
                    shutdownSteamProcess.Start();
                }

                // Wait for Original Process to Terminate

                steamProcess.WaitForExit();
                Console.WriteLine("Steam client shutdown successfully.");
            }
        }

        public static void Restart()
        {
            if (!string.IsNullOrEmpty(steamPath))
            {
                ProcessStartInfo startSteamProcessStartInfo = new ProcessStartInfo
                {
                    FileName = steamPath,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                using (Process startSteamProcess = new Process { StartInfo = startSteamProcessStartInfo })
                {
                    startSteamProcess.Start();
                }
                Console.WriteLine("Steam client restarted.");
            }            
        }
    }
}
