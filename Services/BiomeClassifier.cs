using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AchievementHelper.Services;

public static class BiomeClassifier
{
    // Manual enemy overrides by Character.m_name (bosses, minibosses, dungeon/location-only enemies, summons, and biome overrides)
    private static readonly Dictionary<string, BiomeTier> EnemyBiomeOverrides =
        new(StringComparer.Ordinal)
        {
            // Meadows
            ["$enemy_eikthyr"] = BiomeTier.Meadows,
            ["$enemy_boarpiggy"] = BiomeTier.Meadows,
            ["$piece_trainingdummy"] = BiomeTier.Meadows,

            // Black Forest
            ["$enemy_gdking"] = BiomeTier.BlackForest,
            ["$enemy_skeletonfire"] = BiomeTier.BlackForest, // Brenna
            ["$enemy_ghost"] = BiomeTier.BlackForest,
            ["$enemy_skeleton_poison"] = BiomeTier.BlackForest,

            // Swamp
            ["$enemy_bonemass"] = BiomeTier.Swamp,
            ["$enemy_draugr"] = BiomeTier.Swamp, // Override rare Meadows Draugr Village spawns
            ["$enemy_draugrelite"] = BiomeTier.Swamp,
            ["$enemy_surtling"] = BiomeTier.Swamp,
            ["$enemy_kvastur"] = BiomeTier.Swamp,

            // Mountain
            ["$enemy_dragon"] = BiomeTier.Mountain, // Moder
            ["$enemy_fenringcultist_hildir"] = BiomeTier.Mountain, // Geirrhafa
            ["$enemy_bat"] = BiomeTier.Mountain,
            ["$enemy_ulv"] = BiomeTier.Mountain,
            ["$enemy_fenringcultist"] = BiomeTier.Mountain,
            ["$enemy_wolfcub"] = BiomeTier.Mountain,

            // Plains
            ["$enemy_goblinking"] = BiomeTier.Plains, // Yagluth
            ["$enemy_goblinbrute_hildircombined"] = BiomeTier.Plains, // Zil & Thungr
            ["$enemy_goblin_hildir"] = BiomeTier.Plains, // Zil
            ["$enemy_goblinshaman"] = BiomeTier.Plains,
            ["$enemy_blobtar"] = BiomeTier.Plains, // Growth
            ["$enemy_loxcalf"] = BiomeTier.Plains,
            ["$enemy_chicken"] = BiomeTier.Plains,
            ["$enemy_hen"] = BiomeTier.Plains,

            // Mistlands
            ["$enemy_seekerqueen"] = BiomeTier.Mistlands,
            ["$enemy_tick"] = BiomeTier.Mistlands,
            ["$enemy_seeker_brood"] = BiomeTier.Mistlands,
            ["$enemy_dverger"] = BiomeTier.Mistlands,
            ["$enemy_dvergr_mage"] = BiomeTier.Mistlands,
            ["$enemy_skeleton_summoned"] = BiomeTier.Mistlands, // Dead Raiser Skelett

            // Ashlands
            ["$enemy_fader"] = BiomeTier.Ashlands,
            ["$enemy_charred_melee_Dyrnwyn"] = BiomeTier.Ashlands, // Lord Reto
            ["$enemy_morgen"] = BiomeTier.Ashlands,
            ["$enemy_charred_mage"] = BiomeTier.Ashlands,
            ["$enemy_asksvin_hatchling"] = BiomeTier.Ashlands,
            ["$enemy_charred_melee_Fader"] = BiomeTier.Ashlands,
            ["$enemy_charred_twitcher_summoned"] = BiomeTier.Ashlands,
            ["$enemy_summonedtroll"] = BiomeTier.Ashlands,

            // Deep North
            ["$enemy_frozenking"] = BiomeTier.DeepNorth, // Kall Fimbulbringer
            ["$enemy_frozenking_p3"] = BiomeTier.DeepNorth,
            ["$enemy_aspect_eikthyr"] = BiomeTier.DeepNorth,
            ["$enemy_aspect_gdking"] = BiomeTier.DeepNorth,
            ["$enemy_aspect_bonemass"] = BiomeTier.DeepNorth,
            ["$enemy_aspect_dragon"] = BiomeTier.DeepNorth,
            ["$enemy_aspect_goblinking"] = BiomeTier.DeepNorth,
            ["$enemy_aspect_seekerqueen"] = BiomeTier.DeepNorth,
            ["$enemy_aspect_fader"] = BiomeTier.DeepNorth,
            ["$enemy_trollfrost"] = BiomeTier.DeepNorth, // Gammeltroll
            ["$enemy_jotun_warrior"] = BiomeTier.DeepNorth, // Krigen
            ["$enemy_jotun_witch"] = BiomeTier.DeepNorth, // Hexen
            ["$enemy_elakingmole"] = BiomeTier.DeepNorth, // Eyeless One
            ["$enemy_fallenwarrior"] = BiomeTier.DeepNorth,
            ["$enemy_blobfrost"] = BiomeTier.DeepNorth,
            ["$enemy_blobmork"] = BiomeTier.DeepNorth, // Shapeless Pulp
            ["$enemy_blobmorkmini"] = BiomeTier.DeepNorth, // Tiny Pulp
            ["$enemy_goblin_deepnorth"] = BiomeTier.DeepNorth, // Captive Fuling
            ["$enemy_dvergr_deepnorth"] = BiomeTier.DeepNorth, // Imprisoned Dvergr
            ["$enemy_moosecalf"] = BiomeTier.DeepNorth,
            ["$spiritcaller_boar"] = BiomeTier.DeepNorth,
            ["$spiritcaller_bjorn"] = BiomeTier.DeepNorth,
            ["$spiritcaller_wolf"] = BiomeTier.DeepNorth,
            ["$spiritcaller_moose"] = BiomeTier.DeepNorth,
        };

    // Base materials mapped by prefab name (case-insensitive; take precedence over mob-derived drops)
    private static readonly Dictionary<string, BiomeTier> BaseMaterialBiomes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // 1. Meadows
            ["Wood"] = BiomeTier.Meadows,
            ["FineWood"] = BiomeTier.Meadows,
            ["Stone"] = BiomeTier.Meadows,
            ["Flint"] = BiomeTier.Meadows,
            ["Resin"] = BiomeTier.Meadows,
            ["Feathers"] = BiomeTier.Meadows,
            ["HardAntler"] = BiomeTier.Meadows,
            ["LeatherScraps"] = BiomeTier.Meadows,
            ["DeerHide"] = BiomeTier.Meadows,
            ["RawMeat"] = BiomeTier.Meadows,
            ["NeckTail"] = BiomeTier.Meadows,
            ["Raspberry"] = BiomeTier.Meadows,
            ["Mushroom"] = BiomeTier.Meadows,
            ["Dandelion"] = BiomeTier.Meadows,
            ["Honey"] = BiomeTier.Meadows,
            ["QueenBee"] = BiomeTier.Meadows,
            ["BeechSeeds"] = BiomeTier.Meadows,
            ["BirchSeeds"] = BiomeTier.Meadows,
            ["Acorn"] = BiomeTier.Meadows,
            ["AxeHead1"] = BiomeTier.Meadows,
            ["AxeHead2"] = BiomeTier.Meadows,
            ["CookedMeat"] = BiomeTier.Meadows,
            ["CookedDeerMeat"] = BiomeTier.Meadows,
            ["NeckTailGrilled"] = BiomeTier.Meadows,
            ["FishCooked"] = BiomeTier.Meadows,
            ["Fish1"] = BiomeTier.Meadows, // Perch
            ["Fish2"] = BiomeTier.Meadows, // Pike

            // 2. Black Forest
            ["RoundLog"] = BiomeTier.BlackForest,
            ["CopperOre"] = BiomeTier.BlackForest,
            ["Copper"] = BiomeTier.BlackForest,
            ["CopperScrap"] = BiomeTier.BlackForest,
            ["TinOre"] = BiomeTier.BlackForest,
            ["Tin"] = BiomeTier.BlackForest,
            ["Bronze"] = BiomeTier.BlackForest,
            ["BronzeNails"] = BiomeTier.BlackForest,
            ["Coal"] = BiomeTier.BlackForest,
            ["SurtlingCore"] = BiomeTier.BlackForest,
            ["BoneFragments"] = BiomeTier.BlackForest,
            ["GreydwarfEye"] = BiomeTier.BlackForest,
            ["TrollHide"] = BiomeTier.BlackForest,
            ["BjornHide"] = BiomeTier.BlackForest,
            ["BearHide"] = BiomeTier.BlackForest,
            ["CookedBjornMeat"] = BiomeTier.BlackForest,
            ["Blueberries"] = BiomeTier.BlackForest,
            ["Thistle"] = BiomeTier.BlackForest,
            ["MushroomYellow"] = BiomeTier.BlackForest,
            ["Carrot"] = BiomeTier.BlackForest,
            ["CarrotSeeds"] = BiomeTier.BlackForest,
            ["PineCone"] = BiomeTier.BlackForest,
            ["FirCone"] = BiomeTier.BlackForest,
            ["CryptKey"] = BiomeTier.BlackForest,
            ["YmirRemains"] = BiomeTier.BlackForest,
            ["Thunderstone"] = BiomeTier.BlackForest,
            ["FishingBait"] = BiomeTier.BlackForest,
            ["FireworksRocket_White"] = BiomeTier.BlackForest,
            ["Coins"] = BiomeTier.BlackForest,
            ["Amber"] = BiomeTier.BlackForest,
            ["AmberPearl"] = BiomeTier.BlackForest,
            ["Ruby"] = BiomeTier.BlackForest,
            ["Fish5"] = BiomeTier.BlackForest, // Trollfish

            // 3. Ocean
            ["Chitin"] = BiomeTier.Ocean,
            ["SerpentScale"] = BiomeTier.Ocean,
            ["SerpentMeat"] = BiomeTier.Ocean,
            ["SerpentMeatCooked"] = BiomeTier.Ocean,
            ["Fish3"] = BiomeTier.Ocean, // Tuna
            ["Fish8"] = BiomeTier.Ocean, // Coral cod
            ["Fish12"] = BiomeTier.Ocean, // Pufferfish

            // 4. Swamp
            ["IronScrap"] = BiomeTier.Swamp,
            ["Iron"] = BiomeTier.Swamp,
            ["IronNails"] = BiomeTier.Swamp,
            ["ElderBark"] = BiomeTier.Swamp,
            ["WitheredBone"] = BiomeTier.Swamp,
            ["Guck"] = BiomeTier.Swamp,
            ["Chain"] = BiomeTier.Swamp,
            ["Root"] = BiomeTier.Swamp,
            ["Entrails"] = BiomeTier.Swamp,
            ["Ooze"] = BiomeTier.Swamp,
            ["Bloodbag"] = BiomeTier.Swamp,
            ["Turnip"] = BiomeTier.Swamp,
            ["TurnipSeeds"] = BiomeTier.Swamp,
            ["Wishbone"] = BiomeTier.Swamp,
            ["ScytheHandle"] = BiomeTier.Swamp,
            ["BlobVial"] = BiomeTier.Swamp,
            ["PungentPebbles"] = BiomeTier.Swamp,
            ["FragrantBundle"] = BiomeTier.Swamp,
            ["CuredSquirrelHamstring"] = BiomeTier.Swamp,
            ["PowderedDragonEgg"] = BiomeTier.Swamp,
            ["FreshSeaweed"] = BiomeTier.Swamp,
            ["Toadstool"] = BiomeTier.Swamp,
            ["MushroomBzerker"] = BiomeTier.Swamp,
            ["SpiceForests"] = BiomeTier.Swamp,
            ["SpiceOceans"] = BiomeTier.Swamp,
            ["HelmetRootCrown"] = BiomeTier.Swamp,
            ["Fish6"] = BiomeTier.Swamp, // Giant herring

            // 5. Mountain
            ["SilverOre"] = BiomeTier.Mountain,
            ["Silver"] = BiomeTier.Mountain,
            ["Obsidian"] = BiomeTier.Mountain,
            ["Crystal"] = BiomeTier.Mountain,
            ["FreezeGland"] = BiomeTier.Mountain,
            ["WolfPelt"] = BiomeTier.Mountain,
            ["WolfFang"] = BiomeTier.Mountain,
            ["WolfMeat"] = BiomeTier.Mountain,
            ["CookedWolfMeat"] = BiomeTier.Mountain,
            ["WolfHairBundle"] = BiomeTier.Mountain,
            ["WolfClaw"] = BiomeTier.Mountain,
            ["FenrisHair"] = BiomeTier.Mountain,
            ["FenrisClaw"] = BiomeTier.Mountain,
            ["JuteRed"] = BiomeTier.Mountain,
            ["Onion"] = BiomeTier.Mountain,
            ["OnionSeeds"] = BiomeTier.Mountain,
            ["DragonEgg"] = BiomeTier.Mountain,
            ["DragonTear"] = BiomeTier.Mountain,
            ["SpiceMountains"] = BiomeTier.Mountain,
            ["Fish4_cave"] = BiomeTier.Mountain, // Tetra

            // 6. Plains
            ["BlackMetalScrap"] = BiomeTier.Plains,
            ["BlackMetal"] = BiomeTier.Plains,
            ["Tar"] = BiomeTier.Plains,
            ["Needle"] = BiomeTier.Plains,
            ["LoxPelt"] = BiomeTier.Plains,
            ["LoxMeat"] = BiomeTier.Plains,
            ["CookedLoxMeat"] = BiomeTier.Plains,
            ["Flax"] = BiomeTier.Plains,
            ["LinenThread"] = BiomeTier.Plains,
            ["Barley"] = BiomeTier.Plains,
            ["BarleyFlour"] = BiomeTier.Plains,
            ["Bread"] = BiomeTier.Plains,
            ["LoxPie"] = BiomeTier.Plains,
            ["SpicePlains"] = BiomeTier.Plains,
            ["Cloudberry"] = BiomeTier.Plains,
            ["GoblinTotem"] = BiomeTier.Plains,
            ["YagluthDrop"] = BiomeTier.Plains,
            ["ChickenEgg"] = BiomeTier.Plains,
            ["ChickenMeat"] = BiomeTier.Plains,
            ["CookedChickenMeat"] = BiomeTier.Plains,
            ["Fish7"] = BiomeTier.Plains, // Grouper

            // 7. Mistlands
            ["YggdrasilWood"] = BiomeTier.Mistlands,
            ["BlackMarble"] = BiomeTier.Mistlands,
            ["Softtissue"] = BiomeTier.Mistlands,
            ["Sap"] = BiomeTier.Mistlands,
            ["Eitr"] = BiomeTier.Mistlands,
            ["Wisp"] = BiomeTier.Mistlands,
            ["BlackCore"] = BiomeTier.Mistlands,
            ["Carapace"] = BiomeTier.Mistlands,
            ["Mandible"] = BiomeTier.Mistlands,
            ["ScaleHide"] = BiomeTier.Mistlands,
            ["BloodClot"] = BiomeTier.Mistlands,
            ["Bilebag"] = BiomeTier.Mistlands,
            ["RoyalJelly"] = BiomeTier.Mistlands,
            ["JuteBlue"] = BiomeTier.Mistlands,
            ["Hook"] = BiomeTier.Mistlands,
            ["DvergrNeedle"] = BiomeTier.Mistlands,
            ["DvergrKeyFragment"] = BiomeTier.Mistlands,
            ["DvergrKey"] = BiomeTier.Mistlands,
            ["MechanicalSpring"] = BiomeTier.Mistlands,
            ["Lantern"] = BiomeTier.Mistlands,
            ["QueenDrop"] = BiomeTier.Mistlands,
            ["BugMeat"] = BiomeTier.Mistlands,
            ["CookedBugMeat"] = BiomeTier.Mistlands,
            ["HareMeat"] = BiomeTier.Mistlands,
            ["CookedHareMeat"] = BiomeTier.Mistlands,
            ["MushroomJotunPuffs"] = BiomeTier.Mistlands,
            ["MushroomMagecap"] = BiomeTier.Mistlands,
            ["MisthareSupreme"] = BiomeTier.Mistlands,
            ["SpiceMistlands"] = BiomeTier.Mistlands,
            ["Fish9"] = BiomeTier.Mistlands, // Anglerfish

            // 8. Ashlands
            ["Blackwood"] = BiomeTier.Ashlands,
            ["Grausten"] = BiomeTier.Ashlands,
            ["FlametalOreNew"] = BiomeTier.Ashlands,
            ["FlametalNew"] = BiomeTier.Ashlands,
            ["FlametalOre"] = BiomeTier.Ashlands,
            ["Flametal"] = BiomeTier.Ashlands,
            ["MoltenCore"] = BiomeTier.Ashlands,
            ["BellFragment"] = BiomeTier.Ashlands,
            ["Bell"] = BiomeTier.Ashlands,
            ["ShieldCore"] = BiomeTier.Ashlands,
            ["FaderDrop"] = BiomeTier.Ashlands,
            ["FaderEmber"] = BiomeTier.Ashlands,
            ["GemstoneRed"] = BiomeTier.Ashlands,
            ["GemstoneBlue"] = BiomeTier.Ashlands,
            ["GemstoneGreen"] = BiomeTier.Ashlands,
            ["SulfurStone"] = BiomeTier.Ashlands,
            ["Sulfur"] = BiomeTier.Ashlands,
            ["CharcoalResin"] = BiomeTier.Ashlands,
            ["ProustitePowder"] = BiomeTier.Ashlands,
            ["CharredBone"] = BiomeTier.Ashlands,
            ["CharredCogwheel"] = BiomeTier.Ashlands,
            ["AskHide"] = BiomeTier.Ashlands,
            ["AskBladder"] = BiomeTier.Ashlands,
            ["AsksvinNeck"] = BiomeTier.Ashlands,
            ["AsksvinPelvis"] = BiomeTier.Ashlands,
            ["AsksvinRibcage"] = BiomeTier.Ashlands,
            ["AsksvinSkull"] = BiomeTier.Ashlands,
            ["MorgenHeart"] = BiomeTier.Ashlands,
            ["MorgenSinew"] = BiomeTier.Ashlands,
            ["BonemawSerpentTooth"] = BiomeTier.Ashlands,
            ["BonemawTooth"] = BiomeTier.Ashlands,
            ["CelestialFeather"] = BiomeTier.Ashlands,
            ["Fiddlehead"] = BiomeTier.Ashlands,
            ["Fiddleheadfern"] = BiomeTier.Ashlands,
            ["SmokePuff"] = BiomeTier.Ashlands,
            ["MushroomSmokePuff"] = BiomeTier.Ashlands,
            ["Vineberry"] = BiomeTier.Ashlands,
            ["VineberryCluster"] = BiomeTier.Ashlands,
            ["VineberrySeeds"] = BiomeTier.Ashlands,
            ["VoltureMeat"] = BiomeTier.Ashlands,
            ["CookedVoltureMeat"] = BiomeTier.Ashlands,
            ["AsksvinMeat"] = BiomeTier.Ashlands,
            ["CookedAsksvinMeat"] = BiomeTier.Ashlands,
            ["BonemawSerpentMeat"] = BiomeTier.Ashlands,
            ["BonemawSerpentMeatCooked"] = BiomeTier.Ashlands,
            ["DyrnwynBladeFragment"] = BiomeTier.Ashlands,
            ["DyrnwynHiltFragment"] = BiomeTier.Ashlands,
            ["DyrnwynTipFragment"] = BiomeTier.Ashlands,
            ["SpiceAshlands"] = BiomeTier.Ashlands,
            ["Fish11"] = BiomeTier.Ashlands, // Magmafish

            // 9. Deep North
            ["Frostwood"] = BiomeTier.DeepNorth,
            ["Ice"] = BiomeTier.DeepNorth,
            ["LiquidFrost"] = BiomeTier.DeepNorth,
            ["FrostCore"] = BiomeTier.DeepNorth,
            ["GoldOre"] = BiomeTier.DeepNorth,
            ["Gold"] = BiomeTier.DeepNorth,
            ["NornThread"] = BiomeTier.DeepNorth,
            ["OrbFrostFire"] = BiomeTier.DeepNorth,
            ["OrbThunderBlood"] = BiomeTier.DeepNorth,
            ["AncientGemstoneOrange"] = BiomeTier.DeepNorth,
            ["AncientGemstoneGreen"] = BiomeTier.DeepNorth,
            ["AncientGemstonePurple"] = BiomeTier.DeepNorth,
            ["AncientGemstoneBlack"] = BiomeTier.DeepNorth,
            ["Lingonberry"] = BiomeTier.DeepNorth,
            ["Kale"] = BiomeTier.DeepNorth,
            ["KaleSeeds"] = BiomeTier.DeepNorth,
            ["Poteitr"] = BiomeTier.DeepNorth,
            ["Oat"] = BiomeTier.DeepNorth,
            ["OatSeeds"] = BiomeTier.DeepNorth,
            ["OatFlour"] = BiomeTier.DeepNorth,
            ["Snowball"] = BiomeTier.DeepNorth,
            ["SealBlubber"] = BiomeTier.DeepNorth,
            ["MooseHide"] = BiomeTier.DeepNorth,
            ["MooseMeat"] = BiomeTier.DeepNorth,
            ["CookedMooseMeat"] = BiomeTier.DeepNorth,
            ["SpiceDeepNorth"] = BiomeTier.DeepNorth,
            ["Fish10"] = BiomeTier.DeepNorth, // Northern salmon
            // Smelted Nord weapons, shields, armor, and staffs (cast in Frost Foundry, no standard Recipe)
            ["AtgeirGold"] = BiomeTier.DeepNorth,
            ["AxeGold"] = BiomeTier.DeepNorth,
            ["BattleaxeGold"] = BiomeTier.DeepNorth,
            ["BowGold"] = BiomeTier.DeepNorth,
            ["CrossbowGold"] = BiomeTier.DeepNorth,
            ["FistGold"] = BiomeTier.DeepNorth,
            ["KnifeGold"] = BiomeTier.DeepNorth,
            ["MaceGold"] = BiomeTier.DeepNorth,
            ["SledgeGold"] = BiomeTier.DeepNorth,
            ["SpearGold"] = BiomeTier.DeepNorth,
            ["SwordGold"] = BiomeTier.DeepNorth,
            ["THSwordGold"] = BiomeTier.DeepNorth,
            ["ShieldGold"] = BiomeTier.DeepNorth,
            ["ShieldGoldBuckler"] = BiomeTier.DeepNorth,
            ["ShieldGoldTower"] = BiomeTier.DeepNorth,
            ["HelmetDNHeavy"] = BiomeTier.DeepNorth,
            ["ArmorDeepNorthHeavyChest"] = BiomeTier.DeepNorth,
            ["ArmorDeepNorthHeavylegs"] = BiomeTier.DeepNorth,
            ["HelmetDNMediumHood"] = BiomeTier.DeepNorth,
            ["ArmorDeepNorthMediumChest"] = BiomeTier.DeepNorth,
            ["ArmorDeepNorthMediumlegs"] = BiomeTier.DeepNorth,
            ["HelmetDNMage"] = BiomeTier.DeepNorth,
            ["ArmorDeepNorthMageChest"] = BiomeTier.DeepNorth,
            ["ArmorDeepNorthMagelegs"] = BiomeTier.DeepNorth,
            ["StaffFrostOrbs"] = BiomeTier.DeepNorth,
            ["StaffThunderBlood"] = BiomeTier.DeepNorth,
            ["StaffOrbofAhri"] = BiomeTier.DeepNorth,
            ["StaffSpiritCaller"] = BiomeTier.DeepNorth,
        };

    private static readonly Dictionary<string, BiomeTier> EnemyBiomeByName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, BiomeTier> DerivedMaterialBiomes = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Recipe> RecipeBySharedName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> PrefabNameBySharedName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, BiomeTier> ItemBiomeCache = new(StringComparer.Ordinal);
    private static readonly HashSet<string> LoggedSummaryKeys = new(StringComparer.OrdinalIgnoreCase);

    private static bool _enemyIndexBuilt;
    private static bool _itemIndexBuilt;

    public static void ResetCache()
    {
        EnemyBiomeByName.Clear();
        DerivedMaterialBiomes.Clear();
        RecipeBySharedName.Clear();
        PrefabNameBySharedName.Clear();
        ItemBiomeCache.Clear();
        LoggedSummaryKeys.Clear();
        _enemyIndexBuilt = false;
        _itemIndexBuilt = false;
    }

    public static BiomeTier GetEnemyBiome(string enemySharedName)
    {
        if (string.IsNullOrEmpty(enemySharedName))
        {
            return BiomeTier.Uncategorized;
        }

        if (EnemyBiomeOverrides.TryGetValue(enemySharedName, out BiomeTier overridden))
        {
            return overridden;
        }

        EnsureEnemyIndexBuilt();
        return EnemyBiomeByName.TryGetValue(enemySharedName, out BiomeTier tier)
            ? tier
            : BiomeTier.Uncategorized;
    }

    public static BiomeTier GetItemBiome(string sharedName)
    {
        if (string.IsNullOrEmpty(sharedName))
        {
            return BiomeTier.Uncategorized;
        }

        EnsureEnemyIndexBuilt();
        EnsureItemIndexBuilt();

        if (ItemBiomeCache.TryGetValue(sharedName, out BiomeTier cached))
        {
            return cached;
        }

        HashSet<string> visited = new(StringComparer.Ordinal);
        HashSet<string> leafPrefabs = new(StringComparer.OrdinalIgnoreCase);
        CollectBaseMaterials(sharedName, visited, leafPrefabs);

        BiomeTier result = EvaluateLeafMaterials(leafPrefabs);
        ItemBiomeCache[sharedName] = result;
        return result;
    }

    public static void LogUncategorizedEnemies(string logKey, IEnumerable<string> enemyNames)
    {
        if (!LoggedSummaryKeys.Add("enemies:" + logKey))
        {
            return;
        }

        EnsureEnemyIndexBuilt();

        List<string> uncategorized = new();
        foreach (string enemyName in enemyNames)
        {
            if (GetEnemyBiome(enemyName) == BiomeTier.Uncategorized)
            {
                string display = AchievementService.FormatStatName(enemyName);
                uncategorized.Add($"{display} ({enemyName})");
            }
        }

        if (uncategorized.Count == 0)
        {
            Plugin.Log.LogInfo($"[BiomeClassifier] {logKey}: all enemies categorized.");
            return;
        }

        uncategorized.Sort(StringComparer.OrdinalIgnoreCase);
        Plugin.Log.LogInfo(
            $"[BiomeClassifier] {logKey}: {uncategorized.Count} uncategorized enemies: {string.Join(", ", uncategorized)}");
    }

    public static void LogUncategorizedMaterialsForItems(string logKey, IEnumerable<string> sharedNames)
    {
        if (!LoggedSummaryKeys.Add("items:" + logKey))
        {
            return;
        }

        EnsureEnemyIndexBuilt();
        EnsureItemIndexBuilt();

        Dictionary<string, int> unknownCounts = new(StringComparer.OrdinalIgnoreCase);
        int uncategorizedItems = 0;

        foreach (string sharedName in sharedNames)
        {
            if (GetItemBiome(sharedName) != BiomeTier.Uncategorized)
            {
                continue;
            }

            uncategorizedItems++;
            HashSet<string> visited = new(StringComparer.Ordinal);
            HashSet<string> leafPrefabs = new(StringComparer.OrdinalIgnoreCase);
            CollectBaseMaterials(sharedName, visited, leafPrefabs);

            foreach (string leaf in leafPrefabs)
            {
                if (!TryGetKnownMaterialBiome(leaf, out _))
                {
                    unknownCounts.TryGetValue(leaf, out int c);
                    unknownCounts[leaf] = c + 1;
                }
            }
        }

        if (uncategorizedItems == 0)
        {
            Plugin.Log.LogInfo($"[BiomeClassifier] {logKey}: all craft items categorized.");
            return;
        }

        string topUnknown = string.Join(
            ", ",
            unknownCounts
                .OrderByDescending(kvp => kvp.Value)
                .ThenBy(kvp => kvp.Key)
                .Take(30)
                .Select(kvp => $"{kvp.Key}({kvp.Value})"));

        Plugin.Log.LogInfo(
            $"[BiomeClassifier] {logKey}: {uncategorizedItems} uncategorized craft items. Top unknown base materials: {topUnknown}");
    }

    private static bool TryGetKnownMaterialBiome(string prefabName, out BiomeTier tier)
    {
        if (BaseMaterialBiomes.TryGetValue(prefabName, out tier))
        {
            return true;
        }

        if (prefabName.StartsWith("Mold", StringComparison.OrdinalIgnoreCase))
        {
            tier = BiomeTier.DeepNorth;
            return true;
        }

        return DerivedMaterialBiomes.TryGetValue(prefabName, out tier);
    }

    private static BiomeTier EvaluateLeafMaterials(HashSet<string> leafPrefabs)
    {
        if (leafPrefabs.Count == 0)
        {
            return BiomeTier.Uncategorized;
        }

        BiomeTier highest = BiomeTier.Meadows;
        bool anyKnown = false;

        foreach (string prefab in leafPrefabs)
        {
            if (!TryGetKnownMaterialBiome(prefab, out BiomeTier tier))
            {
                return BiomeTier.Uncategorized;
            }

            anyKnown = true;
            if (tier > highest)
            {
                highest = tier;
            }
        }

        return anyKnown ? highest : BiomeTier.Uncategorized;
    }

    private static void CollectBaseMaterials(
        string sharedName,
        HashSet<string> visited,
        HashSet<string> leafPrefabs)
    {
        if (string.IsNullOrEmpty(sharedName) || !visited.Add(sharedName))
        {
            return;
        }

        if (PrefabNameBySharedName.TryGetValue(sharedName, out string prefabName) &&
            TryGetKnownMaterialBiome(prefabName, out _))
        {
            leafPrefabs.Add(prefabName);
            return;
        }

        if (RecipeBySharedName.TryGetValue(sharedName, out Recipe recipe) &&
            recipe.m_resources != null &&
            recipe.m_resources.Length > 0)
        {
            bool addedAnyIngredient = false;
            foreach (Piece.Requirement req in recipe.m_resources)
            {
                if (req == null || req.m_resItem == null || req.m_amount <= 0 || req.m_upgraderResource)
                {
                    continue;
                }

                string ingPrefab = Utils.GetPrefabName(req.m_resItem.gameObject);
                string? ingShared = req.m_resItem.m_itemData?.m_shared?.m_name;
                addedAnyIngredient = true;

                if (!string.IsNullOrEmpty(ingPrefab) && TryGetKnownMaterialBiome(ingPrefab, out _))
                {
                    leafPrefabs.Add(ingPrefab);
                }
                else if (!string.IsNullOrEmpty(ingShared) && RecipeBySharedName.ContainsKey(ingShared!))
                {
                    CollectBaseMaterials(ingShared!, visited, leafPrefabs);
                }
                else if (!string.IsNullOrEmpty(ingPrefab))
                {
                    leafPrefabs.Add(ingPrefab);
                }
            }

            if (addedAnyIngredient)
            {
                return;
            }
        }

        if (!string.IsNullOrEmpty(prefabName))
        {
            leafPrefabs.Add(prefabName);
        }
    }

    private static void EnsureEnemyIndexBuilt()
    {
        if (_enemyIndexBuilt)
        {
            return;
        }

        SpawnSystem? spawnSystem = ZoneSystem.instance != null && ZoneSystem.instance.m_zoneCtrlPrefab != null
            ? ZoneSystem.instance.m_zoneCtrlPrefab.GetComponent<SpawnSystem>()
            : null;

        if (spawnSystem == null && SpawnSystem.m_instances != null && SpawnSystem.m_instances.Count > 0)
        {
            spawnSystem = SpawnSystem.m_instances[0];
        }

        if (spawnSystem == null)
        {
            return;
        }

        _enemyIndexBuilt = true;
        ItemBiomeCache.Clear();

        List<GameObject> spawnedPrefabs = new();

        if (spawnSystem.m_spawnLists != null)
        {
            foreach (SpawnSystemList list in spawnSystem.m_spawnLists)
            {
                if (list == null || list.m_spawners == null) continue;
                IndexSpawnerList(list.m_spawners, Heightmap.Biome.None, spawnedPrefabs);
            }
        }

        if (AltBiomeList.m_altBiomes != null)
        {
            foreach (AltBiome altBiome in AltBiomeList.m_altBiomes)
            {
                if (altBiome == null || !altBiome.m_enabled || altBiome.m_spawn == null) continue;
                IndexSpawnerList(altBiome.m_spawn, altBiome.m_biome, spawnedPrefabs);
            }
        }

        // Apply manual overrides on top of SpawnSystem
        foreach (KeyValuePair<string, BiomeTier> kvp in EnemyBiomeOverrides)
        {
            EnemyBiomeByName[kvp.Key] = kvp.Value;
        }

        // Propagate classified enemy biomes to their CharacterDrop items (lowest biome wins)
        HashSet<GameObject> processedPrefabs = new();
        foreach (GameObject prefab in spawnedPrefabs)
        {
            if (prefab != null && processedPrefabs.Add(prefab))
            {
                DeriveDropsFromCharacterPrefab(prefab);
            }
        }

        if (ZNetScene.instance != null && ZNetScene.instance.m_prefabs != null)
        {
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
            {
                if (prefab != null && processedPrefabs.Add(prefab))
                {
                    DeriveDropsFromCharacterPrefab(prefab);
                }
            }
        }

        Plugin.Log.LogInfo(
            $"[BiomeClassifier] Indexed {EnemyBiomeByName.Count} enemies from SpawnSystem and derived {DerivedMaterialBiomes.Count} material biomes from mob drops.");
    }

    private static void IndexSpawnerList(
        List<SpawnSystem.SpawnData> spawners,
        Heightmap.Biome fallbackBiomeMask,
        List<GameObject> spawnedPrefabs)
    {
        foreach (SpawnSystem.SpawnData spawnData in spawners)
        {
            if (spawnData == null || !spawnData.m_enabled || spawnData.m_devDisabled || spawnData.m_prefab == null)
            {
                continue;
            }

            // Skip global-key and persistent-event spawners (e.g. post-boss night spawns of Fulings/Seekers/Charred in Meadows)
            if (!string.IsNullOrEmpty(spawnData.m_requiredGlobalKey) ||
                !string.IsNullOrEmpty(spawnData.m_requiredPersistentEvent))
            {
                continue;
            }

            Character character = spawnData.m_prefab.GetComponent<Character>();
            if (character == null || string.IsNullOrEmpty(character.m_name))
            {
                continue;
            }

            Heightmap.Biome effectiveMask = spawnData.m_biome;
            if (fallbackBiomeMask != Heightmap.Biome.None)
            {
                Heightmap.Biome intersection = effectiveMask & fallbackBiomeMask;
                effectiveMask = intersection != Heightmap.Biome.None ? intersection : fallbackBiomeMask;
            }

            BiomeTier tier = MapLowestBiome(effectiveMask);
            if (tier == BiomeTier.Uncategorized)
            {
                continue;
            }

            spawnedPrefabs.Add(spawnData.m_prefab);

            if (!EnemyBiomeByName.TryGetValue(character.m_name, out BiomeTier existing) || tier < existing)
            {
                EnemyBiomeByName[character.m_name] = tier;
            }
        }
    }

    private static void DeriveDropsFromCharacterPrefab(GameObject prefab)
    {
        Character character = prefab.GetComponent<Character>();
        if (character == null || string.IsNullOrEmpty(character.m_name))
        {
            return;
        }

        if (!EnemyBiomeByName.TryGetValue(character.m_name, out BiomeTier enemyTier) ||
            enemyTier == BiomeTier.Uncategorized)
        {
            return;
        }

        CharacterDrop charDrop = prefab.GetComponent<CharacterDrop>();
        if (charDrop == null || charDrop.m_drops == null)
        {
            return;
        }

        foreach (CharacterDrop.Drop drop in charDrop.m_drops)
        {
            if (drop == null || drop.m_prefab == null || drop.m_chance <= 0f || drop.m_amountMax <= 0)
            {
                continue;
            }

            ItemDrop itemDrop = drop.m_prefab.GetComponent<ItemDrop>();
            if (itemDrop == null)
            {
                continue;
            }

            string dropPrefabName = Utils.GetPrefabName(drop.m_prefab);
            if (string.IsNullOrEmpty(dropPrefabName))
            {
                continue;
            }

            if (!DerivedMaterialBiomes.TryGetValue(dropPrefabName, out BiomeTier existing) || enemyTier < existing)
            {
                DerivedMaterialBiomes[dropPrefabName] = enemyTier;
            }
        }
    }

    private static BiomeTier MapLowestBiome(Heightmap.Biome mask)
    {
        if ((mask & Heightmap.Biome.Meadows) != 0) return BiomeTier.Meadows;
        if ((mask & Heightmap.Biome.BlackForest) != 0) return BiomeTier.BlackForest;
        if ((mask & Heightmap.Biome.Ocean) != 0) return BiomeTier.Ocean;
        if ((mask & Heightmap.Biome.Swamp) != 0) return BiomeTier.Swamp;
        if ((mask & Heightmap.Biome.Mountain) != 0) return BiomeTier.Mountain;
        if ((mask & Heightmap.Biome.Plains) != 0) return BiomeTier.Plains;
        if ((mask & Heightmap.Biome.Mistlands) != 0) return BiomeTier.Mistlands;
        if ((mask & Heightmap.Biome.AshLands) != 0) return BiomeTier.Ashlands;
        if ((mask & Heightmap.Biome.DeepNorth) != 0) return BiomeTier.DeepNorth;
        return BiomeTier.Uncategorized;
    }

    private static void EnsureItemIndexBuilt()
    {
        if (_itemIndexBuilt || ObjectDB.instance == null)
        {
            return;
        }

        _itemIndexBuilt = true;

        foreach (GameObject itemObj in ObjectDB.instance.m_items)
        {
            if (itemObj == null) continue;
            ItemDrop drop = itemObj.GetComponent<ItemDrop>();
            string? sharedName = drop?.m_itemData?.m_shared?.m_name;
            if (!string.IsNullOrEmpty(sharedName) && !PrefabNameBySharedName.ContainsKey(sharedName!))
            {
                PrefabNameBySharedName[sharedName!] = Utils.GetPrefabName(itemObj);
            }
        }

        foreach (Recipe recipe in ObjectDB.instance.m_recipes)
        {
            if (recipe == null ||
                recipe.m_item == null ||
                !recipe.m_enabled ||
                recipe.m_noCraftOnlyUpgrade ||
                (recipe.m_craftingStation != null && recipe.m_craftingStation.m_upgrader))
            {
                continue;
            }
            string? sharedName = recipe.m_item.m_itemData?.m_shared?.m_name;
            if (string.IsNullOrEmpty(sharedName)) continue;

            if (!PrefabNameBySharedName.ContainsKey(sharedName!))
            {
                PrefabNameBySharedName[sharedName!] = Utils.GetPrefabName(recipe.m_item.gameObject);
            }

            if (!RecipeBySharedName.ContainsKey(sharedName!))
            {
                RecipeBySharedName[sharedName!] = recipe;
            }
        }
    }
}
