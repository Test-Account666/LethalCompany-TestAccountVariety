using System;
using System.Collections.Generic;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.ShopItems.Telepad;

public class Telepad : GrabbableObject {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Transform teleportationPoint;
    public TelepadTrigger teleportCollider;
    public float teleportationCooldown;

    public AudioSource teleportAudioSource;
    public AudioClip teleportSound;

    public AudioSource ambientAudioSource;
    public AudioClip ambientSound;

    public MeshRenderer meshRenderer;
    public Material enabledMaterial;
    public Material disabledMaterial;

    public float nextTeleport;
    public bool objectBeingPlaced;
    public bool active;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private const int _PLAYER_LAYER_MASK = 1 << 3;

    public override void Start() {
        base.Start();

        SyncActiveStateServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncActiveStateServerRpc() {
        ActivateItemClientRpc(active);
    }

    public override void Update() {
        base.Update();

        HandleAmbientSound();
        UpdateCooldown();
    }

    public void HandleAmbientSound() {
        if (ShouldPlayAmbientSound()) {
            if (ambientAudioSource.isPlaying) return;

            PlayAmbientSound();
            return;
        }

        ambientAudioSource.Stop();
    }

    public void PlayAmbientSound() {
        ambientAudioSource.loop = true;
        ambientAudioSource.clip = ambientSound;
        ambientAudioSource.Play();
    }

    public void UpdateCooldown() {
        if (!CanCooldownTick()) return;

        nextTeleport -= Time.deltaTime;

        if (nextTeleport > 0) return;

        SetMaterialState(true);
    }

    public bool CanBeUsedForTeleport() => nextTeleport <= 0 && CanCooldownTick();

    public bool CanCooldownTick() => !isHeld && active && insertedBattery.charge > 0;

    public bool ShouldPlayAmbientSound() => teleportCollider.enabled && CanBeUsedForTeleport();

    public override void GrabItem() {
        base.GrabItem();

        SetActiveStateServerRpc(false);
    }

    public override void DiscardItem() {
        base.DiscardItem();

        SetActiveStateServerRpc(!objectBeingPlaced);
        objectBeingPlaced = false;
    }

    public override void OnPlaceObject() {
        base.OnPlaceObject();

        objectBeingPlaced = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetActiveStateServerRpc(bool state) {
        ActivateItemClientRpc(state);
    }

    [ClientRpc]
    public void ActivateItemClientRpc(bool activate) {
        active = activate;
        UpdateTelepadState();
    }

    public void UpdateTelepadState() {
        ambientAudioSource.Stop();
        UpdateScale(active? 0.5f : 0.125f);
        teleportCollider.enabled = active;
        SetMaterialState(false);
        ResetCooldown();
    }

    public void UpdateScale(float scale) => transform.localScale = new(scale, scale, scale);

    public void ResetCooldown() => nextTeleport = teleportationCooldown;

    public void SetMaterialState(bool activate) => meshRenderer.material = activate? enabledMaterial : disabledMaterial;

    [ServerRpc(RequireOwnership = false)]
    public void TeleportPlayerServerRpc(int playerId) {
        if (!TryGetPlayer(playerId, out var player)) return;

        var availableTelepads = FindAvailableTelepads();

        if (availableTelepads.Count == 0) {
            SendWarningClientRpc(playerId, "No teleportation points found.");
            return;
        }

        TeleportPlayerToRandomTelepad(player, availableTelepads);
    }

    public List<Telepad> FindAvailableTelepads() {
        var telepads = new List<Telepad>(FindObjectsOfType<Telepad>());
        telepads.RemoveAll(telepad => telepad == this || !telepad.CanBeUsedForTeleport());
        return telepads;
    }

    public void TeleportPlayerToRandomTelepad(PlayerControllerB player, List<Telepad> telepads) {
        var random = new Random((uint) DateTime.Now.Ticks);
        var targetTelepad = telepads[random.NextInt(telepads.Count)];

        var poorPlayerCount = targetTelepad.GetPoorPlayers(out var poorPlayers);

        HandleTeleportationEffects(player, targetTelepad);

        KillPoorPlayers(player, poorPlayers, poorPlayerCount, random);
    }

    public void KillPoorPlayers(PlayerControllerB initiatingPlayer, List<PlayerControllerB> poorPlayers, int poorPlayerCount, Random random) {
        for (var index = 0; index < poorPlayerCount; index++) {
            var poorPlayer = poorPlayers[index];

            if (random.NextBool()) poorPlayer = initiatingPlayer;

            if (poorPlayer.isPlayerDead || !poorPlayer.isPlayerControlled) continue;

            KillPlayer((int) poorPlayer.playerClientId);
        }
    }

    public void KillPlayer(int playerId) {
        KillPlayerClientRpc(playerId);

        if (!TryGetPlayer(playerId, out var player)) return;

        player.KillPlayerServerRpc(playerId, true, Vector3.up, (int) CauseOfDeath.Crushing, 0, Vector3.zero);
    }

    [ClientRpc]
    public void KillPlayerClientRpc(int playerId) {
        if (!TryGetPlayer(playerId, out var player)) return;

        player.KillPlayer(Vector3.up, causeOfDeath: CauseOfDeath.Crushing);
    }

    public int GetPoorPlayers(out List<PlayerControllerB> players) {
        players = [
        ];
        var playerColliders = new Collider[16];
        var count = Physics.OverlapSphereNonAlloc(teleportationPoint.position, 1f, playerColliders, _PLAYER_LAYER_MASK);

        if (count <= 0) return 0;

        for (var i = 0; i < count; i++) {
            var collider = playerColliders[i];
            if (!collider.TryGetComponent(out PlayerControllerB player) || player.isPlayerDead || !player.isPlayerControlled
             || players.Contains(player)) continue;

            players.Add(player);
        }

        return players.Count;
    }

    public void HandleTeleportationEffects(PlayerControllerB player, Telepad targetTelepad) {
        targetTelepad.TeleportPlayerClientRpc((int) player.playerClientId);

        ApplyBatteryUsage(targetTelepad);
        ApplyCooldowns(targetTelepad);

        PlayTeleportSoundClientRpc();
        targetTelepad.PlayTeleportSoundClientRpc();
    }

    [ClientRpc]
    public void PlayTeleportSoundClientRpc() {
        ambientAudioSource.Stop();

        teleportAudioSource.PlayOneShot(teleportSound);

        var isInsideClosedShip = isInShipRoom && StartOfRound.Instance.hangarDoorsClosed;

        RoundManager.Instance.PlayAudibleNoise(teleportationPoint.position, noiseIsInsideClosedShip: isInsideClosedShip);
    }


    [ClientRpc]
    public void TeleportPlayerClientRpc(int playerId) {
        if (!TryGetPlayer(playerId, out var player)) return;

        nextTeleport += teleportationCooldown;
        player.DropAllHeldItems();
        player.TeleportPlayer(teleportationPoint.position);

        player.isInsideFactory = isInFactory;
        player.isInElevator = isInElevator;
        player.isInHangarShipRoom = isInShipRoom;
    }

    public void ApplyBatteryUsage(Telepad targetTelepad) {
        ApplyBatteryUsage();
        targetTelepad.ApplyBatteryUsage();
    }

    public void ApplyBatteryUsage() {
        var charge = insertedBattery.charge;
        var usage = itemProperties.batteryUsage * 100;

        SyncBatteryClientRpc((int) (charge - usage));
    }

    private void ApplyCooldowns(Telepad targetTelepad) {
        ApplyCooldownClientRpc();
        targetTelepad.ApplyCooldownClientRpc();
    }

    [ClientRpc]
    public void ApplyCooldownClientRpc() {
        UpdateTelepadState();
    }

    [ClientRpc]
    public void SendWarningClientRpc(int playerId, string warning) {
        if (!TryGetPlayer(playerId, out var player)) return;

        var localPlayer = StartOfRound.Instance.localPlayerController;
        if (localPlayer != player) return;

        HUDManager.Instance.DisplayTip("Telepad", warning, true);
    }

    public static bool TryGetPlayer(int playerId, out PlayerControllerB player) {
        if (playerId < 0 || playerId >= StartOfRound.Instance.allPlayerScripts.Length) {
            player = null!;
            return false;
        }

        player = StartOfRound.Instance.allPlayerScripts[playerId];
        return player != null;
    }

    public static bool TryGetEnemy(NetworkObjectReference enemyReference, out EnemyAI enemyAi) {
        if (!enemyReference.TryGet(out var networkObject)) {
            Debug.LogWarning("No network object found!");
            enemyAi = null!;
            return false;
        }

        var hasEnemy = networkObject.TryGetComponent(out enemyAi);

        return hasEnemy;
    }


    [ServerRpc(RequireOwnership = false)]
    public void TeleportEnemyServerRpc(NetworkObjectReference enemyReference) {
        if (!TryGetEnemy(enemyReference, out var _)) return;

        var availableTelepads = FindAvailableTelepads();

        if (availableTelepads.Count <= 0) {
            Debug.LogWarning("No teleportation points found.");
            return;
        }

        var random = new Random((uint) (DateTime.Now.Ticks & 0x0000FFFF));

        var telepad = availableTelepads[random.NextInt(0, availableTelepads.Count)];

        telepad.TeleportEnemyClientRpc(enemyReference);

        ApplyCooldowns(telepad);

        PlayTeleportSoundClientRpc();
        telepad.PlayTeleportSoundClientRpc();
    }

    [ClientRpc]
    public void TeleportEnemyClientRpc(NetworkObjectReference enemyReference) {
        if (!TryGetEnemy(enemyReference, out var enemyAi)) return;

        Debug.LogWarning("Teleportation successful!");

        nextTeleport += teleportationCooldown;

        enemyAi.serverPosition = teleportationPoint.position;
        enemyAi.transform.position = teleportationPoint.position;
        enemyAi.agent.Warp(teleportationPoint.position);

        if (!IsHost && !IsServer) return;

        enemyAi.SyncPositionToClients();
    }
}