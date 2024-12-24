using System.Collections;
using GameNetcodeStuff;
using UnityEngine;

namespace TestAccountVariety.Items.Cookie;

public class Cookie : NoisemakerProp {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public AudioSource afterEatingNoiseAudioSource;
    public AudioClip afterEatingNoiseClip;

    public MeshRenderer[] meshRenderers;
    public GameObject scanNodeObject;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public override void ItemActivate(bool used, bool buttonDown = true) {
        base.ItemActivate(used, buttonDown);

        if (playerHeldBy == null || !playerHeldBy) return;

        var localPlayer = StartOfRound.Instance.localPlayerController;

        if (localPlayer == playerHeldBy) localPlayer.StartCoroutine(localPlayer.waitToEndOfFrameToDiscard());

        playerHeldBy.StartCoroutine(HealPlayer(playerHeldBy));
        StartCoroutine(PlayAfterEatingNoiseAndDestroy(playerHeldBy));
    }

    public static IEnumerator HealPlayer(PlayerControllerB playerControllerB) {
        var localPlayer = StartOfRound.Instance.localPlayerController;
        var isLocalPlayer = localPlayer == playerControllerB;

        for (var index = 0; index < 100; index++) {
            if (playerControllerB.health < 100) {
                playerControllerB.health += 1;
                if (isLocalPlayer) HUDManager.Instance.UpdateHealthUI(playerControllerB.health);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator PlayAfterEatingNoiseAndDestroy(PlayerControllerB previouslyHeldBy) {
        foreach (var meshRenderer in meshRenderers) meshRenderer.gameObject.SetActive(false);
        scanNodeObject.SetActive(false);

        grabbable = false;
        grabbableToEnemies = false;
        deactivated = true;

        yield return new WaitUntil(() => !noiseAudio.isPlaying);
        if (afterEatingNoiseClip) {
            var tempAudioSource = Instantiate(afterEatingNoiseAudioSource, previouslyHeldBy.transform);
            tempAudioSource.PlayOneShot(afterEatingNoiseClip);
            yield return new WaitUntil(() => !tempAudioSource.isPlaying);
            Destroy(tempAudioSource.gameObject);
        }

        if (this is {
                IsHost: false,
                IsServer: false,
            }) yield break;

        NetworkObject.Despawn();
    }
}