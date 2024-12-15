using BepInEx.Configuration;

namespace TestAccountVariety.Config;

// ReSharper disable once ClassNeverInstantiated.Global
internal class CubeConfig : VarietyConfig {
    public static ConfigEntry<int> cubeExplodeChance = null!;

    public override void Initialize(ConfigFile configFile) {
        cubeExplodeChance = configFile.BindInt("Cube", "6. Cube Explode Chance", 50, "Chance for the cube to explode.");
    }

    internal class ColoredCubeConfig : VarietyConfig {
        public static ConfigEntry<int> coloredCubeLightChance = null!;
        public static ConfigEntry<int> coloredCubeRainbowChance = null!;

        public override void Initialize(ConfigFile configFile) {
            coloredCubeLightChance = configFile.BindInt("Colored Cube", "6. Cube Light Chance", 10, "Chance for the colored cube to have a light.");

            coloredCubeRainbowChance = configFile.BindInt("Colored Cube", "7. Cube Rainbow Chance", 6, "Chance for the colored cube to be rainbow.");
        }
    }
}