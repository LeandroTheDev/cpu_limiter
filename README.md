# CPULimiter
When launching the unturned server this plugin will find the PID of the unturned server process and will 
limit the cpu usage when standby or not, totally configurable.

This plugin will also change the tickrate of the server when disabled (this should reduce a lot of cpu process in idle).

This plugins does not support cpu limit on windows servers, consider leaving CPULimit at -1 if you are running on windows or linux without limitcpu command.

### Configurations

- SecondsFirstStandby: this is used to make a delay for limiting the cpu in initialization, if you limit the cpu too 
low on initialization the server will hang so much to open
- SecondsCheckNoPlayers: every this seconds the server will check for no players
- CPULimitInStandby: the percentage to limit (does not accept float numbers), the percentage is variable by the amount of threads in your CPU
for example a CPU with 8 threads the max limit is 800, leave -1 for no limitations
- CPULimitOutStandby: the percentage to limit when not in standby, leave it -1 for no limitations
- TickrateInStandby: Server tickrate when the server is idle (note that low tickrates will also make the time slow)
- TickrateOutStandby: Server tickrate when running with players
- ServerTickrate: Server tickrate from Rocket.config.xml (MaxFrames)
- GetCurrentlyProcess: This is the best option, the plugin will get the PID from the currently running proccess automatically
- ProcessName: Less recommended, the plugin will find all process on your system with this process name, and will limit them
- ProcessPath: Full process path, CPULimiter will limit this exactly process, if you run 2 servers at once in the same dedicated server this will not work properly, the best option is GetCurrentlyProcess, use this only in case that GetCurrentlyProcess is not working for some reason
- DebugProcess: With this you can check what process the CPULimiter can find into your currently user

### Requirements

- limitcpu (linux)

# Issues

Normally when forcing the cpu limiter below the used in unturned you will get the error:
```
src/steamnetworkingsockets/clientlib/steamnetworkingsockets_lowlevel.cpp (2123) : 
Assertion Failed: SteamnetworkingSockets service thread waited 100ms for lock!  
This directly adds to network latency!  It could be a bug, but it's usually caused by 
general performance problem such as thread starvation or a debug output handler taking too long.
```
This will cause the server to have a high ping on server view, because the thread that connects to the steam api is embed up within the unturned server process,
unfortunately theres is not i can do to fix that, unless the unturned developer makes a better idle system or divide the unturned server process and steam connection process, to prevent that disable the cpulimit config and use the tickrate only

Setting the cpu too low can make players struggling to connect, my recommendation is to set half the cpu used in idle

# Building

*Windows*: The project uses dotnet 4.8, consider installing into your machine, you need visual studio, 
simple open the solution file open the Build section and hit the build button (ctrl + shift + b) 
or you can do into powershell the command dotnet build -c Debug if you have installed dotnet 4.8.

*Linux*: Install dotnet-sdk from your distro package manager, open the root folder of this project and type ``dotnet build -c Debug``.

FTM License.
