using System;
using GameNetcodeStuff;
using TestAccountVariety.Config;
using UnityEngine;
using static TestAccountVariety.Utils.ReferenceResolver;

namespace TestAccountVariety.Hazards.LaserEmitter;

public class LaserCollider : MonoBehaviour {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public LaserEmitter laserEmitter;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public float nextDamage;
    public float nextEnemyDamage;

    private void Update() {
        nextDamage = Math.Max(0, nextDamage - Time.deltaTime);
        nextEnemyDamage = Math.Max(0, nextEnemyDamage - Time.deltaTime);
    }

    public void OnTriggerStay(Collider other) {
        if (!laserEmitter.valuesSynced || !laserEmitter.laser.activeSelf) return;

        HandleEnemyDamage(other);
        HandlePlayerDamage(other);
    }

    public void HandleEnemyDamage(Collider collider) {
        if (nextEnemyDamage > 0) return;

        if (laserEmitter is {
                IsHost: false, IsServer: false,
            }) return;

        var hasEnemy = TryGetEnemy(collider, out var enemyAI);

        if (!hasEnemy || enemyAI.isEnemyDead) return;

        TestAccountVariety.Logger.LogDebug($"Laser hitting Enemy: {enemyAI.enemyType.enemyName} ({enemyAI})");

        enemyAI.HitEnemyClientRpc(LaserEmitterConfig.laserEnemyDamage.Value, -1, true);
        nextEnemyDamage = LaserEmitterConfig.laserEnemyCoolDown.Value;
    }

    public void HandlePlayerDamage(Collider collider) {
        if (nextDamage > 0) return;

        var localPlayer = StartOfRound.Instance.localPlayerController;

        var hasPlayer = collider.TryGetComponent<PlayerControllerB>(out var player);

        hasPlayer = hasPlayer && localPlayer == player;

        if (!hasPlayer) return;

        player.DamagePlayer(LaserEmitterConfig.laserPlayerDamage.Value, causeOfDeath: CauseOfDeath.Burning, deathAnimation: 6);
        nextDamage = LaserEmitterConfig.laserPlayerCoolDown.Value;
    }
}