using System;
using Unity.Netcode;
using UnityEngine;
using EventHandler = DoorBreach.EventHandler;
using Object = UnityEngine.Object;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Items.DoorScrap;

public static class DoorBreachSupport {
    public static DoorScrapScript doorScrapScript = null!;

    public static void Initialize() => EventHandler.doorBreach += DoorBreached;

    public static void DoorBreached(EventHandler.DoorBreachEventArguments arguments) {
        if (!StartOfRound.Instance.IsHost) return;

        var random = new Random((uint) (DateTime.Now.Ticks & 0x0000FFFF));

        var generatedChance = random.NextInt(1, 100);

        if (generatedChance > doorScrapScript.doorSpawnChance.Value) return;

        var itemProperties = doorScrapScript.itemWithDefaultWeight.item!;

        var position = arguments.doorLock.transform.position;

        var spawnPrefab = itemProperties.spawnPrefab!;

        var gameObject = Object.Instantiate(spawnPrefab, position, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
        var doorScrap = gameObject.GetComponent<DoorScrap>();
        doorScrap.transform.rotation = Quaternion.Euler(doorScrap.itemProperties.restingRotation);
        doorScrap.fallTime = 0.0f;

        doorScrap.scrapValue = random.NextInt((int) (itemProperties.minValue * .4), (int) (itemProperties.maxValue * .4));


        var doorType = "SteelDoor";

        var parentParentObject = arguments.doorLock.transform.parent.parent;

        var parentParentObjectName = parentParentObject? parentParentObject.name : null;

        TestAccountVariety.Logger.LogFatal($"Name? {parentParentObjectName}");

        if (!string.IsNullOrWhiteSpace(parentParentObjectName)) {
            if (parentParentObjectName.Contains("YellowMineDoor")) doorType = "YellowMineDoor";

            var tripleParentObject = parentParentObject.parent;

            if (parentParentObjectName.Contains("SteelDoor") && tripleParentObject) {
                parentParentObjectName = tripleParentObject.name;

                if (parentParentObjectName.Contains("FancyDoor")) doorType = "FancyDoor";
            }
        }

        doorScrap.doorType = doorType;

        var networkObject = gameObject.GetComponent<NetworkObject>();
        networkObject.Spawn();
    }
}