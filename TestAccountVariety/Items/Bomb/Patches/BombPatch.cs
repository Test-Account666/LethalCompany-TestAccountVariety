using HarmonyLib;
using UnityEngine;

namespace TestAccountVariety.Items.Bomb.Patches;

[HarmonyPatch(typeof(StunGrenadeItem))]
public static class BombPatch {
    // I hate C# for not being able to just override methods...
    [HarmonyPatch(nameof(StunGrenadeItem.GetGrenadeThrowDestination))]
    [HarmonyPrefix]
    // ReSharper disable InconsistentNaming
    private static bool RedirectGetGrenadeThrowDestination(StunGrenadeItem __instance, ref Vector3 __result) {
        if (__instance is not ThrowableBombItem bomb) return true;

        __result = bomb.GetGrenadeThrowDestination();
        return false;
    }

    // I hate C# for not being able to just override methods...
    [HarmonyPatch(nameof(StunGrenadeItem.ExplodeStunGrenade))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static bool RedirectExplodeStunGrenade(StunGrenadeItem __instance, bool destroy) {
        if (__instance is not ThrowableBombItem bomb) return true;

        bomb.ExplodeStunGrenade(destroy);
        return false;
    }
}