# CPULimiter for Linux
When launching the unturned server this plugin will find the PID of the unturned server process and will 
limit the cpu usage when standby or not totally configurable.

This plugins does not support windows servers.

### Configurations
- SecondsFirstStandby: this is used to make a delay for limiting the cpu in initialization, if you limit the cpu too 
low on initialization the server will hang so much to open
- SecondsCheckNoPlayers: every this seconds the server will check for no players
- CPULimitInStandby: the percentage to limit (does not accept float numbers), the percentage is variable by the amount of threads in your CPU
for example a CPU with 8 threads the max limit is 800, leave 0 for no limitations
- CPULimitOutStandby: the percentage to limit when not in standby, leave it 0 for no limitations
- ProcessName: Simple process name for unturned server, if for some reason your server has a different process 
name ensure its unique, or leave blank if you want the full path, only use this if you have a unique server in the 
same user if you have more please us the ProcessPath instead and live this empty ``<ProcessName />``
- ProcessPath: Full process path, CPULimiter will limit this exactly process
- DebugProcess: With this you can check what process the CPULimiter can find into your currently user

### Requirements:
- cpulimit (linux)

# Issues
Normally when forcing the cpu limiter below the used in unturned you will get the error:
```
src/steamnetworkingsockets/clientlib/steamnetworkingsockets_lowlevel.cpp (2123) : 
Assertion Failed: SteamnetworkingSockets service thread waited 100ms for lock!  
This directly adds to network latency!  It could be a bug, but it's usually caused by 
general performance problem such as thread starvation or a debug output handler taking too long.
```
This is completly unecessary and you can ignore that



# Building

*Windows*: The project uses dotnet 4.8, consider installing into your machine, you need visual studio, 
simple open the solution file open the Build section and hit the build button (ctrl + shift + b) 
or you can do into powershell the command dotnet build -c Debug if you have installed dotnet 4.8.

*Linux*: Unfortunately versions lower than 6 of dotnet do not have support for linux, 
the best thing you can do is install dotnet 6 or the lowest possible version on your distro and try to compile in dotnet 6
using the command dotnet build -c Debug, this can cause problems within rocket loader.

FTM License.
