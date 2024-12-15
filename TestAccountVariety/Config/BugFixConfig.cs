using BepInEx.Configuration;

namespace TestAccountVariety.Config;

// ReSharper disable once ClassNeverInstantiated.Global
internal class BugFixConfig : VarietyConfig {
    public static ConfigEntry<bool> fixTwoHandedWeapons = null!;

    public override void Initialize(ConfigFile configFile) {
        fixTwoHandedWeapons = configFile.BindBool("Bug Fixes", "Fix Two Handed Weapons", true,
                                                  "If set to true, will fix the two Handed Weapons. "
                                                + "If you swing a two Handed Weapon, you can switch to a different item. This option prevents this behavior.");
    }
}