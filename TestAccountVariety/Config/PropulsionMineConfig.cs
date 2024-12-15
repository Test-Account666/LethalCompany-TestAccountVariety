using BepInEx.Configuration;

namespace TestAccountVariety.Config;

// ReSharper disable once ClassNeverInstantiated.Global
internal class PropulsionMineConfig : VarietyConfig {
    public static ConfigEntry<int> propulsionMinePlayerDamage = null!;

    public override void Initialize(ConfigFile configFile) {
        propulsionMinePlayerDamage = configFile.BindInt("Propulsion Mine", "3. Player Damage", 20, "The damage players receive from propulsion mines.");
    }
}