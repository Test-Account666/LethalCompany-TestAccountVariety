using HarmonyLib;
using TestAccountVariety.Config;

namespace TestAccountVariety.Patches;

[HarmonyPatch(typeof(Shovel))]
public static class ShovelPatch {
    [HarmonyPatch(nameof(Shovel.HitShovel))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void FixTwoHandedShovels(Shovel __instance, bool cancel) {
        if (!BugFixConfig.fixTwoHandedWeapons.Value || cancel) return;

        __instance.previousPlayerHeldBy.twoHanded = __instance.itemProperties.twoHanded;
    }
}