using BepInEx.Configuration;

namespace TestAccountVariety.Config;

// ReSharper disable once ClassNeverInstantiated.Global
internal class LaserEmitterConfig : VarietyConfig {
    public static ConfigEntry<int> laserPlayerDamage = null!;
    public static ConfigEntry<int> laserEnemyDamage = null!;
    public static ConfigEntry<float> laserPlayerCoolDown = null!;
    public static ConfigEntry<float> laserEnemyCoolDown = null!;
    public static ConfigEntry<float> laserMinimumMovementSpeed = null!;
    public static ConfigEntry<float> laserMaximumMovementSpeed = null!;

    public override void Initialize(ConfigFile configFile) {
        laserPlayerDamage = configFile.BindInt("Laser Emitter", "3. Player Damage", 15, "The damage a player receives while inside the laser.", 1);

        laserEnemyDamage = configFile.BindInt("Laser Emitter", "4. Enemy Damage", 1, "The damage an enemy receives while inside the laser.");

        laserPlayerCoolDown = configFile.BindFloat("Laser Emitter", "5. Player Damage Cooldown", .2F, "The cooldown between damage for players.");

        laserEnemyCoolDown = configFile.BindFloat("Laser Emitter", "6. Enemy Damage Cooldown", .5F, "The cooldown between damage for enemies.");

        laserMinimumMovementSpeed = configFile.BindFloat("Laser Emitter", "7. Minimum Movement Speed", .6F, "Minimum speed the laser will move at");

        laserMaximumMovementSpeed = configFile.BindFloat("Laser Emitter", "7. Maximum Movement Speed", 1.6F, "Maximum speed the laser will move at", max: 2F);
    }
}