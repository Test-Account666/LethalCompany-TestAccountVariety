using BepInEx.Configuration;

namespace TestAccountVariety;

internal static class VarietyConfig {
    public static ConfigEntry<int> cubeExplodeChance = null!;
    public static ConfigEntry<int> yippeeParticleChance = null!;
    public static ConfigEntry<bool> giftMimicSpawnsOutsideEnemies = null!;
    public static ConfigEntry<int> giftMimicAlternativeVariantChance = null!;
    public static ConfigEntry<string> giftMimicEnemyBlacklist = null!;
    public static ConfigEntry<int> giftMimicScrapChance = null!;
    public static ConfigEntry<string> giftMimicScrapBlacklist = null!;
    public static ConfigEntry<int> bombExplodeChance = null!;
    public static ConfigEntry<int> coloredCubeLightChance = null!;
    public static ConfigEntry<int> coloredCubeRainbowChance = null!;
    public static ConfigEntry<bool> fixTwoHandedWeapons = null!;

    public static void Initialize(ConfigFile configFile) {
        cubeExplodeChance = configFile.Bind("Cube", "6. Cube Explode Chance", 50, new ConfigDescription("Chance for the cube to explode.",
                                                                                                        new AcceptableValueRange<int>(0, 100)));


        yippeeParticleChance = configFile.Bind("Yippee", "6. Yippee Particle Chance", 60, "Chance for the yippee scrap to throw confetti.");


        giftMimicSpawnsOutsideEnemies = configFile.Bind("Gift Mimic", "3. Spawns Outside Enemies", false,
                                                        "If set to true, will also spawn outside enemies. Starlancer AI Fix Required!!!.");

        giftMimicAlternativeVariantChance = configFile.Bind("Gift Mimic", "4. Gift Mimic Alternative Variant Chance", 35, new ConfigDescription(
                                                                "Defines the chance for the Gift Mimic to use an alternative texture."
                                                              + " Best when combined with UpturnedVariety.",
                                                                new AcceptableValueRange<int>(0, 100)));

        giftMimicEnemyBlacklist = configFile.Bind("Gift Mimic", "5. Enemy Blacklist", "example1, example2",
                                                  "A comma separated list of blacklisted enemies. "
                                                + "Uses startsWith, so you don't need the full name.");

        giftMimicScrapChance = configFile.Bind("Gift Mimic", "6. Scrap Chance", 45,
                                               new ConfigDescription("The chance for spawning scrap instead of enemies",
                                                                     new AcceptableValueRange<int>(0, 100)));

        giftMimicScrapBlacklist = configFile.Bind("Gift Mimic", "7. Scrap Blacklist", "example1, example2",
                                                  "A comma separated list of blacklisted scraps. "
                                                + "Uses startsWith, so you don't need the full name.");


        bombExplodeChance = configFile.Bind("Bomb", "6. Bomb Explode Chance", 4, new ConfigDescription("Chance for the bomb to explode on drop.",
                                                                                                       new AcceptableValueRange<int>(0, 100)));

        coloredCubeLightChance = configFile.Bind("Colored Cube", "6. Cube Light Chance", 10, new ConfigDescription(
                                                     "Chance for the colored cube to have a light.",
                                                     new AcceptableValueRange<int>(0, 100)));

        coloredCubeRainbowChance = configFile.Bind("Colored Cube", "7. Cube Rainbow Chance", 6, new ConfigDescription(
                                                       "Chance for the colored cube to be rainbow.",
                                                       new AcceptableValueRange<int>(0, 100)));


        fixTwoHandedWeapons = configFile.Bind("Bug Fixes", "Fix Two Handed Weapons", true, "If set to true, will fix the two Handed Weapons. "
                                                                                         + "If you swing a two Handed Weapon, you can switch to a different item. "
                                                                                         + "This option prevents this behavior.");
    }
}