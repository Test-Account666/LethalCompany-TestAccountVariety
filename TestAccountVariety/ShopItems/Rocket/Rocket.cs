using System;
using System.Collections;
using GameNetcodeStuff;
using TestAccountVariety.Utils;
using Unity.Netcode;
using UnityEngine;
using static TestAccountVariety.Utils.ReferenceResolver;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.ShopItems.Rocket;

public class Rocket : GrabbableObject {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public ParticleSystem[] particles;
    public ParticleSystem sparkParticles;
    public MeshRenderer meshRenderer;

    public AudioSource flightSource;
    public AudioSource explosionSource;
    public AudioSource indoorExplosionSource;

    public AudioClip[] explosionClips;
    public AudioClip[] indoorExplosionClips;

    public BoxCollider boxCollider;

    public float flightSpeed = 30F;

    private bool _isFlying;
    private bool _collided;

    private PlayerControllerB _launcher;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public override void ItemActivate(bool used, bool buttonDown = true) {
        base.ItemActivate(used, buttonDown);

        if (!playerHeldBy) return;

        var localPlayer = StartOfRound.Instance.localPlayerController;

        if (playerHeldBy != localPlayer) return;

        var rotation = transform.rotation.eulerAngles;

        localPlayer.StartCoroutine(localPlayer.waitToEndOfFrameToDiscard());
        ShootRocketServerRpc((int) localPlayer.playerClientId, rotation);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShootRocketServerRpc(int playerWhoLaunched, Vector3 rotation) {
        var random = new Random((uint) (DateTime.Now.Ticks + transform.position.ConvertToInt()));

        var particleIndex = random.NextInt(0, particles.Length);

        ShootRocketClientRpc(playerWhoLaunched, rotation, particleIndex);
    }

    [ClientRpc]
    public void ShootRocketClientRpc(int playerWhoLaunched, Vector3 rotation, int particleIndex) {
        var hasPlayer = TryGetPlayer(playerWhoLaunched, out var player);

        if (!hasPlayer) return;
        _launcher = player;

        transform.rotation = Quaternion.Euler(rotation);

        var particle = particles[particleIndex];

        StartCoroutine(ShootRocket(particle, transform.rotation, transform.up));
    }

    public IEnumerator ShootRocket(ParticleSystem particle, Quaternion rotation, Vector3 direction) {
        _isFlying = true;

        var targetPosition = transform.position + direction * 1000;

        flightSource.Play();

        sparkParticles.Play();
        var endTime = Time.time + 1.4F;

        while (!_collided && Time.time <= endTime) {
            yield return null;

            hasHitGround = false;
            targetFloorPosition = Vector3.MoveTowards(targetFloorPosition, targetPosition, flightSpeed * Time.deltaTime);
            transform.rotation = rotation;
        }

        var random = new Random((uint) (DateTime.Now.Ticks + transform.position.ConvertToInt()));

        var explosionIndex = random.NextInt(0, explosionClips.Length);
        explosionSource.clip = explosionClips[explosionIndex];
        indoorExplosionSource.clip = indoorExplosionClips[explosionIndex];

        flightSource.Stop();
        explosionSource.Play();
        indoorExplosionSource.Play();
        RoundManager.Instance.PlayAudibleNoise(transform.position, 50f, 5f);

        sparkParticles.Stop();
        particle.Play();

        Landmine.SpawnExplosion(transform.position, killRange: 5f, damageRange: 10f, nonLethalDamage: 75, physicsForce: 35f, goThroughCar: true);

        grabbable = false;
        deactivated = true;

        meshRenderer.gameObject.SetActive(false);

        yield return new WaitUntil(() => !particle.isPlaying);

        if (!IsHost && !IsServer) yield break;

        NetworkObject.Despawn();
    }

    private const int _COLLISION_LAYER_MASK = 1 << 3 | 1 << 8 | 1 << 11 | 1 << 19 | 1 << 24 | 1 << 25 | 1 << 28 | 1 << 30;

    public override void Update() {
        base.Update();

        if (!_isFlying) return;

        var results = new Collider[16];


        var size = Physics.OverlapSphereNonAlloc(transform.position, .25f, results, _COLLISION_LAYER_MASK);

        for (var index = 0; index < size; index++) {
            var hit = results[index];

            if (hit.gameObject == gameObject) continue;

            if (hit.gameObject.layer == 3) {
                var hasPlayer = hit.TryGetComponent<PlayerControllerB>(out var player);

                if (hasPlayer && player.playerClientId == _launcher.playerClientId) continue;
            }

            _collided = true;
            break;
        }
    }
}