using System;
using System.Linq;
using GameNetcodeStuff;
using TestAccountVariety.Utils;
using Unity.Netcode;
using UnityEngine;
using static TestAccountVariety.Utils.ReferenceResolver;
using Random = System.Random;

namespace TestAccountVariety.ShopItems.Telepad;

public class TelepadTrigger : MonoBehaviour {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Telepad telepad;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public void OnTriggerEnter(Collider other) {
        if (!enabled) return;

        if (!telepad.CanTeleport()) return;

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

        var availableTelepads = telepad.GetAvailableTelepads();

        if (availableTelepads.Count <= 0) {
            HUDManager.Instance.DisplayTip("Telepad", "No teleportation points found!", true);
            return;
        }

        availableTelepads.RemoveAll(otherTelepad => Vector3.Distance(otherTelepad.transform.position, telepad.transform.position) <= 5F);

        if (availableTelepads.Count <= 0) {
            HUDManager.Instance.DisplayTip("Telepad", "Walk, you lazy ass!", true);
            return;
        }

        var random = new Random((int) DateTime.Now.Ticks + telepad.teleportationPoint.position.ConvertToInt());

        var targetTelepad = availableTelepads[random.Next(0, availableTelepads.Count)];

        targetTelepad.TeleportOnLocalClient(telepad, player);

        var telepadReference = (NetworkObjectReference) telepad.NetworkObject;
        targetTelepad.TeleportPlayerServerRpc((int) player.playerClientId, telepadReference);
    }

    public void TryHandleEnemyTeleport(Collider other) {
        if (!VarietyConfig.telepadEnableEnemyTeleport.Value) return;

        var hasEnemy = TryGetEnemy(other, out var enemyAI);

        if (!hasEnemy) return;

        var blackList = VarietyConfig.telepadEnemyBlacklist.Value.Replace(", ", ",").Split(",").ToHashSet();

        if (blackList.Any(blacklistedEnemy => enemyAI.enemyType.enemyName.ToLower().StartsWith(blacklistedEnemy.ToLower()))) return;

        var availableTelepads = telepad.GetAvailableTelepads();

        if (availableTelepads.Count <= 0) {
            TestAccountVariety.Logger.LogDebug("[Enemy] No teleportation points found!");
            return;
        }

        var random = new Random((int) DateTime.Now.Ticks);

        var targetTelepad = availableTelepads[random.Next(0, availableTelepads.Count)];

        var networkObject = enemyAI.NetworkObject;

        var telepadReference = (NetworkObjectReference) telepad.NetworkObject;

        targetTelepad.TeleportEnemyClientRpc(telepadReference, (NetworkObjectReference) networkObject);
    }
}