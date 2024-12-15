using System.Linq;
using HarmonyLib;

namespace TestAccountVariety.Items.Yippee.Patches;

[HarmonyPatch(typeof(RoundManager))]
public class RoundManagerPatch {
    [HarmonyPatch(nameof(RoundManager.UnloadSceneObjectsEarly))]
    [HarmonyPostfix]
    public static void CleanEvilYippeeSpawns() {
        if (RoundManager.Instance is {
                IsHost: false,
                IsServer: false,
            }) return;

        foreach (var spawnedPrefab in EvilYippeeParticles.SpawnedPrefabs.Where(spawnedPrefab => spawnedPrefab)) spawnedPrefab.Despawn();

        EvilYippeeParticles.SpawnedPrefabs.Clear();
    }
}