# Mod Concept

## General Rules
- **World Difficulty:** Difficulty-dependent achievement checks (such as undefeated enemies for `KillAllCreaturesHard` on Hard+ worlds) respect the current world's difficulty, while `Any`-difficulty achievements (crafting, cooking, eating, building, trophies) check `DifficultyRequirement.Any`.
- **Progress Scope (Config):** Configurable between `Character` (default) and `Global` without editing character (`.fch`) or world (`.db`) files:
  - `Character`: Tracks and displays progress strictly for the current character.
  - `Global`: Aggregates stats across all characters in memory (sum for accumulative counters and lists, max for record stats) and unlocks achievements when combined progress reaches 100%.
  - **Count Cheated Characters (Config):** Optional setting (default `false`) controlling whether cheated characters (`m_usedCheats`) are included in `Global` progress.
- **Sync Steam Unlocks (Config):** Separate setting (default `false`) that marks achievements already completed on Steam as unlocked in-game.
- **Feature Toggles (Config):** Individual mod features (tooltips, filters, enemy indicators, building icons, only hide spoiler achievements) are configurable via BepInEx config.

## 1. Enemies
- **Undefeated Indicator:** If a non-boss, non-tamed enemy belongs to `KillAllCreatures` / `KillAllCreaturesHard` and has never been killed, display a crossed-swords icon to the right of its health bar.
- **Missing Trophy Indicator:** If a non-boss, non-tamed enemy drops a trophy belonging to `FindAllTrophies` that has not been collected yet, display a trophy cup icon next to the health bar.

## 2. Achievements Tab
- **Only Hide Spoiler Achievements:** Keep boss-defeat and spoiler achievements (`hidden`) concealed, while showing non-spoiler concealed achievements (e.g., "Get killed by all types of trees").
- **Details Window Title:** Replace the generic "ACHIEVEMENT DETAILS" header with the actual achievement name.
- **Reveal / Hide Buttons:** By default, requirements are shown as `???`. Clicking a `???` row reveals it, and pressing **Reveal** in the corner of the frame reveals all conditions for that achievement. Whenever at least one condition is revealed (and Master Reveal All is off), a **Hide** button appears to re-hide that achievement. Revealed state is saved permanently.
- **Master Reveal All / Hide All Button:** Add a single master toggle button in the corner of the achievements screen to reveal all spoiler/hidden achievements at once (**Reveal All**, with a confirmation click), or reset all revealed achievements and conditions back to hidden (**Hide All**). State is saved permanently.
- **Always Show Counter Progress:** For milestone/counter achievements (such as "Chop 500 trees"), progress is always displayed.
- **Completion Percentage on Tiles:** Display completion percentage (`%`) directly on achievement tiles in the main achievements list for non-hidden, multi-step/counter achievements.
- **Biome Grouping in Details:** Group supported achievement detail lists by biome progression with `(completed / total)` per biome header and an `Uncategorized` group for unrecognized entries: enemies (`KillAllCreatures`, `KillAllCreaturesHard`) from `SpawnSystem` biomes, and craftable/food items (`AllItemCraft`, `AllWeaponCraft`, `AllFoodCooked`, `AllFoodEaten`) derived recursively from base materials, mob drops, cooking station conversions, and required station levels (designed generically for later reuse on pieces and trophies).
- **Display Types (`show:`):**
  - `all`: Shows every condition name and its full progress (`x / y`) immediately.
  - `revealable`: Shows the progress counter (`x / y`) while keeping uncompleted condition names hidden as `???` by default; pressing the **Reveal** button in the corner of the details window (or the Master Reveal All button) reveals all missing items/conditions.
  - `simple`: Visible on the main list, but clicking it does not open the details window (for straightforward one-off achievements).
  - `hidden`: Remains completely concealed (`Concealed` on the main list and `???` in details) until unlocked or revealed via the Master Reveal All button.

## 3. Crafting
- **Not-Crafted Filter:** Add a toggle button (`Not Crafted (N)`) in the crafting menu showing the count of missing achievement recipes for the current station. When enabled, it filters the list to only items not yet crafted for the achievement; it automatically resets to off when closing the inventory or switching to the Upgrade tab.
- **Tooltip Indicator:** On every item tooltip (in the corner or another suitable spot), show whether the item has not been crafted for the achievement yet.

## 4. Cooking & Eating
- **Not-Cooked Filter & Tooltip:** Cooking works similarly to crafting in Valheim, so it follows the same behavior as Crafting, using `Not Cooked (N)` instead of `Not Crafted (N)` (including unbaked recipes whose roasted/baked variant is not yet cooked).
- **Raw Cookable Items:** On raw cookable item tooltips, show if the cooked variant has not been cooked or eaten for the achievement yet.
- **Not-Eaten Indicator:** On every consumable item tooltip, show whether the item has not been eaten for the achievement (`AllFoodEaten`) yet.

## 5. Building
- **Unbuilt Piece Indicator:** Display a corner badge on building pieces in the build menu that belong to `AllBuildPieces` and have not been built yet, and append `Not built for achievement` to the piece description.
- **Not-Built Filter:** Add a `Not Built (N)` toggle button to the build menu (for piece tables with achievement targets, such as the Hammer) showing the count of missing pieces. When enabled, filters the build grid to only unbuilt achievement pieces, refreshes immediately after placing a piece, stays active across build menu closes while holding the tool, and resets when unequipping the tool.
