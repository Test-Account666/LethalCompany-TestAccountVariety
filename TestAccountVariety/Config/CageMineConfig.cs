using BepInEx.Configuration;

namespace TestAccountVariety.Config;

// ReSharper disable once ClassNeverInstantiated.Global
internal class CageMineConfig : VarietyConfig {
    public static ConfigEntry<float> cageMineCoolDown = null!;
    public static ConfigEntry<float> cagePlayerTrapTime = null!;
    public static ConfigEntry<float> cageEnemyTrapTime = null!;
    public static ConfigEntry<bool> useBigEnemyCollider = null!;

    public override void Initialize(ConfigFile configFile) {
        cageMineCoolDown = configFile.BindFloat("Cage Mine", "3. Cooldown", 6F, "The amount of time before the cage mine can be triggered again.", 3F, 12F);

        cagePlayerTrapTime = configFile.BindFloat("Cage Mine", "3. Player Trap Time", 12F, "The amount of time a player can be trapped.", 6F, 24F);

        cageEnemyTrapTime = configFile.BindFloat("Cage Mine", "4. Enemy Trap Time", 8F, "The amount of time an enemy can be trapped.", 6F, 24F);

        useBigEnemyCollider = configFile.BindBool("Cage Mine", "5. Use Big Enemy Collider", false,
                                                  "If set to true, will use the same collider as players to trap enemies.");
    }
}