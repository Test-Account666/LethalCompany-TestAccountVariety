using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Items.ThrowableCube;

public class ThrowableCube : StunGrenadeItem {
    private static InputAction? _interactInputAction;
    private static InputAction? _discardInputAction;
    private const float KILL_RANGE = 1F;
    private const float DAMAGE_RANGE = 3F;
    private Random _random;

    public override void Start() {
        base.Start();
        _random = new((uint) (DateTime.Now.Ticks & 0x0000FFFF));
    }

    public override void Update() {
        if (!isHeld) {
            base.Update();
            return;
        }

        var localPlayer = StartOfRound.Instance.localPlayerController;

        if (playerHeldBy != localPlayer) {
            base.Update();
            return;
        }

        _interactInputAction ??= IngamePlayerSettings.Instance.playerInput.actions.FindAction("Interact");
        _discardInputAction ??= IngamePlayerSettings.Instance.playerInput.actions.FindAction("Discard");


        explodeOnCollision = !_interactInputAction.IsPressed() && !_discardInputAction.IsPressed();

        base.Update();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ExplodeServerRpc() {
        var generatedChance = _random.NextInt(1, 100);

        var explode = generatedChance < VarietyConfig.cubeExplodeChance.Value;

        ExplodeClientRpc(explode);
    }

    [ClientRpc]
    public void ExplodeClientRpc(bool explode) {
        ExplodeStunGrenade(explode);
    }

    public new void ExplodeStunGrenade(bool explode) {
        gotExplodeOnThrowRPC = false;
        hasCollided = false;
        explodeTimer = 2.5F;
        hasExploded = false;

        if (!explode) return;

        Landmine.SpawnExplosion(transform.position + Vector3.up * 0.2f, killRange: KILL_RANGE, damageRange: DAMAGE_RANGE, nonLethalDamage: 40,
                                physicsForce: 45f);

        var parent = !isInElevator? RoundManager.Instance.mapPropsContainer.transform : StartOfRound.Instance.elevatorTransform;

        Instantiate(stunGrenadeExplosion, transform.position, Quaternion.identity, parent);
        itemAudio.PlayOneShot(explodeSFX);
        WalkieTalkie.TransmitOneShotAudio(itemAudio, explodeSFX);
    }
}