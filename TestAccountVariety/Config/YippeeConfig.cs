using BepInEx.Configuration;

namespace TestAccountVariety.Config;

// ReSharper disable once ClassNeverInstantiated.Global
internal class YippeeConfig : VarietyConfig {
    public static ConfigEntry<int> yippeeParticleChance = null!;

    public override void Initialize(ConfigFile configFile) {
        yippeeParticleChance = configFile.BindInt("Yippee", "6. Yippee Particle Chance", 60, "Chance for the yippee scrap to throw confetti.");
    }

// ReSharper disable once ClassNeverInstantiated.Global
    public class EvilYippeeConfig : VarietyConfig {
        public static ConfigEntry<int> evilYippeeHazardSpawnChance = null!;

        public override void Initialize(ConfigFile configFile) {
            evilYippeeHazardSpawnChance = configFile.BindInt("Evil Yippee", "6. Map Hazard Spawn Chance", 100,
                                                             "The chance of Evil Yippee spawning map hazards. (Only spawns hazards from this mod!)");
        }
    }
}