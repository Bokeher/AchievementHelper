using System;
using System.Collections.Generic;

namespace AchievementHelper.Services;

public static partial class BiomeClassifier
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
}
