using System;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Items.DoorScrap;

public class DoorScrap : GrabbableObject {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public GameObject steelDoorArt;
    public GameObject fancyDoorArt;
    public GameObject mineDoorArt;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public static readonly string[] PossibleDoorVariations = [
        "SteelDoor", "FancyDoor", "YellowMineDoor",
    ];

    public string? doorType;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        SyncScrapValueServerRpc();
        SyncDoorTypeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncScrapValueServerRpc() {
        SyncScrapValueClientRpc(scrapValue);
    }

    [ClientRpc]
    public void SyncScrapValueClientRpc(int value) {
        SetScrapValue(value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncDoorTypeServerRpc() {
        if (string.IsNullOrWhiteSpace(doorType)) {
            var random = new Random((uint) (DateTime.Now.Ticks & 0x0000FFFF));

            doorType = PossibleDoorVariations[random.NextInt(0, PossibleDoorVariations.Length)];
        }

        SyncDoorTypeClientRpc(doorType);
    }

    [ClientRpc]
    // ReSharper disable once ParameterHidesMember
    public void SyncDoorTypeClientRpc(string doorType) {
        this.doorType = doorType;

        switch (doorType) {
            case "SteelDoor": {
                fancyDoorArt.SetActive(false);
                mineDoorArt.SetActive(false);
                steelDoorArt.SetActive(true);

                itemProperties.toolTips = [
                ];
                break;
            }
            case "YellowMineDoor": {
                steelDoorArt.SetActive(false);
                fancyDoorArt.SetActive(false);
                mineDoorArt.SetActive(true);

                itemProperties.toolTips = [
                    "WHY WOULD IT BLOCK MY SIGHT?!",
                ];
                break;
            }
            case "FancyDoor": {
                steelDoorArt.SetActive(false);
                mineDoorArt.SetActive(false);
                fancyDoorArt.SetActive(true);

                itemProperties.toolTips = [
                    "WHY WOULD IT BLOCK MY SIGHT?!",
                ];
                break;
            }
        }
    }
}