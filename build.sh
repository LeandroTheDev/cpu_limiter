#!/bin/sh
game_dir=".../SteamLibrary/steamapps/common/U3DS/Servers/playtoearn2/Rocket/Plugins/"
library_dir=".../SteamLibrary/steamapps/common/U3DS/Servers/playtoearn2/Rocket/Libraries/"

dotnet build -c Release
cp -r ./bin/Release/net4.8/CPULimiter.dll "$game_dir"