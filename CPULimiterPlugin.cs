﻿extern alias UnityEngineCoreModule;

using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Permissions;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Diagnostics;
using UnityEngine;

namespace CPULimiter
{
    public class CPULimiterPlugin : RocketPlugin<CPULimiterConfiguration>
    {
        private static Process? cpuLimit;
        private readonly List<string> playersOnline = new();
        private bool isInitializating = true;
        private TickrateFreeze? tickrateFreeze;
        public override void LoadPlugin()
        {
            base.LoadPlugin();
            if (Configuration.Instance.ProcessPath == null && Configuration.Instance.ProcessName == null)
            {
                Logger.LogError("[CPULimiter] No process name or path has been set please check the configurations");
                base.UnloadPlugin();
                return;
            }
            Logger.Log("CPULimiter from github.com/LeandroTheDev/cpu_limiter");
            UnturnedPermissions.OnJoinRequested += PlayerTryingToConnect;

            if (!Configuration.Instance.GetCurrentlyProcess)
            {

                //  Get all process running on the system
                Process[] processes = Process.GetProcesses();

                // Swipe every process
                if (Configuration.Instance.DebugProcess) Logger.Log("[CPULimiter] -----Process-----");
                foreach (Process process in processes)
                {
                    try
                    {
                        if (Configuration.Instance.DebugProcess)
                        {
                            Logger.Log($"[CPULimiter] {process.ProcessName}");
                        }
                        else if (Configuration.Instance.ProcessPath != null && process.ProcessName.ToString() == Configuration.Instance.ProcessPath.ToString())
                        {
                            // Check if the process already exist
                            if (CPULimiterTools.UnturnedProcessId != null)
                                Logger.LogWarning("[CPULimiter] Two process with the same ProcessPath have been found, check your configuration probably you forget to change the ProcessPath to your new server, please ensure the ProcessPath is unique into your user, you can check the process using the configuration DebugProcess");
                            CPULimiterTools.UnturnedProcessId = process.Id;
                        }
                        else if (Configuration.Instance.ProcessName != null && process.ProcessName.Contains(Configuration.Instance.ProcessName.ToString()))
                        {
                            // Check if the process already exist
                            if (CPULimiterTools.UnturnedProcessId != null)
                                Logger.LogWarning("[CPULimiter] Two process with the same ProcessName have been found, please ensure the ProcessName is unique into your user or consider using the ProcessPath instead, you can check the process using the configuration DebugProcess");
                            CPULimiterTools.UnturnedProcessId = process.Id;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"[CPULimiter] Cannot read the process reason: {ex.Message}");
                    }
                }
            }
            else CPULimiterTools.UnturnedProcessId = Process.GetCurrentProcess().Id;

            // Check if process exist
            if (CPULimiterTools.UnturnedProcessId == null)
            {
                Logger.LogError($"[CPULimiter] The plugin failed to find any process");
                base.UnloadPlugin();
                return;
            }
            else Logger.Log($"[CPULimiter] CPULimiter will limit the process with the PID: {CPULimiterTools.UnturnedProcessId}, consider checking if this is the valid process");

            tickrateFreeze = gameObject.AddComponent<TickrateFreeze>();
            tickrateFreeze.ServerTickrate = Configuration.Instance.ServerTickrate;
            tickrateFreeze.DesiredTickrate = Configuration.Instance.TickrateOutStandby;

            // Entering in standby
            Task.Delay(Configuration.Instance.SecondsFirstStandby * 1000).ContinueWith((_) =>
            {
                isInitializating = false;
                if (playersOnline.Count == 0 && Configuration.Instance.CPULimitInStandby > -1)
                    EnableCPUStandby(Configuration.Instance.CPULimitInStandby);
            });
            // Timer for checking online players
            new Timer(CheckForPlayers, null, 0, Configuration.Instance.SecondsCheckNoPlayers * 1000);

            Rocket.Unturned.U.Events.OnPlayerConnected += OnPlayerConnected;
            Rocket.Unturned.U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            playersOnline.Remove(player.Id);
            CheckForPlayers(null);
        }
        private void OnPlayerConnected(UnturnedPlayer player)
        {
            playersOnline.Add(player.Id);
            CheckForPlayers(null);
        }

        private void CheckForPlayers(object? state)
        {
            // If no players enter in standby
            if (playersOnline.Count == 0 && !CPULimiterTools.IsStandByMode && !isInitializating)
            {
                Logger.Log("[CPULimiter] No players detected entering in standby");
                EnableCPUStandby(Configuration.Instance.CPULimitInStandby);
            }
            // If no players and out standyby disabled
            else if (playersOnline.Count > 0 && CPULimiterTools.IsStandByMode)
            {
                Logger.Log("[CPULimiter] Players detected disabling limitcpu");
                DisableCPUStandby();
            }
        }
        // Event for player trying to connect
        private void PlayerTryingToConnect(CSteamID player, ref ESteamRejection? rejectionReason)
        {
            if (CPULimiterTools.IsStandByMode)
            {
                Logger.Log("[CPULimiter] Player trying to connect to the server, disabling standby");
                DisableCPUStandby();
            }
        }

        // Enable the limitcpu to the currently unturned process id
        private void EnableCPUStandby(int amount)
        {
            if (CPULimiterTools.IsStandByMode) return;

            if (amount > -1)
            {
                cpuLimit?.Kill();
                cpuLimit?.Close();
                cpuLimit?.Dispose();

                // Command
                string command = $"limitcpu -p {CPULimiterTools.UnturnedProcessId} -l {amount}";
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
                cpuLimit.OutputDataReceived += (sender, args) => Logger.Log($"[CPULimiter] limitcpu output: {args.Data}");
                cpuLimit.ErrorDataReceived += (sender, args) => Logger.LogError($"[CPULimiter] limitcpu error: {args.Data}");

                // Start the process
                cpuLimit.Start();

                // Read the process output and error
                cpuLimit.BeginOutputReadLine();
                cpuLimit.BeginErrorReadLine();
            }

            CPULimiterTools.IsStandByMode = true;

            if (tickrateFreeze != null)
                tickrateFreeze.DesiredTickrate = Configuration.Instance.TickrateInStandby;

            Logger.Log($"[CPULimiter] CPU Limited to {amount}, Tickrate: {Configuration.Instance.TickrateInStandby}");
        }

        // Disable and dispose the limitcpu process, enable the cpu out standby if enabled
        private void DisableCPUStandby()
        {
            cpuLimit?.Kill();
            cpuLimit?.Close();
            cpuLimit?.Dispose();
            cpuLimit = null;

            CPULimiterTools.IsStandByMode = false;

            if (tickrateFreeze != null)
            {
                tickrateFreeze.DesiredTickrate = Configuration.Instance.TickrateOutStandby;
                Logger.Log($"[CPULimiter] Out Standby Tickrate: {Configuration.Instance.TickrateOutStandby}");
            }

            if (Configuration.Instance.CPULimitOutStandby > -1)
                EnableCPUOutStandby(Configuration.Instance.CPULimitOutStandby);
        }

        // Enable Limitation while without standby
        private void EnableCPUOutStandby(int amount)
        {
            if (!CPULimiterTools.IsStandByMode) return;

            if (amount > -1)
            {

                cpuLimit?.Kill();
                cpuLimit?.Close();
                cpuLimit?.Dispose();

                // Command
                string command = $"limitcpu -p {CPULimiterTools.UnturnedProcessId} -l {amount}";
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
                cpuLimit.OutputDataReceived += (sender, args) => Logger.Log($"[CPULimiter] limitcpu output: {args.Data}");
                cpuLimit.ErrorDataReceived += (sender, args) => Logger.LogError($"[CPULimiter] limitcpu error: {args.Data}");

                // Start the process
                cpuLimit.Start();

                // Read the process output and error
                cpuLimit.BeginOutputReadLine();
                cpuLimit.BeginErrorReadLine();
            }

            CPULimiterTools.IsStandByMode = false;

            Logger.Log($"[CPULimiter] Out Standby CPU Limited to {amount}");
        }

        // Disable and dispose the cpu limit process for not in standby, enable the cpu standby if enabled
        private void DisableCPUOutStandby()
        {
            cpuLimit?.Kill();
            cpuLimit?.Close();
            cpuLimit?.Dispose();
            cpuLimit = null;
            CPULimiterTools.IsStandByMode = true;
            if (Configuration.Instance.CPULimitInStandby > -1)
                EnableCPUStandby(Configuration.Instance.CPULimitOutStandby);
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

public class TickrateFreeze : MonoBehaviour
{
    private int _desiredTickrate = 60;
    private int _serverTickrate = 60;
    private int _sleepMs = 0;

    public int DesiredTickrate
    {
        get => _desiredTickrate;
        set
        {
            _desiredTickrate = value;
            RecalculateSleep();
        }
    }

    public int ServerTickrate
    {
        get => _serverTickrate;
        set
        {
            _serverTickrate = value;
            RecalculateSleep();
        }
    }

    private void RecalculateSleep()
    {
        _sleepMs = Mathf.Max(0, (1000 / _desiredTickrate) - (1000 / _serverTickrate));
        Logger.Log($"Sleep updated to: {_sleepMs}");
    }

    public void Update()
    {
        Thread.Sleep(_sleepMs);
    }
}