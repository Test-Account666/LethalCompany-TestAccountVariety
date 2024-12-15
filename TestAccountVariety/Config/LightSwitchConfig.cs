using BepInEx.Configuration;

namespace TestAccountVariety.Config;

// ReSharper disable once ClassNeverInstantiated.Global
internal class LightSwitchConfig : VarietyConfig {
    public static ConfigEntry<int> lightSwitchKillChance = null!;
    public static ConfigEntry<int> lightSwitchBatteryUsage = null!;

    public override void Initialize(ConfigFile configFile) {
        lightSwitchKillChance = configFile.BindInt("Light Switch", "6. Kill Chance", 5, "The chance using this item will kill you.");

        lightSwitchBatteryUsage = configFile.BindInt("Light Switch", "7. Battery Usage", 15, "The battery usage for every use.");
    }
}