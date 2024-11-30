using HarmonyLib;
using UnityEngine;

namespace TestAccountVariety.Items.ThrowableCube.Patches;

[HarmonyPatch(typeof(StunGrenadeItem))]
public static class ThrowableCubePatch {
    // I hate C# for not being able to just override methods...
    [HarmonyPatch(nameof(StunGrenadeItem.GetGrenadeThrowDestination))]
    [HarmonyPrefix]
    // ReSharper disable InconsistentNaming
    private static bool RedirectGetGrenadeThrowDestination(StunGrenadeItem __instance, ref Vector3 __result) {
        if (__instance is not ThrowableCube throwableCube) return true;

        __result = throwableCube.GetGrenadeThrowDestination();
        return false;
    }

    // I hate C# for not being able to just override methods...
    [HarmonyPatch(nameof(StunGrenadeItem.ExplodeStunGrenade))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static bool RedirectExplodeStunGrenade(StunGrenadeItem __instance) {
        if (__instance is not ThrowableCube throwableCube) return true;

        if (!throwableCube.IsOwner) return false;
        if (!throwableCube.grabbable) return false;

        throwableCube.ExplodeServerRpc();
        return false;
    }
}