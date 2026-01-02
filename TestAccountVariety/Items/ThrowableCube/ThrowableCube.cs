using System;
using GameNetcodeStuff;
using TestAccountVariety.Config;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Items.ThrowableCube;

public class ThrowableCube : StunGrenadeItem {
    public float physicsForce = 45F;

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

        var explode = generatedChance < CubeConfig.cubeExplodeChance.Value;

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

        var explosionPosition = transform.position + Vector3.up * 0.2f;

        Landmine.SpawnExplosion(explosionPosition, killRange: KILL_RANGE, damageRange: DAMAGE_RANGE, nonLethalDamage: 40);

        ApplyPhysicsForce(explosionPosition);

        var parent = !isInElevator? RoundManager.Instance.mapPropsContainer.transform : StartOfRound.Instance.elevatorTransform;

        Instantiate(stunGrenadeExplosion, transform.position, Quaternion.identity, parent);
        itemAudio.PlayOneShot(explodeSFX);
        WalkieTalkie.TransmitOneShotAudio(itemAudio, explodeSFX);
    }

    public void ApplyPhysicsForce(Vector3 explosionPosition) {
        var localPlayer = StartOfRound.Instance.localPlayerController;

        if (!IsWithinEffectiveRange(localPlayer.transform.position, explosionPosition)) return;
        if (IsBlockedByObstacle(explosionPosition, localPlayer.transform.position)) return;

        var force = CalculatePhysicsForce(localPlayer.transform.position, explosionPosition);
        if (force.magnitude <= 2.0) return;

        ApplyForceToLocalPlayer(localPlayer, force);
    }

    private static bool IsWithinEffectiveRange(Vector3 playerPosition, Vector3 explosionPosition) {
        var distance = Vector3.Distance(playerPosition, explosionPosition);
        return distance < 35.0;
    }

    private static bool IsBlockedByObstacle(Vector3 explosionPosition, Vector3 playerPosition) =>
        Physics.Linecast(explosionPosition, playerPosition + Vector3.up * 0.3f, out _, 256, QueryTriggerInteraction.Ignore);

    private Vector3 CalculatePhysicsForce(Vector3 playerPosition, Vector3 explosionPosition) {
        var distance = Vector3.Distance(playerPosition, explosionPosition);
        var direction = Vector3.Normalize(playerPosition + Vector3.up * distance - explosionPosition);
        return direction / (distance * 0.35f) * physicsForce;
    }

    private static void ApplyForceToLocalPlayer(PlayerControllerB localPlayer, Vector3 force) {
        if (force.magnitude > 10.0) localPlayer.CancelSpecialTriggerAnimations();

        if (localPlayer.inVehicleAnimation && (localPlayer.externalForceAutoFade + force).magnitude < 50.0) return;

        localPlayer.externalForceAutoFade += force;
    }


    public new Vector3 GetGrenadeThrowDestination() {
        const float maxDistance = 15f;

        var gameplayCameraTransform = playerHeldBy.gameplayCamera.transform;

        var startPosition = gameplayCameraTransform.position;
        var forward = gameplayCameraTransform.forward;

        // Handle edge case when looking straight down
        if (forward.y <= -0.9f) startPosition += -forward;

        var hitObject = Physics.Raycast(startPosition, forward, out var hit, maxDistance, stunGrenadeMask);

        var endPosition = hitObject? hit.point : startPosition + forward * maxDistance;

        var hitGround = Physics.Raycast(new(endPosition + Vector3.up, Vector3.down), out var groundHit, 100, stunGrenadeMask);

        if (hitGround) return groundHit.point + new Vector3(0, itemProperties.verticalOffset, 0);

        TestAccountVariety.Logger.LogError($"{playerHeldBy.playerUsername} cube couldn't find ground!");
        return endPosition;
    }
}