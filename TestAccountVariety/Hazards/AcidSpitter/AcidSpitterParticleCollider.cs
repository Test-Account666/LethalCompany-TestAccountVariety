using System;
using System.Collections.Generic;
using GameNetcodeStuff;
using TestAccountVariety.Config;
using UnityEngine;
using static TestAccountVariety.Utils.ReferenceResolver;

namespace TestAccountVariety.Hazards.AcidSpitter;

public class AcidSpitterParticleCollider : MonoBehaviour {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Hazards.AcidSpitter.AcidSpitter laserEmitter;

    public ParticleSystem particleSystem;

    public BoxCollider boxCollider;
    public AudioSource acidAudio;

    public float colliderUpdateCooldown;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public float nextDamage;
    public float nextEnemyDamage;

    private float _nextColliderUpdate;

    private readonly List<ParticleCollisionEvent> _collisionEvents = new(10);

    private void Update() {
        nextDamage = Math.Max(0, nextDamage - Time.deltaTime);
        nextEnemyDamage = Math.Max(0, nextEnemyDamage - Time.deltaTime);

        _nextColliderUpdate = Math.Max(0, _nextColliderUpdate - Time.deltaTime);
    }

    public void OnParticleCollision(GameObject other) {
        if (_nextColliderUpdate > 0) return;

        _collisionEvents.Clear();
        particleSystem.GetCollisionEvents(other, _collisionEvents);

        if (_collisionEvents.Count <= 0) return;

        var collisionEvent = _collisionEvents[0];
        boxCollider.center = transform.InverseTransformPoint(collisionEvent.intersection);
        acidAudio.transform.position = collisionEvent.intersection;

        _nextColliderUpdate = colliderUpdateCooldown;
    }

    private void OnTriggerStay(Collider other) {
        HandlePlayerDamage(other);
        HandleEnemyDamage(other);
    }


    public void HandleEnemyDamage(Collider other) {
        if (nextEnemyDamage > 0) return;

        if (laserEmitter is {
                IsHost: false, IsServer: false,
            }) return;

        var hasEnemy = TryGetEnemy(other, out var enemyAI);

        if (!hasEnemy) return;

        TestAccountVariety.Logger.LogDebug($"Laser hitting Enemy: {enemyAI.enemyType.enemyName} ({enemyAI})");

        enemyAI.HitEnemyOnLocalClient(AcidSpitterConfig.acidEnemyDamage.Value, Vector3.down, playHitSFX: true);
        nextEnemyDamage = AcidSpitterConfig.acidEnemyCoolDown.Value;
    }

    public void HandlePlayerDamage(Collider other) {
        if (nextDamage > 0) return;

        var localPlayer = StartOfRound.Instance.localPlayerController;

        var hasPlayer = other.TryGetComponent<PlayerControllerB>(out var player);

        hasPlayer = hasPlayer && localPlayer == player;

        if (!hasPlayer) return;

        player.DamagePlayer(AcidSpitterConfig.acidPlayerDamage.Value, causeOfDeath: CauseOfDeath.Burning, deathAnimation: 6);
        nextDamage = AcidSpitterConfig.acidPlayerCoolDown.Value;
    }
}