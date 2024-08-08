using BepInEx.Configuration;

namespace TestAccountVariety;

internal static class VarietyConfig {
    public static ConfigEntry<int> cubeExplodeChance = null!;

    public static void Initialize(ConfigFile configFile) {
        cubeExplodeChance = configFile.Bind("Cube", "6. Cube Explode Change", 50, "Chance for the cube to explode.");
    }
}