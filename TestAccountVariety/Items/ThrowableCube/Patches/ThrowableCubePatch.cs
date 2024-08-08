using HarmonyLib;

namespace TestAccountVariety.Items.ThrowableCube.Patches;

[HarmonyPatch(typeof(StunGrenadeItem))]
public static class ThrowableCubePatch {
    // I hate C# for not being able to just override methods...
    [HarmonyPatch(nameof(StunGrenadeItem.ExplodeStunGrenade))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static bool RedirectExplodeStunGrenade(StunGrenadeItem __instance) {
        if (__instance is not ThrowableCube throwableCube) return true;

        throwableCube.ExplodeServerRpc();
        return false;
    }
}