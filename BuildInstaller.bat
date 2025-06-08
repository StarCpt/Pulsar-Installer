:: Prereq:
:: msbuild in PATH
:: dotnet CLI in PATH
:: .net framework 4.8 SDK
:: .net 9 SDK

:: Clean /Assets directory
rmdir /S PluginLoaderInstaller\Assets\

:: Build Pulsar
msbuild restore -p:RuntimeIdentifier=win Pulsar\PluginLoader\PluginLoader.csproj
msbuild -t:Build -p:OutputPath=..\..\PluginLoaderInstaller\Assets,Configuration=Release,Platform=x64,TargetFrameworkVersion=v4.8 Pulsar\PluginLoader\PluginLoader.csproj

:: Build SpaceEngineersLauncher
msbuild restore -p:RuntimeIdentifier=win SpaceEngineersLauncher\SpaceEngineersLauncher\SpaceEngineersLauncher.csproj
msbuild -t:Build -p:OutputPath=..\..\PluginLoaderInstaller\Assets,Configuration=Release,Platform=x64,TargetFrameworkVersion=v4.8,PostBuildEvent="" SpaceEngineersLauncher\SpaceEngineersLauncher\SpaceEngineersLauncher.csproj

:: Remove unneeded files
del PluginLoaderInstaller\Assets\*.pdb
del PluginLoaderInstaller\Assets\SpaceEngineersLauncher.exe.config

:: Create ZIP archive with proper paths
mkdir PluginLoaderInstaller\Assets\Plugins\Libraries

move PluginLoaderInstaller\Assets\Pulsar.dll PluginLoaderInstaller\Assets\Plugins\loader.dll
move PluginLoaderInstaller\Assets\*.dll PluginLoaderInstaller\Assets\Plugins\Libraries\

powershell Compress-Archive .\PluginLoaderInstaller\Assets\* .\PluginLoaderInstaller\Assets\pluginloader.zip

:: Build Pulsar-Installer
dotnet publish PluginLoaderInstaller\PulsarInstaller.csproj -c Release -r win-x64 --sc true -p:DebugSymbols=false,PublishDir="bin/publish/",PublishSingleFile=true