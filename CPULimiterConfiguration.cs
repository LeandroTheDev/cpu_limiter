using Rocket.API;

namespace CPULimiter
{
    public class CPULimiterConfiguration : IRocketPluginConfiguration
    {
        public int CPULimitInStandby = 100;
        public int CPULimitOutStandby = -1;
        public int SecondsCheckNoPlayers = 60;
        public int SecondsFirstStandby = 20;
        public bool GetCurrentlyProcess = true;
        public string? ProcessName = "Unturned_Headless.x86_64";
        public string? ProcessPath = "C:\\SteamLibrary\\steamapps\\common\\U3DS\\Unturned_Headless.x86_64";
        public bool DebugProcess = false;
        public void LoadDefaults()
        {

        }
    }
}
