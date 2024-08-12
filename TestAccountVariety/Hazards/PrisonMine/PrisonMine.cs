using System;
using System.Collections;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace TestAccountVariety.Hazards.PrisonMine;

public class PrisonMine : NetworkBehaviour {
    private static readonly int _TriggerAnimatorHash = Animator.StringToHash("Trigger");
    private static readonly int _UntriggerAnimatorHash = Animator.StringToHash("Untrigger");

    public float nextTrigger;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public AudioSource audioSource;
    public AudioSource farAudioSource;

    public AudioClip triggerClip;
    public AudioClip farTriggerClip;

    public AudioClip trapClip;
    public AudioClip farTrapClip;

    public AudioClip untrapClip;
    public AudioClip farUntrapClip;

    public Animator animator;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private void Update() => nextTrigger -= Time.deltaTime;

    private void OnTriggerEnter(Collider other) {
        if (nextTrigger > 0) return;

        var localPlayer = StartOfRound.Instance.localPlayerController;

        var hasPlayer = other.TryGetComponent<PlayerControllerB>(out var player);

        hasPlayer = hasPlayer && localPlayer == player;

        if (hasPlayer) {
            TriggerCageServerRpc();
            return;
        }

        if (!IsHost) return;

        TriggerCageClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TriggerCageServerRpc() {
        TriggerCageClientRpc();
    }


    [ClientRpc]
    public void TriggerCageClientRpc() {
        nextTrigger = 6F;
        StartCoroutine(TriggerPrisonMine());
    }

    public IEnumerator TriggerPrisonMine() {
        audioSource.PlayOneShot(triggerClip);
        farAudioSource.PlayOneShot(farTriggerClip);

        yield return new WaitForSeconds(.25F);

        animator.SetTrigger(_TriggerAnimatorHash);

        audioSource.PlayOneShot(trapClip);
        farAudioSource.PlayOneShot(farTrapClip);

        yield return new WaitForSeconds(3F);

        animator.SetTrigger(_UntriggerAnimatorHash);

        audioSource.PlayOneShot(untrapClip);
        farAudioSource.PlayOneShot(farUntrapClip);
    }
}