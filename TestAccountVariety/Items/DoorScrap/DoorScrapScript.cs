using System;
using BepInEx.Configuration;
using TestAccountCore;
using TestAccountVariety.Dependencies;
using UnityEngine;

namespace TestAccountVariety.Items.DoorScrap;

[CreateAssetMenu(menuName = "TestAccountVariety/DoorScrapScript", order = 1)]
public class DoorScrapScript : CustomScript {
    public ConfigEntry<int> doorSpawnChance = null!;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    // ReSharper disable once UnassignedField.Global
    public ItemWithDefaultWeight itemWithDefaultWeight;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public override void Initialize(ConfigFile? configFile) {
        DontDestroyOnLoad(itemWithDefaultWeight);

        configFile ??= TestAccountVariety.Instance.Config;

        if (itemWithDefaultWeight.item == null) throw new ArgumentNullException(nameof(itemWithDefaultWeight.item), "ItemProperties cannot be null!");

        var itemName = itemWithDefaultWeight.item.itemName;

        var canItemSpawn = configFile.Bind($"{itemName}", "1. Enabled", true,
                                           $"If false, {itemName} will not be registered. This is different from a spawn weight of 0!");

        if (!canItemSpawn.Value) return;

        doorSpawnChance = configFile.Bind($"{itemName}", "6. Spawn Chance", 35,
                                          new ConfigDescription("If DoorBreach is installed, you can set a spawn chance here.",
                                                                new AcceptableValueRange<int>(0, 100)));

        if (!DependencyChecker.IsDoorBreachInstalled()) return;

        DoorBreachSupport.doorScrapScript = this;
        DoorBreachSupport.Initialize();
    }
}