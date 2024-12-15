using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using TestAccountVariety.Config;
using TestAccountVariety.Utils;
using Unity.Netcode;
using UnityEngine;
using static TestAccountVariety.Utils.ReferenceResolver;
using Random = System.Random;

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

    public float currentTeleportCoolDown;

    public bool active;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private const int _PLAYER_LAYER_MASK = 1 << 3;

    private const float _ACTIVATED_SCALE = .5f;
    private const float _DEACTIVATED_SCALE = .125f;

    public override void Update() {
        base.Update();

        TickCoolDown();
        UpdateAmbientSound();
    }

    public override void GrabItem() {
        Deactivate();

        base.GrabItem();
    }

    public override void GrabItemFromEnemy(EnemyAI enemy) {
        Deactivate();

        base.GrabItemFromEnemy(enemy);
    }

    public override void DiscardItemFromEnemy() {
        base.DiscardItemFromEnemy();

        active = true;
        SetScale(_ACTIVATED_SCALE);
    }

    public void Deactivate() {
        ApplyCoolDown();
        active = false;
        SetScale(_DEACTIVATED_SCALE);
    }

    public override void ItemActivate(bool used, bool buttonDown = true) {
        base.ItemActivate(used, buttonDown);

        active = true;
        SetScale(_ACTIVATED_SCALE);

        if (!isHeld || !playerHeldBy) return;

        var localPlayer = StartOfRound.Instance.localPlayerController;

        if (playerHeldBy != localPlayer) return;

        localPlayer.StartCoroutine(localPlayer.waitToEndOfFrameToDiscard());
    }

    public void UpdateAmbientSound() {
        if (!CanTeleport()) {
            if (!ambientAudioSource.isPlaying) return;

            ambientAudioSource.Stop();
            return;
        }

        PlayAmbientSound();
    }

    public void PlayAmbientSound() {
        if (ambientAudioSource.isPlaying) return;

        ambientAudioSource.clip = ambientSound;
        ambientAudioSource.loop = true;
        ambientAudioSource.Play();
    }

    public void TickCoolDown() {
        if (!CanCoolDownTick()) {
            currentTeleportCoolDown = teleportationCooldown;
            return;
        }

        if (currentTeleportCoolDown <= 0) return;

        currentTeleportCoolDown -= Time.deltaTime;

        if (currentTeleportCoolDown > 0) return;
        UpdateMaterial(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TeleportPlayerServerRpc(int playerIndex, NetworkObjectReference sourceTelepadReference) {
        var hasPlayer = TryGetPlayer(playerIndex, out var player);

        if (!hasPlayer) return;

        var random = new Random((int) (DateTime.Now.Ticks + transform.position.ConvertToInt()));

        var poorPlayers = GetPoorPlayers(player);

        foreach (var poorPlayer in poorPlayers) {
            var willObliterate = random.Next(0, 2) > 0;

            if (!willObliterate) {
                KillPlayerClientRpc(playerIndex);
                continue;
            }

            KillPlayerClientRpc((int) poorPlayer.playerClientId);
        }

        TeleportPlayerClientRpc(playerIndex, sourceTelepadReference);
    }

    [ClientRpc]
    public void KillPlayerClientRpc(int playerIndex) {
        var hasPlayer = TryGetPlayer(playerIndex, out var player);

        if (!hasPlayer) return;

        player.KillPlayer(Vector3.up, causeOfDeath: CauseOfDeath.Crushing);

        var deadBody = player.deadBody;

        if (!deadBody) return;

        if (deadBody.bodyParts.Length <= 0) return;

        foreach (var deadBodyBodyPart in deadBody.bodyParts) deadBodyBodyPart.position = teleportationPoint.position;
    }

    public List<PlayerControllerB> GetPoorPlayers(PlayerControllerB exemptPlayer) {
        var colliders = new Collider[16];
        var count = Physics.OverlapSphereNonAlloc(teleportationPoint.position, 2F, colliders, _PLAYER_LAYER_MASK, QueryTriggerInteraction.Ignore);

        return colliders.Take(count).Select(collider => collider.GetComponent<PlayerControllerB>()).Where(player => player && player != exemptPlayer).Distinct()
                        .ToList();
    }


    [ClientRpc]
    public void TeleportPlayerClientRpc(int playerIndex, NetworkObjectReference sourceTelepadReference) {
        var hasPlayer = TryGetPlayer(playerIndex, out var player);
        if (!hasPlayer) return;

        // Local client bypasses RPCs for teleportation
        var localPlayer = StartOfRound.Instance.localPlayerController;
        if (player == localPlayer) return;

        var hasSourceTelepad = TryGetTelepad(sourceTelepadReference, out var sourceTelepad);

        if (!hasSourceTelepad) return;

        TeleportOnLocalClient(sourceTelepad, player);
    }

    public void TeleportOnLocalClient(Telepad sourceTelepad, PlayerControllerB player) {
        HandlePreTeleport(sourceTelepad);

        StartCoroutine(TeleportPlayer(player));
    }

    public IEnumerator TeleportPlayer(PlayerControllerB player) {
        yield return new WaitForEndOfFrame();

        if (TelepadConfig.telepadDropsItems.Value) {
            player.DropAllHeldItems();

            yield return null;
            yield return new WaitForEndOfFrame();
        }

        player.TeleportPlayer(teleportationPoint.position);
        player.isInsideFactory = isInFactory;
        player.isInElevator = isInElevator;
        player.isInHangarShipRoom = isInShipRoom;
    }

    [ClientRpc]
    public void TeleportEnemyClientRpc(NetworkObjectReference sourceTelepadReference, NetworkObjectReference enemyReference) {
        var hasSourceTelepad = TryGetTelepad(sourceTelepadReference, out var sourceTelepad);

        if (!hasSourceTelepad) return;

        var hasEnemy = TryGetEnemy(enemyReference, out var enemyAI);

        if (!hasEnemy) return;

        HandlePreTeleport(sourceTelepad, TelepadConfig.telepadEnemyUsesPower.Value);

        enemyAI.agent.Warp(teleportationPoint.position);
        enemyAI.isInsidePlayerShip = isInShipRoom;
        enemyAI.isOutside = !isInFactory;
    }

    public void HandlePreTeleport(Telepad sourceTelepad, bool ignoreBattery = false) {
        sourceTelepad.ApplyCoolDown();

        sourceTelepad.teleportAudioSource.PlayOneShot(teleportSound);

        if (IsHost && !ignoreBattery)
            sourceTelepad.SyncBatteryClientRpc((int) (sourceTelepad.insertedBattery.charge * 100 - sourceTelepad.itemProperties.batteryUsage * 100));

        ApplyCoolDown();

        teleportAudioSource.PlayOneShot(teleportSound);

        if (IsHost && !ignoreBattery) SyncBatteryClientRpc((int) (insertedBattery.charge * 100 - itemProperties.batteryUsage * 100));
    }

    public List<Telepad> GetAvailableTelepads() {
        var telepads = FindObjectsByType<Telepad>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        return telepads.Where(telepad => telepad).Where(telepad => telepad != this && telepad.CanTeleport()).ToList();
    }

    public void ApplyCoolDown() {
        currentTeleportCoolDown = teleportationCooldown;
        UpdateMaterial(false);
    }

    public void UpdateMaterial(bool activate) => meshRenderer.material = activate? enabledMaterial : disabledMaterial;

    public bool CanTeleport() => CanCoolDownTick() && currentTeleportCoolDown <= 0;

    public bool CanCoolDownTick() => active && !isHeld && !isHeldByEnemy && insertedBattery.charge > 0;

    public void SetScale(float newScale) {
        originalScale = new(newScale, newScale, newScale);
        transform.localScale = new(newScale, newScale, newScale);
    }
}