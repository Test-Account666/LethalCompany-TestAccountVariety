using BepInEx.Configuration;

namespace TestAccountVariety.Config;

// ReSharper disable once ClassNeverInstantiated.Global
internal class TelepadConfig : VarietyConfig {
    public static ConfigEntry<bool> telepadEnableEnemyTeleport = null!;
    public static ConfigEntry<string> telepadEnemyBlacklist = null!;
    public static ConfigEntry<bool> telepadEnemyUsesPower = null!;
    public static ConfigEntry<bool> telepadDropsItems = null!;

    public override void Initialize(ConfigFile configFile) {
        telepadEnableEnemyTeleport = configFile.BindBool("Telepad", "4. Enable Enemy Teleport", false,
                                                         "If set to true, will allow enemies to use Telepads. Do not use without an AI Fix mod!");

        telepadEnemyBlacklist = configFile.BindString("Telepad", "5. Enemy Blacklist", "example1, example2",
                                                      "A comma separated list of blacklisted enemies. Uses startsWith, so you don't need the full name.");

        telepadEnemyUsesPower = configFile.BindBool("Telepad", "6. Enemy Uses Power", true, "If set to true, will use battery power to teleport enemies.");

        telepadDropsItems = configFile.BindBool("Telepad", "7. Teleport Drops Items", false, "If set to true, will drop all items upon teleport.");
    }
}