using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Permissions;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Diagnostics;

namespace CPULimiter
{
    public class CPULimiterPlugin : RocketPlugin<CPULimiterConfiguration>
    {
        private static Process? cpuLimit;
        private readonly List<string> playersOnline = new();
        private bool standbyState = false;
        private bool isInitializating = true;
        public override void LoadPlugin()
        {
            base.LoadPlugin();
            if (Configuration.Instance.ProcessPath == null && Configuration.Instance.ProcessName == null)
            {
                Logger.LogError("[CPULimiter] No process name or path has been set please check the configurations");
                base.UnloadPlugin();
                return;
            }
            Logger.Log("CPULimiter by LeandroTheDev");
            UnturnedPermissions.OnJoinRequested += PlayerTryingToConnect;

            //  Get all process running on the system
            Process[] processes = Process.GetProcesses();

            // Swipe every process
            if (Configuration.Instance.DebugProcess) Logger.Log("[CPULimiter] -----Process-----");
            foreach (Process process in processes)
            {
                try
                {

                    if (Configuration.Instance.DebugProcess) Logger.Log($"[CPULimiter] {process.ProcessName}");
                    if (Configuration.Instance.ProcessName != null && process.ProcessName.Contains(Configuration.Instance.ProcessName.ToString()))
                    {
                        // Check if the process already exist
                        if (CPULimiterTools.UnturnedProcessId != null)
                            Logger.LogWarning("[CPULimiter] Two process with the same ProcessName have been found, please ensure the ProcessName is unique into your user or consider using the ProcessPath instead, you can check the process using the configuration DebugProcess");
                        CPULimiterTools.UnturnedProcessId = process.Id;
                    }
                    else if (Configuration.Instance.ProcessPath != null && process.ProcessName.ToString() == Configuration.Instance.ProcessPath.ToString())
                    {
                        // Check if the process already exist
                        if (CPULimiterTools.UnturnedProcessId != null)
                            Logger.LogWarning("[CPULimiter] Two process with the same ProcessPath have been found, check your configuration probably you forget to change the ProcessPath to your new server, please ensure the ProcessPath is unique into your user, you can check the process using the configuration DebugProcess");
                        CPULimiterTools.UnturnedProcessId = process.Id;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[CPULimiter] Cannot read the process reason: {ex.Message}");
                }
            }

            // Check if process exist
            if (CPULimiterTools.UnturnedProcessId == null)
            {
                Logger.LogError($"[CPULimiter] The plugin failed to find any process");
                base.UnloadPlugin();
                return;
            }

            // Entering in standby
            Task.Delay(Configuration.Instance.SecondsFirstStandby * 1000).ContinueWith((_) =>
            {
                isInitializating = false;
                if (playersOnline.Count == 0 && Configuration.Instance.CPULimitInStandby != 0)
                    EnableCPUStandby(Configuration.Instance.CPULimitInStandby, true);
            });
            // Timer for checking online players
            new Timer(CheckForPlayers, null, 0, Configuration.Instance.SecondsCheckNoPlayers * 1000);

            Rocket.Unturned.U.Events.OnPlayerConnected += OnPlayerConnected;
            Rocket.Unturned.U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            playersOnline.Remove(player.Id);
        }
        private void OnPlayerConnected(UnturnedPlayer player)
        {
            playersOnline.Add(player.Id);
        }
        private void CheckForPlayers(object state)
        {
            // If no players enter in standby
            if (playersOnline.Count == 0 && !standbyState && !isInitializating && Configuration.Instance.CPULimitInStandby != 0)
            {
                Logger.Log("[CPULimiter] No players detected entering in standby");
                EnableCPUStandby(Configuration.Instance.CPULimitInStandby, true);
            }
        }
        private void PlayerTryingToConnect(CSteamID player, ref ESteamRejection? rejectionReason)
        {
            if (standbyState && Configuration.Instance.CPULimitInStandby != 0)
                DisableCPUStandby();
        }
        private void EnableCPUStandby(int amount, bool _standbyState)
        {
            cpuLimit?.Close();
            cpuLimit?.Dispose();

            // Command
            string command = $"cpulimit -p {CPULimiterTools.UnturnedProcessId} -l {amount}";
            if (Configuration.Instance.DebugProcess) Logger.Log($"[CPULimiter] Executing the command: {command}");

            // Create the process
            cpuLimit = new();
            cpuLimit.StartInfo.FileName = "/bin/bash";
            cpuLimit.StartInfo.Arguments = $"-c \"{command}\"";
            cpuLimit.StartInfo.RedirectStandardOutput = true;
            cpuLimit.StartInfo.RedirectStandardError = true;
            cpuLimit.StartInfo.UseShellExecute = false;
            cpuLimit.StartInfo.CreateNoWindow = true;

            // Process output
            cpuLimit.OutputDataReceived += (sender, args) => Logger.Log($"[CPULimiter] cpulimit output: {args.Data}");
            cpuLimit.ErrorDataReceived += (sender, args) => Logger.LogError($"[CPULimiter] cpulimit error: {args.Data}");

            // Start the process
            cpuLimit.Start();

            // Read the process output and error
            cpuLimit.BeginOutputReadLine();
            cpuLimit.BeginErrorReadLine();

            standbyState = _standbyState;
            CPULimiterTools.IsStandByMode = standbyState;

            Logger.Log($"[CPULimiter] CPU Limited to {amount}");
        }
        private void DisableCPUStandby()
        {
            // Check if theres limitation without standby
            if (Configuration.Instance.CPULimitOutStandby != 0)
                EnableCPUStandby(Configuration.Instance.CPULimitOutStandby, false);
            // Otherwise just dispose the cpu limitation
            else
            {
                cpuLimit?.Close();
                cpuLimit?.Dispose();
                cpuLimit = null;
                standbyState = false;
                CPULimiterTools.IsStandByMode = false;
            };
        }
    }

    static public class CPULimiterTools
    {
        /// <summary>
        /// The server process PID
        /// </summary>
        public static int? UnturnedProcessId = null;
        /// <summary>
        /// Check if the server is on standby mode
        /// </summary>
        public static bool IsStandByMode = false;
    }
}
