using Unity.Netcode;
using UnityEngine;

namespace TestAccountVariety.Items.Battery;

public class Battery : GrabbableObject {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public AudioSource audioSource;
    public AudioClip insertBatteryClip;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public override void ItemActivate(bool used, bool buttonDown = true) {
        base.ItemActivate(used, buttonDown);

        if (playerHeldBy == null || !playerHeldBy) return;

        var localPlayer = StartOfRound.Instance.localPlayerController;

        if (playerHeldBy != localPlayer) return;

        var cameraTransform = localPlayer.gameplayCamera.transform;
        var objectRay = new Ray(cameraTransform.position, cameraTransform.forward);

        var foundObject = Physics.Raycast(objectRay, out var hit, localPlayer.grabDistance, localPlayer.interactableObjectsMask)
                       && hit.collider.gameObject.layer != 8 && hit.collider.tag == "PhysicsProp" && !Physics.Linecast(
                              cameraTransform.position, hit.collider.transform.position + localPlayer.transform.up * 0.16f, 1073741824,
                              QueryTriggerInteraction.Ignore);

        if (!foundObject) return;

        var hasGrabbableObject = hit.collider.gameObject.TryGetComponent<GrabbableObject>(out var grabbableObject);

        if (!hasGrabbableObject) return;

        if (!grabbableObject.grabbable || grabbableObject.isHeld || grabbableObject.isHeldByEnemy) return;

        if (!grabbableObject.itemProperties.requiresBattery) return;

        if (grabbableObject.insertedBattery.charge >= 1) return;

        var hasNetworkObject = grabbableObject.TryGetComponent<NetworkObject>(out var networkObject);

        if (!hasNetworkObject) return;


        UseBatteryServerRpc(networkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UseBatteryServerRpc(NetworkObjectReference grabbableObjectReference) {
        UseBatterClientRpc(grabbableObjectReference);
    }

    [ClientRpc]
    public void UseBatterClientRpc(NetworkObjectReference grabbableObjectReference) {
        var networkObject = (NetworkObject) grabbableObjectReference;

        if (networkObject == null || !networkObject) return;

        var grabbableObject = networkObject.GetComponentInChildren<GrabbableObject>();

        if (grabbableObject == null || !grabbableObject) return;

        grabbableObject.insertedBattery.empty = false;
        grabbableObject.insertedBattery.charge = 1F;
        grabbableObject.ChargeBatteries();

        audioSource.PlayOneShot(insertBatteryClip);

        if (playerHeldBy == null || !playerHeldBy) return;

        DestroyObjectInHand(playerHeldBy);
    }
}