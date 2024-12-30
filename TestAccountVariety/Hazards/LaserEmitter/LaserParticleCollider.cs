using System;
using GameNetcodeStuff;
using TestAccountVariety.Config;
using UnityEngine;

namespace TestAccountVariety.Hazards.LaserEmitter;

public class LaserParticleCollider : MonoBehaviour {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public LaserEmitter laserEmitter;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public float nextDamage;
    public float nextEnemyDamage;

    private void Update() {
        nextDamage = Math.Max(0, nextDamage - Time.deltaTime);
        nextEnemyDamage = Math.Max(0, nextEnemyDamage - Time.deltaTime);
    }

    public void OnParticleCollision(GameObject other) {
        HandleEnemyDamage(other);
        HandlePlayerDamage(other);
    }

    public void HandleEnemyDamage(GameObject other) {
        if (nextEnemyDamage > 0) return;

        if (laserEmitter is {
                IsHost: false, IsServer: false,
            }) return;

        var hasEnemyCollider = other.TryGetComponent<EnemyAICollisionDetect>(out var enemyCollider);

        if (!hasEnemyCollider) return;

        var enemyAI = enemyCollider.mainScript;

        if (!enemyAI || enemyAI.isEnemyDead) return;

        TestAccountVariety.Logger.LogDebug($"Laser hitting Enemy: {enemyAI.enemyType.enemyName} ({enemyAI})");

        enemyAI.HitEnemyOnLocalClient(LaserEmitterConfig.laserEnemyDamage.Value, Vector3.down, playHitSFX: true);
        nextEnemyDamage = LaserEmitterConfig.laserEnemyCoolDown.Value;
    }

    public void HandlePlayerDamage(GameObject other) {
        if (nextDamage > 0) return;

        var localPlayer = StartOfRound.Instance.localPlayerController;

        var hasPlayer = other.TryGetComponent<PlayerControllerB>(out var player);

        hasPlayer = hasPlayer && localPlayer == player;

        if (!hasPlayer) return;

        player.DamagePlayer(LaserEmitterConfig.laserPlayerDamage.Value, causeOfDeath: CauseOfDeath.Burning, deathAnimation: 6);
        nextDamage = LaserEmitterConfig.laserPlayerCoolDown.Value;
    }
}