# Mod Concept

## General Rules
- **World Difficulty:** Achievement status checks (undefeated enemies, missing items, pieces, etc.) respect the current world's difficulty setting.

## 1. Enemies
- **Undefeated Indicator:** If an enemy has never been killed, display an icon next to its stars (or where stars would appear) or on the opposite side of the health bar indicating that it has not been defeated yet.
- **Missing Trophy Indicator:** If an enemy has not dropped its trophy yet, display a trophy icon.

## 2. Achievements Tab
- **Disable Hiding for Non-Boss Achievements:** Add an option to disable hiding for non-boss achievements. Boss-defeat achievements (e.g., "Kill Boss X") remain hidden, while achievements such as "Get killed by all types of trees" should be visible.
- **Details Window Title:** Replace the generic "ACHIEVEMENT DETAILS" header with the actual achievement name.
- **Reveal / Hide Buttons:** By default, requirements are shown as `???`. Clicking a `???` row reveals it, and pressing **Reveal** in the corner of the frame reveals all conditions for that achievement. Whenever at least one condition is revealed (and Master Reveal All is off), a **Hide** button appears to re-hide that achievement. Revealed state is saved permanently.
- **Master Reveal All / Hide All Button:** Add a single master toggle button in the corner of the achievements screen to reveal all spoiler/hidden achievements at once (**Reveal All**, with a confirmation click), or reset all revealed achievements and conditions back to hidden (**Hide All**). State is saved permanently.
- **Always Show Counter Progress:** For milestone/counter achievements (such as "Chop 500 trees"), progress is always displayed.
- **Completion Percentage on Tiles:** Display completion percentage (`%`) directly on achievement tiles in the main achievements list for non-hidden, multi-step/counter achievements.
- **Display Types (`show:`):**
  - `all`: Shows every condition name and its full progress (`x / y`) immediately.
  - `revealable`: Shows the progress counter (`x / y`) while keeping uncompleted condition names hidden as `???` by default; pressing the **Reveal** button in the corner of the details window (or the Master Reveal All button) reveals all missing items/conditions.
  - `simple`: Visible on the main list, but clicking it does not open the details window (for straightforward one-off achievements).
  - `hidden`: Remains completely concealed (`Concealed` on the main list and `???` in details) until unlocked or revealed via the Master Reveal All button.

## 3. Crafting
- **Not-Crafted Filter:** Add an extra filter (`not-crafted`) to the crafting menu so players can toggle it to see only items that have not yet been crafted for the achievement.
- **Tooltip Indicator:** On every item tooltip (in the corner or another suitable spot), show whether the item has not been crafted for the achievement yet.

## 4. Cooking & Eating
- **Not-Cooked Filter & Tooltip:** Cooking works similarly to crafting in Valheim, so it should follow the exact same behavior as Crafting, using `not-cooked` instead of `not-crafted`.
- **Cooking Station (Roasted Items):** On cooking/roasting stations (`CookingStation`), show in the hover prompt if the resulting food has not been cooked for the achievement yet.
- **Not-Eaten Indicator:** On every consumable item tooltip, show whether the item has not been eaten for the achievement (`AllFoodEaten`) yet.

## 5. Building
- **Built Status Icon:** Display an icon in the corner of each building piece (or another appropriate place) indicating whether it has been built.
