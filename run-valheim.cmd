@echo off
setlocal
dotnet build "%~dp0AchievementHelper.csproj" --no-restore -p:CopyToPlugins=true
if errorlevel 1 (
    echo Build failed!
    pause
    exit /b 1
)
echo Launching Valheim via Steam...
start "" "steam://rungameid/892970"
