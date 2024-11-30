using GameNetcodeStuff;
using UnityEngine;

namespace TestAccountVariety.ShopItems.Telepad;

public class TelepadTrigger : MonoBehaviour {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Telepad telepad;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public void OnTriggerEnter(Collider other) {
        if (!enabled) return;

        if (!telepad.CanBeUsedForTeleport()) return;

        var hasPlayer = other.TryGetComponent<PlayerControllerB>(out var player);

        if (!hasPlayer) {
            if (telepad is {
                    IsHost: false, IsServer: false,
                }) return;

            TryHandleEnemyTeleport(other);
            return;
        }

        var localPlayer = StartOfRound.Instance.localPlayerController;

        if (player != localPlayer) return;

        telepad.TeleportPlayerServerRpc((int) player.playerClientId);
    }

    public void TryHandleEnemyTeleport(Collider other) {
        var hasEnemy = other.TryGetComponent<EnemyAICollisionDetect>(out var collisionDetect);

        if (!hasEnemy || !collisionDetect.mainScript) return;

        var networkObject = collisionDetect.mainScript.NetworkObject;

        telepad.TeleportEnemyServerRpc(networkObject);
    }
}