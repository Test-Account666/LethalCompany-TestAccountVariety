using System.Collections;
using GameNetcodeStuff;
using TestAccountVariety.Config;
using Unity.Netcode;
using UnityEngine;
using static TestAccountVariety.Utils.ReferenceResolver;

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

    public Transform enemyTeleportPoint;
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

        if (!IsHost || !CageMineConfig.useBigEnemyCollider.Value) return;

        TriggerAsEnemy(other);
    }

    public void TriggerAsEnemy(Collider collider) {
        TriggerCageClientRpc(CageMineConfig.cageMineCoolDown.Value, CageMineConfig.cageEnemyTrapTime.Value);

        var hasEnemy = TryGetEnemy(collider, out var enemyAI);

        if (!hasEnemy) return;

        TeleportEnemyClientRpc(enemyAI.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TriggerCageServerRpc() {
        TriggerCageClientRpc(CageMineConfig.cageMineCoolDown.Value, CageMineConfig.cagePlayerTrapTime.Value);
    }


    [ClientRpc]
    public void TriggerCageClientRpc(float coolDown, float trapTime) {
        nextTrigger = coolDown + trapTime;
        StartCoroutine(TriggerPrisonMine(trapTime));
    }

    [ClientRpc]
    public void TeleportEnemyClientRpc(NetworkObjectReference enemyReference) {
        var hasEnemy = TryGetEnemy(enemyReference, out var enemyAI);

        if (!hasEnemy) return;

        enemyAI.agent.Warp(enemyTeleportPoint.position);
    }

    public IEnumerator TriggerPrisonMine(float trapTime) {
        audioSource.PlayOneShot(triggerClip);
        farAudioSource.PlayOneShot(farTriggerClip);

        yield return new WaitForSeconds(.25F);

        animator.SetTrigger(_TriggerAnimatorHash);

        audioSource.PlayOneShot(trapClip);
        farAudioSource.PlayOneShot(farTrapClip);

        yield return new WaitForSeconds(trapTime);

        animator.SetTrigger(_UntriggerAnimatorHash);

        audioSource.PlayOneShot(untrapClip);
        farAudioSource.PlayOneShot(farUntrapClip);
    }
}