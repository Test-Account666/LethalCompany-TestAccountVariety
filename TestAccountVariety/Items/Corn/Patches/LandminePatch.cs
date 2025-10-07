using HarmonyLib;
using Unity.Mathematics;
using UnityEngine;

namespace TestAccountVariety.Items.Corn.Patches;

[HarmonyPatch(typeof(Landmine), nameof(Landmine.SpawnExplosion))]
public class LandminePatch {
    [HarmonyPatch]
    [HarmonyPostfix]
    public static void PopCorn(Vector3 explosionPosition, bool spawnExplosionEffect, float damageRange, float killRange) {
        if (StartOfRound.Instance is {
                IsHost: false,
                IsServer: false,
            }) return;

        const int propsLayerMask = 1 << 6;

        var max = math.max(damageRange, killRange);
        if (max <= 0) return;

        var results = new Collider[24];
        var hits = Physics.OverlapSphereNonAlloc(explosionPosition, max, results, propsLayerMask);

        for (var index = 0; index < hits; index++) {
            var result = results[index];

            var grabbableObject = result.GetComponent<GrabbableObject>();

            if (!grabbableObject) continue;
            if (grabbableObject is not CornItem cornItem) continue;

            cornItem.PopCorn();
        }
    }
}