namespace AchievementHelper.Services;

public enum BiomeTier
{
    Meadows = 0,
    BlackForest = 1,
    Ocean = 2,
    Swamp = 3,
    Mountain = 4,
    Plains = 5,
    Mistlands = 6,
    Ashlands = 7,
    DeepNorth = 8,
    Uncategorized = 99
}

public static class BiomeTierExtensions
{
    public static string GetDisplayName(this BiomeTier tier)
    {
        return tier switch
        {
            BiomeTier.Meadows => "Meadows",
            BiomeTier.BlackForest => "Black Forest",
            BiomeTier.Ocean => "Ocean",
            BiomeTier.Swamp => "Swamp",
            BiomeTier.Mountain => "Mountain",
            BiomeTier.Plains => "Plains",
            BiomeTier.Mistlands => "Mistlands",
            BiomeTier.Ashlands => "Ashlands",
            BiomeTier.DeepNorth => "Deep North",
            _ => "Uncategorized"
        };
    }
}
