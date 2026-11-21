# AchievementHelper (Valheim Mod)

Quality-of-life mod for Valheim achievements, crafting/cooking/building tracking, and enemy indicators. See [concept.md](concept.md) and [achievements.md](achievements.md) for the full design.

## Setup

1. Install **BepInExPack_Valheim** into your Valheim directory.
2. Configure your local Valheim path using one of:
   - Copy `Directory.Build.props.example` to `Directory.Build.props` (gitignored) and set `<ValheimDir>`, **or**
   - Set the `VALHEIM_DIR` environment variable, **or**
   - Pass `-p:ValheimDir="C:\Path\To\Valheim"` to `dotnet build`.

## Build & Run

```powershell
dotnet restore
dotnet build -p:CopyToPlugins=true
```

Or run `run-valheim.cmd` to build, copy the plugin to `BepInEx\plugins\AchievementHelper\`, and launch Valheim via Steam.
