using BepInEx.Configuration;

namespace TestAccountVariety.Config;

// ReSharper disable once ClassNeverInstantiated.Global
internal class GiftMimicConfig : VarietyConfig {
    public static ConfigEntry<bool> giftMimicSpawnsOutsideEnemies = null!;
    public static ConfigEntry<int> giftMimicAlternativeVariantChance = null!;
    public static ConfigEntry<string> giftMimicEnemyBlacklist = null!;
    public static ConfigEntry<int> giftMimicScrapChance = null!;
    public static ConfigEntry<string> giftMimicScrapBlacklist = null!;

    public override void Initialize(ConfigFile configFile) {
        giftMimicSpawnsOutsideEnemies = configFile.BindBool("Gift Mimic", "3. Spawns Outside Enemies", false,
                                                            "If set to true, will also spawn outside enemies. Starlancer AI Fix Required!!!.");

        giftMimicAlternativeVariantChance = configFile.BindInt("Gift Mimic", "4. Gift Mimic Alternative Variant Chance", 35,
                                                               "Defines the chance for the Gift Mimic to use an alternative texture."
                                                             + " Best when combined with UpturnedVariety.");

        giftMimicEnemyBlacklist = configFile.BindString("Gift Mimic", "5. Enemy Blacklist", "example1, example2",
                                                        "A comma separated list of blacklisted enemies. Uses startsWith, so you don't need the full name.");

        giftMimicScrapChance = configFile.BindInt("Gift Mimic", "6. Scrap Chance", 45, "The chance for spawning scrap instead of enemies");

        giftMimicScrapBlacklist = configFile.BindString("Gift Mimic", "7. Scrap Blacklist", "example1, example2",
                                                        "A comma separated list of blacklisted scraps. Uses startsWith, so you don't need the full name.");
    }
}