using System.Linq;
using BepInEx.Bootstrap;

namespace TestAccountVariety.Dependencies;

public static class DependencyChecker {
    public static bool IsDoorBreachInstalled() =>
        Chainloader.PluginInfos.Values.Any(metadata => metadata.Metadata.GUID.Contains("TestAccount666.DoorBreach"));
}