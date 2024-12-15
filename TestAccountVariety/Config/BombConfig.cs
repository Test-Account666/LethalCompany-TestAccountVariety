using BepInEx.Configuration;

namespace TestAccountVariety.Config;

// ReSharper disable once ClassNeverInstantiated.Global
internal class BombConfig : VarietyConfig {
    public static ConfigEntry<int> bombExplodeChance = null!;

    public override void Initialize(ConfigFile configFile) {
        bombExplodeChance = configFile.BindInt("Bomb", "6. Bomb Explode Chance", 4, "Chance for the bomb to explode on drop.");
    }
}