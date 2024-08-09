using System;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Items.Yippee;

public class YippeeParticles : NetworkBehaviour {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ParticleSystem particleSystem;
    public GrabbableObject grabbableObject;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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
        var generatedChance = _random.NextInt(1, 100);

        var confetti = generatedChance < VarietyConfig.yippeeParticleChance.Value;

        if (!confetti) return;

        PlayParticlesClientRpc();
    }

    [ClientRpc]
    public void PlayParticlesClientRpc() {
        if (!particleSystem) return;

        particleSystem.time = 0F;
        particleSystem.Play();
    }
}