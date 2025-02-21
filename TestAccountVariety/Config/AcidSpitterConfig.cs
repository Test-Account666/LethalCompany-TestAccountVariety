using BepInEx.Configuration;

namespace TestAccountVariety.Config;

// ReSharper disable once ClassNeverInstantiated.Global
internal class AcidSpitterConfig : VarietyConfig {
    public static ConfigEntry<int> acidPlayerDamage = null!;
    public static ConfigEntry<int> acidEnemyDamage = null!;
    public static ConfigEntry<float> acidPlayerCoolDown = null!;
    public static ConfigEntry<float> acidEnemyCoolDown = null!;
    public static ConfigEntry<float> spitterMinimumMovementSpeed = null!;
    public static ConfigEntry<float> spitterMaximumMovementSpeed = null!;
    public static ConfigEntry<int> acidColorRed = null!;
    public static ConfigEntry<int> acidColorGreen = null!;
    public static ConfigEntry<int> acidColorBlue = null!;

    public override void Initialize(ConfigFile configFile) {
        acidPlayerDamage = configFile.BindInt("Acid Spitter", "3. Player Damage", 15, "The damage a player receives while inside the laser.", 1);
        acidEnemyDamage = configFile.BindInt("Acid Spitter", "4. Enemy Damage", 1, "The damage an enemy receives while inside the laser.");

        acidPlayerCoolDown = configFile.BindFloat("Acid Spitter", "5. Player Damage Cooldown", .2F, "The cooldown between damage for players.");
        acidEnemyCoolDown = configFile.BindFloat("Acid Spitter", "6. Enemy Damage Cooldown", .5F, "The cooldown between damage for enemies.");

        spitterMinimumMovementSpeed = configFile.BindFloat("Acid Spitter", "7. Minimum Movement Speed", .6F, "Minimum speed the laser will move at");
        spitterMaximumMovementSpeed = configFile.BindFloat("Acid Spitter", "8. Maximum Movement Speed", 1.6F, "Maximum speed the laser will move at", max: 2F);

        acidColorRed = configFile.BindInt("Acid Spitter", "9. Acid Color Red", 255, "Red color for acid", max: 255);
        acidColorGreen = configFile.BindInt("Acid Spitter", "10. Acid Color Green", 0, "Green color for acid", max: 255);
        acidColorBlue = configFile.BindInt("Acid Spitter", "11. Acid Color Blue", 0, "Blue color for acid", max: 255);
    }
}