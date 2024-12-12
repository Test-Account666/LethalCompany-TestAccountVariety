using System;
using System.Collections;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using static TestAccountVariety.Utils.ReferenceResolver;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Items.LightSwitch;

public class LightSwitch : NoisemakerProp {
    private static readonly int _OnAnimatorHash = Animator.StringToHash("on");
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Animator animator;

    public AudioSource deathAudioSource;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public override void Start() {
        base.Start();

        insertedBattery = new(false, 1F);
    }

    public override void ItemActivate(bool used, bool buttonDown = true) {
        base.ItemActivate(used, buttonDown);

        animator.SetBool(_OnAnimatorHash, !animator.GetBool(_OnAnimatorHash));

        if (!playerHeldBy) return;

        var localPlayer = StartOfRound.Instance.localPlayerController;
        if (playerHeldBy != localPlayer) return;

        ToggleLightsServerRpc((int) localPlayer.playerClientId, playerHeldBy.isInsideFactory);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleLightsServerRpc(int playerId, bool factoryLights) {
        ApplyBatteryUsage();

        if (factoryLights) {
            var breakerBox = FindObjectOfType<BreakerBox>();
            if (!breakerBox) return;

            RoundManager.Instance.SwitchPower(!breakerBox.isPowerOn);
            RollKillChance(playerId);
            return;
        }

        var shipLights = FindObjectOfType<ShipLights>();
        if (!shipLights) return;

        shipLights.ToggleShipLightsOnLocalClientOnly();
        shipLights.SetShipLightsClientRpc(shipLights.areLightsOn);
        RollKillChance(playerId);
    }

    [ClientRpc]
    public void KillPlayerClientRpc(int playerId) {
        var hasPlayer = TryGetPlayer(playerId, out var player);

        if (!hasPlayer) return;

        if (playerHeldBy.isPlayerDead || !player.isPlayerControlled) return;

        if (!playerHeldBy.AllowPlayerDeath()) return;

        StartCoroutine(KillPlayer(player));
    }

    public IEnumerator KillPlayer(PlayerControllerB player) {
        var localPlayer = StartOfRound.Instance.localPlayerController;

        if (localPlayer == player) deathAudioSource.Play();

        yield return new WaitForSeconds(1.5F);

        deathAudioSource.Stop();

        player.KillPlayer(Vector3.up, causeOfDeath: CauseOfDeath.Blast, deathAnimation: 1);
    }

    public void RollKillChance(int playerId) {
        var random = new Random((uint) (DateTime.Now.Ticks & 0x0000FFFF));

        var generatedChance = random.NextInt(1, 100);

        var kill = generatedChance < VarietyConfig.lightSwitchKillChance.Value;
        if (!kill) return;

        KillPlayerClientRpc(playerId);
    }

    public void ApplyBatteryUsage() {
        if (!IsHost && !IsServer) return;

        var currentCharge = insertedBattery.charge * 100;

        SyncBatteryClientRpc(Math.Max((int) (currentCharge - VarietyConfig.lightSwitchBatteryUsage.Value), 0));
    }
}