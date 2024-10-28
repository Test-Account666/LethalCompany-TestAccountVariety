using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TestAccountCore.Dependencies;
using TestAccountCore.Dependencies.Compatibility;
using TestAccountVariety.Items.ThrowableCube.Patches;
using TestAccountVariety.Patches;
using static TestAccountCore.AssetLoader;
using static TestAccountCore.Netcode;

namespace TestAccountVariety;

[BepInDependency("TestAccount666.DoorBreach", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("evaisa.lethallib")]
[BepInDependency("TestAccount666.TestAccountCore", "1.9.0")]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class TestAccountVariety : BaseUnityPlugin {
    public static TestAccountVariety Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    internal static void Patch() {
        Harmony ??= new(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll(typeof(ThrowableCubePatch));
        Harmony.PatchAll(typeof(ShovelPatch));

        Logger.LogDebug("Finished patching!");
    }

    private void Awake() {
        Logger = base.Logger;
        Instance = this;

        if (DependencyChecker.IsLobbyCompatibilityInstalled()) {
            Logger.LogInfo("Found LobbyCompatibility Mod, initializing support :)");
            LobbyCompatibilitySupport.Initialize(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, CompatibilityLevel.Everyone,
                                                 VersionStrictness.Minor);
        }

        var assembly = Assembly.GetExecutingAssembly();

        ExecuteNetcodePatcher(assembly);

        LoadBundle(assembly, "TestAccountVariety");
        LoadItems(Config);
        LoadHazards(Config);
        LoadUnlockables(Config);
        LoadCustomScripts(Config);

        VarietyConfig.Initialize(Config);

        Patch();


        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Unpatch() {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}