using BepInEx.Configuration;

namespace TestAccountVariety;

internal static class VarietyConfig {
    public static ConfigEntry<int> cubeExplodeChance = null!;
    public static ConfigEntry<int> yippeeParticleChance = null!;
    public static ConfigEntry<bool> giftMimicSpawnsOutsideEnemies = null!;

    public static void Initialize(ConfigFile configFile) {
        cubeExplodeChance = configFile.Bind("Cube", "6. Cube Explode Chance", 50, "Chance for the cube to explode.");
        yippeeParticleChance = configFile.Bind("Yippee", "6. Yippee Particle Chance", 60, "Chance for the yippee scrap to throw confetti.");
        giftMimicSpawnsOutsideEnemies = configFile.Bind("Gift Mimic", "3. Spawns Outside Enemies", false,
                                                        "If set to true, will also spawn outside enemies. Starlancer AI Fix Required!!!.");
    }
}