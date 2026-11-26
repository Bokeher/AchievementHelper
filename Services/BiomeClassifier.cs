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

    // Base materials mapped by prefab name (case-insensitive; take precedence over mob-derived drops and cooking conversions)
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
            ["BarberKit"] = BiomeTier.Meadows,
            ["Ironpit"] = BiomeTier.Meadows,
            ["StoneRock"] = BiomeTier.Meadows,
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
            ["TrophyForestTroll"] = BiomeTier.BlackForest,
            ["BjornHide"] = BiomeTier.BlackForest,
            ["BearHide"] = BiomeTier.BlackForest,
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
            ["BarrelRings"] = BiomeTier.Swamp,
            ["CandleWick"] = BiomeTier.Swamp,
            ["SilverNecklace"] = BiomeTier.Swamp,
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
            ["Flax"] = BiomeTier.Plains,
            ["LinenThread"] = BiomeTier.Plains,
            ["Barley"] = BiomeTier.Plains,
            ["BarleyFlour"] = BiomeTier.Plains,
            ["SpicePlains"] = BiomeTier.Plains,
            ["Cloudberry"] = BiomeTier.Plains,
            ["GoblinTotem"] = BiomeTier.Plains,
            ["YagluthDrop"] = BiomeTier.Plains,
            ["ChickenEgg"] = BiomeTier.Plains,
            ["ChickenMeat"] = BiomeTier.Plains,
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
            ["HareMeat"] = BiomeTier.Mistlands,
            ["MushroomJotunPuffs"] = BiomeTier.Mistlands,
            ["MushroomMagecap"] = BiomeTier.Mistlands,
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
            ["Charredskull"] = BiomeTier.Ashlands,
            ["CharredCogwheel"] = BiomeTier.Ashlands,
            ["Pot_Shard_Green"] = BiomeTier.Ashlands,
            ["AskHide"] = BiomeTier.Ashlands,
            ["AskBladder"] = BiomeTier.Ashlands,
            ["AsksvinNeck"] = BiomeTier.Ashlands,
            ["AsksvinPelvis"] = BiomeTier.Ashlands,
            ["AsksvinRibcage"] = BiomeTier.Ashlands,
            ["AsksvinSkull"] = BiomeTier.Ashlands,
            ["AsksvinCarrionNeck"] = BiomeTier.Ashlands,
            ["AsksvinCarrionPelvic"] = BiomeTier.Ashlands,
            ["AsksvinCarrionRibcage"] = BiomeTier.Ashlands,
            ["AsksvinCarrionSkull"] = BiomeTier.Ashlands,
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
            ["AsksvinMeat"] = BiomeTier.Ashlands,
            ["BonemawSerpentMeat"] = BiomeTier.Ashlands,
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
            ["GlowWorm"] = BiomeTier.DeepNorth,
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

    // Static biome progression per crafting station level (index 0 = level 1, index 1 = level 2, ...)
    private static readonly Dictionary<string, BiomeTier[]> StationLevelBiomes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["$piece_workbench"] = new[]
            {
                BiomeTier.Meadows,      // lvl 1: Workbench
                BiomeTier.Meadows,      // lvl 2: Chopping Block
                BiomeTier.Meadows,      // lvl 3: Tanning Rack
                BiomeTier.BlackForest,  // lvl 4: Adze
                BiomeTier.Mountain,     // lvl 5: Tool Shelf
            },
            ["$piece_forge"] = new[]
            {
                BiomeTier.BlackForest,  // lvl 1: Forge
                BiomeTier.BlackForest,  // lvl 2: Anvils
                BiomeTier.BlackForest,  // lvl 3: Forge Cooler
                BiomeTier.Swamp,        // lvl 4: Smith's Anvil
                BiomeTier.Swamp,        // lvl 5: Forge Toolrack
                BiomeTier.Swamp,        // lvl 6: Forge Bellows
                BiomeTier.Mountain,     // lvl 7: Grinding Wheel
            },
            ["$piece_cauldron"] = new[]
            {
                BiomeTier.BlackForest,  // lvl 1: Cauldron
                BiomeTier.Swamp,        // lvl 2: Spice Rack
                BiomeTier.Mountain,     // lvl 3: Butcher's Table
                BiomeTier.Plains,       // lvl 4: Pots and Pans
                BiomeTier.Mistlands,    // lvl 5: Mortar and Pestle
                BiomeTier.Ashlands,     // lvl 6: Ashlands upgrade
                BiomeTier.DeepNorth,    // lvl 7: Deep North upgrade
            },
            ["$piece_meadcauldron"] = new[] { BiomeTier.BlackForest },
            ["$piece_preptable"] = new[] { BiomeTier.Swamp },
            ["$piece_artisanstation"] = new[] { BiomeTier.Mountain },
            ["$piece_blackforge"] = new[]
            {
                BiomeTier.Mistlands,    // lvl 1
                BiomeTier.Mistlands,    // lvl 2
                BiomeTier.Ashlands,     // lvl 3
                BiomeTier.Ashlands,     // lvl 4
                BiomeTier.DeepNorth,    // lvl 5
            },
            ["$piece_magetable"] = new[]
            {
                BiomeTier.Mistlands,    // lvl 1
                BiomeTier.Mistlands,    // lvl 2
                BiomeTier.Ashlands,     // lvl 3
                BiomeTier.DeepNorth,    // lvl 4
            },
        };

    // Static biome per CookingStation prefab name (campfire grill, iron cooking station, stone oven)
    private static readonly Dictionary<string, BiomeTier> CookingStationBiomes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["piece_cookingstation"] = BiomeTier.Meadows,
            ["piece_cookingstation_iron"] = BiomeTier.Swamp,
            ["piece_oven"] = BiomeTier.Mountain,
        };

    private readonly struct CookingConversionSource
    {
        public readonly string FromSharedName;
        public readonly BiomeTier StationBiome;

        public CookingConversionSource(string fromSharedName, BiomeTier stationBiome)
        {
            FromSharedName = fromSharedName;
            StationBiome = stationBiome;
        }
    }

    private static readonly Dictionary<string, BiomeTier> EnemyBiomeByName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, BiomeTier> DerivedMaterialBiomes = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Recipe> RecipeBySharedName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, List<CookingConversionSource>> CookingSourcesBySharedName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, Piece> PieceByName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> PrefabNameBySharedName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, BiomeTier> ItemBiomeCache = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, BiomeTier> PieceBiomeCache = new(StringComparer.Ordinal);
    private static readonly HashSet<string> LoggedSummaryKeys = new(StringComparer.OrdinalIgnoreCase);

    private static bool _enemyIndexBuilt;
    private static bool _itemIndexBuilt;
    private static bool _cookingIndexBuilt;
    private static bool _pieceIndexBuilt;

    public static void ResetCache()
    {
        EnemyBiomeByName.Clear();
        DerivedMaterialBiomes.Clear();
        RecipeBySharedName.Clear();
        CookingSourcesBySharedName.Clear();
        PieceByName.Clear();
        PrefabNameBySharedName.Clear();
        ItemBiomeCache.Clear();
        PieceBiomeCache.Clear();
        LoggedSummaryKeys.Clear();
        _enemyIndexBuilt = false;
        _itemIndexBuilt = false;
        _cookingIndexBuilt = false;
        _pieceIndexBuilt = false;
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
        EnsureCookingIndexBuilt();

        if (ItemBiomeCache.TryGetValue(sharedName, out BiomeTier cached))
        {
            return cached;
        }

        HashSet<string> visiting = new(StringComparer.Ordinal);
        BiomeTier result = ResolveItemBiome(sharedName, visiting);
        ItemBiomeCache[sharedName] = result;
        return result;
    }

    public static BiomeTier GetPieceBiome(string pieceName)
    {
        if (string.IsNullOrEmpty(pieceName))
        {
            return BiomeTier.Uncategorized;
        }

        EnsureEnemyIndexBuilt();
        EnsureItemIndexBuilt();
        EnsureCookingIndexBuilt();
        EnsurePieceIndexBuilt();

        if (PieceBiomeCache.TryGetValue(pieceName, out BiomeTier cached))
        {
            return cached;
        }

        if (!PieceByName.TryGetValue(pieceName, out Piece piece) || piece == null)
        {
            PieceBiomeCache[pieceName] = BiomeTier.Uncategorized;
            return BiomeTier.Uncategorized;
        }

        HashSet<string> visiting = new(StringComparer.Ordinal);
        BiomeTier highest = BiomeTier.Meadows;
        bool anyIngredient = false;

        if (piece.m_resources != null)
        {
            foreach (Piece.Requirement req in piece.m_resources)
            {
                if (req == null || req.m_resItem == null || req.m_amount <= 0)
                {
                    continue;
                }

                anyIngredient = true;
                BiomeTier tier = ResolveIngredientBiome(req.m_resItem, visiting);
                if (tier == BiomeTier.Uncategorized)
                {
                    PieceBiomeCache[pieceName] = BiomeTier.Uncategorized;
                    return BiomeTier.Uncategorized;
                }

                if (tier > highest)
                {
                    highest = tier;
                }
            }
        }

        BiomeTier result = anyIngredient ? highest : BiomeTier.Uncategorized;
        if (result != BiomeTier.Uncategorized &&
            piece.m_craftingStation != null &&
            !piece.m_craftingStation.m_upgrader)
        {
            BiomeTier stationTier = GetCraftingStationBiome(piece.m_craftingStation, 1);
            if (stationTier != BiomeTier.Uncategorized && stationTier > result)
            {
                result = stationTier;
            }
        }

        PieceBiomeCache[pieceName] = result;
        return result;
    }

    public static void LogUncategorizedPieces(string logKey, IEnumerable<string> pieceNames)
    {
        if (!LoggedSummaryKeys.Add("pieces:" + logKey))
        {
            return;
        }

        EnsureEnemyIndexBuilt();
        EnsureItemIndexBuilt();
        EnsureCookingIndexBuilt();
        EnsurePieceIndexBuilt();

        Dictionary<string, int> unknownCounts = new(StringComparer.OrdinalIgnoreCase);
        int uncategorizedPieces = 0;

        foreach (string pieceName in pieceNames)
        {
            if (GetPieceBiome(pieceName) != BiomeTier.Uncategorized)
            {
                continue;
            }

            uncategorizedPieces++;
            if (!PieceByName.TryGetValue(pieceName, out Piece piece) || piece == null || piece.m_resources == null)
            {
                string key = $"MISSING_PIECE:{pieceName}";
                unknownCounts.TryGetValue(key, out int c);
                unknownCounts[key] = c + 1;
                continue;
            }

            HashSet<string> visited = new(StringComparer.Ordinal);
            HashSet<string> unknownLeaves = new(StringComparer.OrdinalIgnoreCase);
            foreach (Piece.Requirement req in piece.m_resources)
            {
                if (req == null || req.m_resItem == null || req.m_amount <= 0) continue;
                string ingPrefab = Utils.GetPrefabName(req.m_resItem.gameObject);
                string? ingShared = req.m_resItem.m_itemData?.m_shared?.m_name;
                if (!string.IsNullOrEmpty(ingPrefab) && TryGetKnownMaterialBiome(ingPrefab, out _)) continue;
                if (!string.IsNullOrEmpty(ingShared) && GetItemBiome(ingShared!) != BiomeTier.Uncategorized) continue;

                if (!string.IsNullOrEmpty(ingShared) &&
                    (RecipeBySharedName.ContainsKey(ingShared!) || CookingSourcesBySharedName.ContainsKey(ingShared!)))
                {
                    CollectUnknownLeaves(ingShared!, visited, unknownLeaves);
                }
                else if (!string.IsNullOrEmpty(ingPrefab))
                {
                    unknownLeaves.Add(ingPrefab);
                }
            }

            foreach (string leaf in unknownLeaves)
            {
                unknownCounts.TryGetValue(leaf, out int c);
                unknownCounts[leaf] = c + 1;
            }
        }

        if (uncategorizedPieces == 0)
        {
            Plugin.Log.LogInfo($"[BiomeClassifier] {logKey}: all build pieces categorized.");
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
            $"[BiomeClassifier] {logKey}: {uncategorizedPieces} uncategorized build pieces. Top unknown materials: {topUnknown}");
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
        EnsureCookingIndexBuilt();

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
            HashSet<string> unknownLeaves = new(StringComparer.OrdinalIgnoreCase);
            CollectUnknownLeaves(sharedName, visited, unknownLeaves);

            foreach (string leaf in unknownLeaves)
            {
                unknownCounts.TryGetValue(leaf, out int c);
                unknownCounts[leaf] = c + 1;
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

    private static BiomeTier ResolveItemBiome(string sharedName, HashSet<string> visiting)
    {
        if (string.IsNullOrEmpty(sharedName))
        {
            return BiomeTier.Uncategorized;
        }

        if (ItemBiomeCache.TryGetValue(sharedName, out BiomeTier cached))
        {
            return cached;
        }

        if (PrefabNameBySharedName.TryGetValue(sharedName, out string prefabName) &&
            TryGetKnownMaterialBiome(prefabName, out BiomeTier directTier))
        {
            return directTier;
        }

        if (!visiting.Add(sharedName))
        {
            return BiomeTier.Uncategorized;
        }

        try
        {
            // 1. Crafting recipe (enabled, non-upgrade-only) + required station level
            if (RecipeBySharedName.TryGetValue(sharedName, out Recipe recipe) &&
                recipe.m_resources != null &&
                recipe.m_resources.Length > 0)
            {
                BiomeTier fromRecipe = EvaluateRecipe(recipe, visiting);
                if (fromRecipe != BiomeTier.Uncategorized)
                {
                    return fromRecipe;
                }
            }

            // 2. CookingStation conversions (campfire grill, iron cooking station, stone oven)
            if (CookingSourcesBySharedName.TryGetValue(sharedName, out List<CookingConversionSource> sources) &&
                sources.Count > 0)
            {
                BiomeTier lowest = BiomeTier.Uncategorized;
                foreach (CookingConversionSource source in sources)
                {
                    BiomeTier fromTier = ResolveItemBiome(source.FromSharedName, visiting);
                    if (fromTier == BiomeTier.Uncategorized)
                    {
                        continue;
                    }

                    BiomeTier effectiveTier = fromTier;
                    if (source.StationBiome != BiomeTier.Uncategorized && source.StationBiome > effectiveTier)
                    {
                        effectiveTier = source.StationBiome;
                    }

                    if (effectiveTier < lowest)
                    {
                        lowest = effectiveTier;
                    }
                }

                if (lowest != BiomeTier.Uncategorized)
                {
                    return lowest;
                }
            }

            return BiomeTier.Uncategorized;
        }
        finally
        {
            visiting.Remove(sharedName);
        }
    }

    private static BiomeTier EvaluateRecipe(Recipe recipe, HashSet<string> visiting)
    {
        BiomeTier ingredientTier;

        if (recipe.m_requireOnlyOneIngredient)
        {
            BiomeTier lowest = BiomeTier.Uncategorized;
            foreach (Piece.Requirement req in recipe.m_resources)
            {
                if (req == null || req.m_resItem == null || req.m_amount <= 0 || req.m_upgraderResource)
                {
                    continue;
                }

                BiomeTier tier = ResolveIngredientBiome(req.m_resItem, visiting);
                if (tier != BiomeTier.Uncategorized && tier < lowest)
                {
                    lowest = tier;
                }
            }

            ingredientTier = lowest;
        }
        else
        {
            BiomeTier highest = BiomeTier.Meadows;
            bool anyIngredient = false;

            foreach (Piece.Requirement req in recipe.m_resources)
            {
                if (req == null || req.m_resItem == null || req.m_amount <= 0 || req.m_upgraderResource)
                {
                    continue;
                }

                anyIngredient = true;
                BiomeTier tier = ResolveIngredientBiome(req.m_resItem, visiting);
                if (tier == BiomeTier.Uncategorized)
                {
                    return BiomeTier.Uncategorized;
                }

                if (tier > highest)
                {
                    highest = tier;
                }
            }

            ingredientTier = anyIngredient ? highest : BiomeTier.Uncategorized;
        }

        if (ingredientTier == BiomeTier.Uncategorized)
        {
            return BiomeTier.Uncategorized;
        }

        if (recipe.m_craftingStation != null && !recipe.m_craftingStation.m_upgrader)
        {
            BiomeTier stationTier = GetCraftingStationBiome(recipe.m_craftingStation, recipe.m_minStationLevel);
            if (stationTier != BiomeTier.Uncategorized && stationTier > ingredientTier)
            {
                return stationTier;
            }
        }

        return ingredientTier;
    }

    private static BiomeTier ResolveIngredientBiome(ItemDrop resItem, HashSet<string> visiting)
    {
        string ingPrefab = Utils.GetPrefabName(resItem.gameObject);
        if (!string.IsNullOrEmpty(ingPrefab) && TryGetKnownMaterialBiome(ingPrefab, out BiomeTier direct))
        {
            return direct;
        }

        string? ingShared = resItem.m_itemData?.m_shared?.m_name;
        if (!string.IsNullOrEmpty(ingShared))
        {
            return ResolveItemBiome(ingShared!, visiting);
        }

        return BiomeTier.Uncategorized;
    }

    private static BiomeTier GetCraftingStationBiome(CraftingStation station, int minLevel)
    {
        if (station == null || string.IsNullOrEmpty(station.m_name) || station.m_upgrader)
        {
            return BiomeTier.Uncategorized;
        }

        if (!StationLevelBiomes.TryGetValue(station.m_name, out BiomeTier[] tiers) || tiers.Length == 0)
        {
            return BiomeTier.Uncategorized;
        }

        int index = Math.Max(0, Math.Min(minLevel - 1, tiers.Length - 1));
        return tiers[index];
    }

    private static void CollectUnknownLeaves(
        string sharedName,
        HashSet<string> visited,
        HashSet<string> unknownLeaves)
    {
        if (string.IsNullOrEmpty(sharedName) || !visited.Add(sharedName))
        {
            return;
        }

        if (PrefabNameBySharedName.TryGetValue(sharedName, out string prefabName) &&
            TryGetKnownMaterialBiome(prefabName, out _))
        {
            return;
        }

        if (RecipeBySharedName.TryGetValue(sharedName, out Recipe recipe) &&
            recipe.m_resources != null &&
            recipe.m_resources.Length > 0)
        {
            bool anyIngredient = false;
            foreach (Piece.Requirement req in recipe.m_resources)
            {
                if (req == null || req.m_resItem == null || req.m_amount <= 0 || req.m_upgraderResource)
                {
                    continue;
                }

                anyIngredient = true;
                string ingPrefab = Utils.GetPrefabName(req.m_resItem.gameObject);
                string? ingShared = req.m_resItem.m_itemData?.m_shared?.m_name;

                if (!string.IsNullOrEmpty(ingPrefab) && TryGetKnownMaterialBiome(ingPrefab, out _))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(ingShared) && GetItemBiome(ingShared!) != BiomeTier.Uncategorized)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(ingShared) &&
                    (RecipeBySharedName.ContainsKey(ingShared!) || CookingSourcesBySharedName.ContainsKey(ingShared!)))
                {
                    CollectUnknownLeaves(ingShared!, visited, unknownLeaves);
                }
                else if (!string.IsNullOrEmpty(ingPrefab))
                {
                    unknownLeaves.Add(ingPrefab);
                }
            }

            if (anyIngredient)
            {
                return;
            }
        }

        if (CookingSourcesBySharedName.TryGetValue(sharedName, out List<CookingConversionSource> sources) &&
            sources.Count > 0)
        {
            foreach (CookingConversionSource source in sources)
            {
                CollectUnknownLeaves(source.FromSharedName, visited, unknownLeaves);
            }

            return;
        }

        if (!string.IsNullOrEmpty(prefabName))
        {
            unknownLeaves.Add(prefabName);
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
        ItemBiomeCache.Clear();

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

    private static void EnsureCookingIndexBuilt()
    {
        if (_cookingIndexBuilt || ZNetScene.instance == null || ZNetScene.instance.m_prefabs == null)
        {
            return;
        }

        _cookingIndexBuilt = true;
        ItemBiomeCache.Clear();

        int cookingConversions = 0;

        foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
        {
            if (prefab == null) continue;

            CookingStation cookingStation = prefab.GetComponent<CookingStation>();
            if (cookingStation == null || cookingStation.m_conversion == null) continue;

            string stationPrefab = Utils.GetPrefabName(prefab);
            BiomeTier stationBiome = CookingStationBiomes.TryGetValue(stationPrefab, out BiomeTier tier)
                ? tier
                : BiomeTier.Uncategorized;

            foreach (CookingStation.ItemConversion conv in cookingStation.m_conversion)
            {
                string? fromShared = conv?.m_from?.m_itemData?.m_shared?.m_name;
                string? toShared = conv?.m_to?.m_itemData?.m_shared?.m_name;
                if (string.IsNullOrEmpty(fromShared) || string.IsNullOrEmpty(toShared) || fromShared == toShared)
                {
                    continue;
                }

                if (!PrefabNameBySharedName.ContainsKey(fromShared!))
                {
                    PrefabNameBySharedName[fromShared!] = Utils.GetPrefabName(conv!.m_from.gameObject);
                }

                if (!PrefabNameBySharedName.ContainsKey(toShared!))
                {
                    PrefabNameBySharedName[toShared!] = Utils.GetPrefabName(conv!.m_to.gameObject);
                }

                if (!CookingSourcesBySharedName.TryGetValue(toShared!, out List<CookingConversionSource> sources))
                {
                    sources = new List<CookingConversionSource>();
                    CookingSourcesBySharedName[toShared!] = sources;
                }

                sources.Add(new CookingConversionSource(fromShared!, stationBiome));
                cookingConversions++;
            }
        }

        Plugin.Log.LogInfo($"[BiomeClassifier] Indexed {cookingConversions} cooking conversions.");
    }

    private static void EnsurePieceIndexBuilt()
    {
        if (_pieceIndexBuilt || ZNetScene.instance == null || ZNetScene.instance.m_prefabs == null)
        {
            return;
        }

        _pieceIndexBuilt = true;
        PieceBiomeCache.Clear();

        foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
        {
            if (prefab == null) continue;
            Piece piece = prefab.GetComponent<Piece>();
            if (piece == null || string.IsNullOrEmpty(piece.m_name)) continue;

            if (!PieceByName.TryGetValue(piece.m_name, out Piece existing) || (!existing.m_enabled && piece.m_enabled))
            {
                PieceByName[piece.m_name] = piece;
            }
        }

        if (ObjectDB.instance != null && ObjectDB.instance.m_items != null)
        {
            foreach (GameObject itemObj in ObjectDB.instance.m_items)
            {
                if (itemObj == null) continue;
                PieceTable? table = itemObj.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_buildPieces;
                if (table == null || table.m_pieces == null) continue;

                foreach (GameObject piecePrefab in table.m_pieces)
                {
                    if (piecePrefab == null) continue;
                    Piece piece = piecePrefab.GetComponent<Piece>();
                    if (piece == null || string.IsNullOrEmpty(piece.m_name)) continue;

                    if (!PieceByName.ContainsKey(piece.m_name))
                    {
                        PieceByName[piece.m_name] = piece;
                    }
                }
            }
        }

        Plugin.Log.LogInfo($"[BiomeClassifier] Indexed {PieceByName.Count} build pieces.");
    }
}
