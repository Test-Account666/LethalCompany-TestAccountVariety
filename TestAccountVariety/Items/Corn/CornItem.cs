using System;
using TestAccountCore;
using TestAccountVariety.Items.Popcorn;
using TestAccountVariety.Utils;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Items.Corn;

public class CornItem : PhysicsProp {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public ItemWithDefaultWeight popcornWeight;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public void PopCorn() {
        if (!IsHost && !IsServer) return;
        if (!popcornWeight.isRegistered) return;

        var random = new Random((uint)(DateTime.Now.Ticks & 0x0000FFFF));

        var spawnedProperties = popcornWeight.item!;
        var position = transform.position;
        var spawnPrefab = spawnedProperties.spawnPrefab!;

        var spawnedObject = Instantiate(spawnPrefab, position, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
        var popCorn = spawnedObject.GetComponent<PopcornItem>();
        popCorn.transform.rotation = Quaternion.Euler(popCorn.itemProperties.restingRotation);
        popCorn.fallTime = 0.0f;

        popCorn.scrapValue = random.NextInt((int)(spawnedProperties.minValue * .4), (int)(spawnedProperties.maxValue * .4));

        var networkObject = spawnedObject.GetComponent<NetworkObject>();
        networkObject.Spawn();

        DestroyItemClientRpc();
    }

    [ClientRpc]
    public void DestroyItemClientRpc() => this.DestroyItem();
}