namespace AchievementHelper.Services;

public enum BiomeTier
{
    Meadows = 0,
    BlackForest = 1,
    Swamp = 2,
    Mountain = 3,
    Plains = 4,
    Mistlands = 5,
    Ashlands = 6,
    DeepNorth = 7,
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
