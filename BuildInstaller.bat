:: Prereq:
:: dotnet CLI in PATH
:: .net 9 SDK

:: Build Pulsar-Installer
dotnet publish PluginLoaderInstaller\PulsarInstaller.csproj -c Release -r win-x64 --sc true -p:DebugSymbols=false,PublishDir="bin/publish/",PublishSingleFile=true