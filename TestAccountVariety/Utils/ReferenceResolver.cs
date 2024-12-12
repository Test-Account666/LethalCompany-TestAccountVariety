using GameNetcodeStuff;
using TestAccountVariety.ShopItems.Telepad;
using Unity.Netcode;
using UnityEngine;

namespace TestAccountVariety.Utils;

public static class ReferenceResolver {
    public static bool TryGetTelepad(NetworkObjectReference telepadReference, out Telepad telepad) {
        var networkObject = (NetworkObject) telepadReference;

        if (networkObject) return networkObject.TryGetComponent(out telepad);

        telepad = null!;
        return false;
    }

    public static bool TryGetPlayer(int playerIndex, out PlayerControllerB player) {
        if (playerIndex < 0) {
            player = null!;
            return false;
        }

        if (playerIndex >= StartOfRound.Instance.allPlayerScripts.Length) {
            player = null!;
            return false;
        }

        player = StartOfRound.Instance.allPlayerScripts[playerIndex];
        return player;
    }

    public static bool TryGetEnemy(Collider collider, out EnemyAI enemyAI) {
        var hasEnemy = collider.TryGetComponent<EnemyAICollisionDetect>(out var collisionDetect);

        if (!hasEnemy || !collisionDetect.mainScript) {
            enemyAI = null!;
            return false;
        }

        enemyAI = collisionDetect.mainScript;
        return true;
    }

    public static bool TryGetEnemy(NetworkObjectReference enemyReference, out EnemyAI enemyAI) {
        var networkObject = (NetworkObject) enemyReference;

        if (networkObject) return networkObject.TryGetComponent(out enemyAI);

        enemyAI = null!;
        return false;
    }
}