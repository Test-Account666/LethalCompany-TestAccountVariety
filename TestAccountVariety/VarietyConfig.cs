using BepInEx.Configuration;

namespace TestAccountVariety;

internal static class VarietyConfig {
    public static ConfigEntry<int> yippeeParticleChance = null!;

    public static ConfigEntry<bool> giftMimicSpawnsOutsideEnemies = null!;
    public static ConfigEntry<int> giftMimicAlternativeVariantChance = null!;
    public static ConfigEntry<string> giftMimicEnemyBlacklist = null!;
    public static ConfigEntry<int> giftMimicScrapChance = null!;
    public static ConfigEntry<string> giftMimicScrapBlacklist = null!;

    public static ConfigEntry<int> bombExplodeChance = null!;

    public static ConfigEntry<int> cubeExplodeChance = null!;

    public static ConfigEntry<int> coloredCubeLightChance = null!;
    public static ConfigEntry<int> coloredCubeRainbowChance = null!;

    public static ConfigEntry<bool> telepadEnableEnemyTeleport = null!;
    public static ConfigEntry<string> telepadEnemyBlacklist = null!;
    public static ConfigEntry<bool> telepadEnemyUsesPower = null!;
    public static ConfigEntry<bool> telepadDropsItems = null!;

    public static ConfigEntry<float> cageMineCoolDown = null!;
    public static ConfigEntry<float> cagePlayerTrapTime = null!;
    public static ConfigEntry<float> cageEnemyTrapTime = null!;
    public static ConfigEntry<bool> useBigEnemyCollider = null!;

    public static ConfigEntry<int> lightSwitchKillChance = null!;
    public static ConfigEntry<int> lightSwitchBatteryUsage = null!;

    public static ConfigEntry<int> laserPlayerDamage = null!;
    public static ConfigEntry<int> laserEnemyDamage = null!;

    public static ConfigEntry<bool> fixTwoHandedWeapons = null!;

    public static void Initialize(ConfigFile configFile) {
        cubeExplodeChance = configFile.Bind("Cube", "6. Cube Explode Chance", 50,
                                            new ConfigDescription("Chance for the cube to explode.", new AcceptableValueRange<int>(0, 100)));


        yippeeParticleChance = configFile.Bind("Yippee", "6. Yippee Particle Chance", 60, "Chance for the yippee scrap to throw confetti.");


        giftMimicSpawnsOutsideEnemies = configFile.Bind("Gift Mimic", "3. Spawns Outside Enemies", false,
                                                        "If set to true, will also spawn outside enemies. Starlancer AI Fix Required!!!.");

        giftMimicAlternativeVariantChance = configFile.Bind("Gift Mimic", "4. Gift Mimic Alternative Variant Chance", 35,
                                                            new ConfigDescription(
                                                                "Defines the chance for the Gift Mimic to use an alternative texture."
                                                              + " Best when combined with UpturnedVariety.", new AcceptableValueRange<int>(0, 100)));

        giftMimicEnemyBlacklist = configFile.Bind("Gift Mimic", "5. Enemy Blacklist", "example1, example2",
                                                  "A comma separated list of blacklisted enemies. " + "Uses startsWith, so you don't need the full name.");

        giftMimicScrapChance = configFile.Bind("Gift Mimic", "6. Scrap Chance", 45,
                                               new ConfigDescription("The chance for spawning scrap instead of enemies", new AcceptableValueRange<int>(0, 100)));

        giftMimicScrapBlacklist = configFile.Bind("Gift Mimic", "7. Scrap Blacklist", "example1, example2",
                                                  "A comma separated list of blacklisted scraps. Uses startsWith, so you don't need the full name.");


        bombExplodeChance = configFile.Bind("Bomb", "6. Bomb Explode Chance", 4,
                                            new ConfigDescription("Chance for the bomb to explode on drop.", new AcceptableValueRange<int>(0, 100)));

        coloredCubeLightChance = configFile.Bind("Colored Cube", "6. Cube Light Chance", 10,
                                                 new ConfigDescription("Chance for the colored cube to have a light.", new AcceptableValueRange<int>(0, 100)));

        coloredCubeRainbowChance = configFile.Bind("Colored Cube", "7. Cube Rainbow Chance", 6,
                                                   new ConfigDescription("Chance for the colored cube to be rainbow.", new AcceptableValueRange<int>(0, 100)));


        telepadEnableEnemyTeleport = configFile.Bind("Telepad", "4. Enable Enemy Teleport", false,
                                                     "If set to true, will allow enemies to use Telepads. Do not use without an AI Fix mod!");

        telepadEnemyBlacklist = configFile.Bind("Telepad", "5. Enemy Blacklist", "example1, example2",
                                                "A comma separated list of blacklisted enemies. Uses startsWith, so you don't need the full name.");

        telepadEnemyUsesPower = configFile.Bind("Telepad", "6. Enemy Uses Power", true, "If set to true, will use battery power to teleport enemies.");

        telepadDropsItems = configFile.Bind("Telepad", "7. Teleport Drops Items", false, "If set to true, will drop all items upon teleport.");


        cageMineCoolDown = configFile.Bind("Cage Mine", "3. Cooldown", 6F,
                                           new ConfigDescription("The amount of time before the cage mine can be triggered again.",
                                                                 new AcceptableValueRange<float>(3F, 12F)));

        cagePlayerTrapTime = configFile.Bind("Cage Mine", "3. Player Trap Time", 12F,
                                             new ConfigDescription("The amount of time a player can be trapped.", new AcceptableValueRange<float>(6F, 24F)));

        cageEnemyTrapTime = configFile.Bind("Cage Mine", "4. Enemy Trap Time", 8F,
                                            new ConfigDescription("The amount of time an enemy can be trapped.", new AcceptableValueRange<float>(6F, 24F)));

        useBigEnemyCollider = configFile.Bind("Cage Mine", "5. Use Big Enemy Collider", false,
                                              "If set to true, will use the same collider as players to trap enemies.");


        lightSwitchKillChance = configFile.Bind("Light Switch", "6. Kill Chance", 5,
                                                new ConfigDescription("The chance using this item will kill you.", new AcceptableValueRange<int>(0, 100)));

        lightSwitchBatteryUsage = configFile.Bind("Light Switch", "7. Battery Usage", 15,
                                                  new ConfigDescription("The battery usage for every use.", new AcceptableValueRange<int>(0, 100)));


        laserPlayerDamage = configFile.Bind("Laser Emitter", "3. Player Damage", 15,
                                            new ConfigDescription("The damage a player receives while inside the laser.", new AcceptableValueRange<int>(1, 100)));
        laserEnemyDamage = configFile.Bind("Laser Emitter", "4. Enemy Damage", 1,
                                           new ConfigDescription("The damage an enemy receives while inside the laser.", new AcceptableValueRange<int>(1, 100)));


        fixTwoHandedWeapons = configFile.Bind("Bug Fixes", "Fix Two Handed Weapons", true,
                                              "If set to true, will fix the two Handed Weapons. "
                                            + "If you swing a two Handed Weapon, you can switch to a different item. This option prevents this behavior.");
    }
}