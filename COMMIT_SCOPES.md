# Conventional Commit Scopes

Format: `<type>(<scope>): <short description>` (scope is optional)

## Types
| Type       | Usage                                             |
| :--------- | :------------------------------------------------ |
| `feat`     | New feature or mod functionality                  |
| `fix`      | Bug fix or UI/behavior correction                 |
| `refactor` | Code restructuring without behavior change        |
| `perf`     | Performance improvement                           |
| `docs`     | Documentation updates only                        |
| `chore`    | Maintenance, build scripts, or config adjustments |

## Scopes
| Scope          | Area                                                                        |
| :------------- | :-------------------------------------------------------------------------- |
| `achievements` | Achievements tab UI, display modes, `%` badges, Reveal / Master Reveal All  |
| `enemies`      | Enemy HUD indicators (undefeated icon, missing trophy icon)                 |
| `crafting`     | Crafting menu `not-crafted` filter and crafting tooltip indicators          |
| `cooking`      | Cooking `not-cooked` filter, `CookingStation` prompt, `not-eaten` indicator |
| `building`     | Building piece HUD built-status icons                                       |
| `tracking`     | `AchievementService`, difficulty stat checks, account-wide progress         |
| `config`       | BepInEx configuration entries and persistence                               |
| `build`        | `.csproj`, `Directory.Build.props.example`, `run-valheim.cmd`, tasks        |
