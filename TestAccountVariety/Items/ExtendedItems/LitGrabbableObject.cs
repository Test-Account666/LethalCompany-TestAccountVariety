using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TestAccountVariety.Items.ExtendedItems;

public class LitGrabbableObject : GrabbableObject {
    public List<Light> lights = [
    ];

    public List<GameObject> displays = [
    ];

    public override void PocketItem() {
        base.PocketItem();

        SetLightsEnabledServerRpc(false);
    }

    public override void GrabItem() {
        base.GrabItem();

        SetLightsEnabledServerRpc(true);
    }

    public override void EquipItem() {
        base.EquipItem();

        SetLightsEnabledServerRpc(true);
    }

    public override void DiscardItem() {
        base.DiscardItem();

        SetLightsEnabledServerRpc(true);
    }

    [ServerRpc(RequireOwnership = false)]
    // ReSharper disable once ParameterHidesMember
    public void SetLightsEnabledServerRpc(bool enabled) {
        SetLightsEnabledClientRpc(enabled);
    }

    [ClientRpc]
    // ReSharper disable once ParameterHidesMember
    public void SetLightsEnabledClientRpc(bool enabled) {
        lights.ForEach(light => light.enabled = enabled);

        displays.ForEach(display => display.SetActive(enabled));
    }
}