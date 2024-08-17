using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Items.WebleyPyramid;

public class WebleyPyramid : MonoBehaviour {
    private static readonly int _UntriggerAnimatorHash = Animator.StringToHash("Untrigger");

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public GrabbableObject grabbableObject;

    public AudioSource audioSource;

    public List<AudioClipWithWeight> audioClips;

    public Animator animator;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private void OnEnable() {
        if (grabbableObject.currentUseCooldown > 0) {
            animator.SetTrigger(_UntriggerAnimatorHash);
            return;
        }

        if (!grabbableObject.isHeld) {
            animator.SetTrigger(_UntriggerAnimatorHash);
            return;
        }

        var playerHeldBy = grabbableObject.playerHeldBy;

        var localPlayer = StartOfRound.Instance.localPlayerController;

        if (playerHeldBy != localPlayer) {
            animator.SetTrigger(_UntriggerAnimatorHash);
            return;
        }

        var wholeWeight = audioClips.Select((_, weight) => weight).Sum();

        var random = new Random((uint) (DateTime.Now.Ticks & 0x0000FFFF));

        var generatedWeight = random.NextInt(wholeWeight * 3, wholeWeight * 12 + 1);

        AudioClipWithWeight selectedAudioClip = null!;

        while (true) {
            var breakOut = false;

            foreach (var audioClip in audioClips) {
                generatedWeight -= audioClip.weight;

                if (generatedWeight > 0) continue;

                selectedAudioClip = audioClip;
                breakOut = true;
                break;
            }

            if (breakOut) break;
        }

        grabbableObject.currentUseCooldown = selectedAudioClip.audioClip.length;

        audioSource.PlayOneShot(selectedAudioClip.audioClip);

        if (!selectedAudioClip.isDeadly) {
            animator.SetTrigger(_UntriggerAnimatorHash);
            return;
        }

        StartCoroutine(KillPlayerAfterClip());
    }

    public IEnumerator KillPlayerAfterClip() {
        yield return new WaitUntil(() => !audioSource || !audioSource.isPlaying);

        var localPlayer = StartOfRound.Instance.localPlayerController;

        if (localPlayer == null || !localPlayer) {
            if (animator) animator.SetTrigger(_UntriggerAnimatorHash);
            yield break;
        }

        if (localPlayer.isPlayerDead) {
            if (animator) animator.SetTrigger(_UntriggerAnimatorHash);
            yield break;
        }

        localPlayer.DamagePlayer(int.MaxValue, causeOfDeath: CauseOfDeath.Fan, deathAnimation: 1);

        if (animator) animator.SetTrigger(_UntriggerAnimatorHash);
    }
}