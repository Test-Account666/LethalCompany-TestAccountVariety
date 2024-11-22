using System;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Items.Bomb;

public class ThrowableBombItem : StunGrenadeItem {
    private const float _KILL_RANGE = 4F;
    private const float _DAMAGE_RANGE = 6F;
    public bool primed;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public AudioSource itemAudioFar;
    public AudioClip explodeFarAudioClip;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public override void ItemActivate(bool used, bool buttonDown = true) {
        base.ItemActivate(used, buttonDown);

        PrimeBombServerRpc();
    }

    public override void EquipItem() {
        base.EquipItem();

        RollExplosionChanceServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RollExplosionChanceServerRpc() {
        var generatedChance = new Random((uint) (DateTime.Now.Ticks & 0x0000FFFF)).NextInt(1, 100);

        var explode = generatedChance < VarietyConfig.bombExplodeChance.Value;

        if (!explode) return;

        PrimeBombClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PrimeBombServerRpc() {
        PrimeBombClientRpc();
    }

    [ClientRpc]
    public void PrimeBombClientRpc() {
        primed = true;
    }

    public new Vector3 GetGrenadeThrowDestination() {
        const float maxDistance = 3.5F;

        var gameplayCameraTransform = playerHeldBy.gameplayCamera.transform;

        var startPosition = gameplayCameraTransform.position;
        var forward = gameplayCameraTransform.forward;

        // Handle edge case when looking straight down
        if (forward.y <= -0.9f) startPosition += -forward;

        var hitObject = Physics.Raycast(startPosition, forward, out var hit, maxDistance, stunGrenadeMask);

        var endPosition = hitObject? hit.point : startPosition + forward * maxDistance;

        var hitGround = Physics.Raycast(new(endPosition + Vector3.up, Vector3.down), out var groundHit, 100, stunGrenadeMask);

        if (hitGround) return groundHit.point + new Vector3(0, itemProperties.verticalOffset, 0);

        TestAccountVariety.Logger.LogError($"{playerHeldBy.playerUsername} bomb couldn't find ground!");
        return endPosition;
    }

    public new void ExplodeStunGrenade(bool explode) {
        if (hasExploded || !explode || !primed) {
            gotExplodeOnThrowRPC = false;
            hasCollided = false;
            explodeTimer = 2.5F;
            hasExploded = false;
            return;
        }

        hasExploded = true;

        Landmine.SpawnExplosion(transform.position + Vector3.up * 0.2f, killRange: _KILL_RANGE, damageRange: _DAMAGE_RANGE, nonLethalDamage: 40,
                                physicsForce: 45f);

        var parent = !isInElevator? RoundManager.Instance.mapPropsContainer.transform : StartOfRound.Instance.elevatorTransform;

        Instantiate(stunGrenadeExplosion, transform.position, Quaternion.identity, parent);
        itemAudio.PlayOneShot(explodeSFX);
        itemAudioFar.PlayOneShot(explodeFarAudioClip);
        WalkieTalkie.TransmitOneShotAudio(itemAudio, explodeSFX);
        RoundManager.Instance.PlayAudibleNoise(transform.position, 30F);

        if (!DestroyGrenade) return;
        DestroyObjectInHand(playerThrownBy);
    }
}