using System;
using System.Collections.Generic;
using MonoMod.Utils;
using TestAccountVariety.Config;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Items.Yippee;

public class EvilYippeeParticles : NetworkBehaviour {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ParticleSystem particleSystem;
    public GrabbableObject grabbableObject;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [NonSerialized]
    public static readonly List<NetworkObject> SpawnedPrefabs = [
    ];

    private Random _random;

    public void Start() => _random = new((uint) (DateTime.Now.Ticks & 0x0000FFFF));

    private void OnEnable() {
        if (!grabbableObject.IsOwner) return;
        PlayParticlesServerRpc();
    }

    private void OnDisable() {
        if (!particleSystem) return;
        particleSystem.Stop();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayParticlesServerRpc() {
        try {
            SpawnHazard();
        } catch (Exception exception) {
            exception.LogDetailed();
        }

        var generatedChance = _random.NextInt(1, 100);

        var confetti = generatedChance < YippeeConfig.yippeeParticleChance.Value;

        if (!confetti) return;

        PlayParticlesClientRpc();
    }

    public void SpawnHazard() {
        var generatedChance = _random.NextInt(1, 100);

        var spawnHazard = generatedChance < YippeeConfig.EvilYippeeConfig.evilYippeeHazardSpawnChance.Value;

        if (!spawnHazard) return;

        if (StartOfRound.Instance.inShipPhase) return;

        List<SpawnableMapObject> potentialMapHazards = [
            ..RoundManager.Instance.currentLevel.spawnableMapObjects,
        ];

        potentialMapHazards.RemoveAll(spawnableMapObject => {
            if (!spawnableMapObject.prefabToSpawn) return true;

            var hazardPrefabName = spawnableMapObject.prefabToSpawn.name.ToLower();

            if (hazardPrefabName.Contains("laseremitter")) return false;

            if (hazardPrefabName.Contains("yeetmine")) return false;

            return !hazardPrefabName.Contains("cagemine");
        });

        var hazardToSpawn = potentialMapHazards[_random.NextInt(0, potentialMapHazards.Count)];

        var parent = RoundManager.Instance.mapPropsContainer.transform;

        var hazard = Instantiate(hazardToSpawn.prefabToSpawn, grabbableObject.playerHeldBy.transform.position, Quaternion.identity, parent);

        var networkObject = hazard.GetComponent<NetworkObject>();

        networkObject.Spawn();

        SpawnedPrefabs.Add(networkObject);
    }

    [ClientRpc]
    public void PlayParticlesClientRpc() {
        if (!particleSystem) return;

        particleSystem.time = 0F;
        particleSystem.Play();
    }
}