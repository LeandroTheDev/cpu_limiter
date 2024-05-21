using Rocket.API;

namespace CPULimiter
{
    public class CPULimiterConfiguration : IRocketPluginConfiguration
    {
        public int CPULimitInStandby = 5;
        public int CPULimitOutStandby = 0;
        public int SecondsCheckNoPlayers = 60;
        public int SecondsFirstStandby = 20;
        public string? ProcessName = "Unturned_Headless.x86_64";
        public string? ProcessPath = "C:\\SteamLibrary\\steamapps\\common\\U3DS\\Unturned_Headless.x86_64";
        public bool DebugProcess = false;
        public void LoadDefaults()
        {

        }
    }
}
