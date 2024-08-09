using System.Collections;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TestAccountVariety.Hazards.PropulsionMine;

public class PropulsionMine : NetworkBehaviour {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public AudioSource mineAudio;
    public AudioSource farMineAudio;

    public AudioClip[] beepClip;

    public AudioClip[] mineDetonateClip;
    public AudioClip[] farMineDetonateClip;

    public AudioClip[] pressedClip;

    public AudioClip deactivateClip;
    public AudioClip activateClip;

    public Animator shockwaveAnimator;

    public Light light;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private static readonly int _ShockAnimatorHash = Animator.StringToHash("Shock");

    public int pressed;

    public bool activated = true;

    private void Start() => StartCoroutine(PlayBeeps());

    private IEnumerator PlayBeeps() {
        if (!this) yield break;

        yield return new WaitForSeconds(Random.Range(10, 16));

        PlayBeep();

        light.enabled = true;
        yield return new WaitForSeconds(.1F);
        light.enabled = false;

        yield return PlayBeeps();
    }

    private void OnTriggerEnter(Collider other) {
        var hasPlayer = other.TryGetComponent<PlayerControllerB>(out var player);

        if (!hasPlayer) return;

        var localPlayer = StartOfRound.Instance.localPlayerController;

        if (player != localPlayer) return;

        if (!activated) return;

        PressMineServerRpc(true);
    }

    private void OnTriggerExit(Collider other) {
        var hasPlayer = other.TryGetComponent<PlayerControllerB>(out var player);

        if (!hasPlayer) return;

        var localPlayer = StartOfRound.Instance.localPlayerController;

        if (player != localPlayer) return;

        if (pressed <= 0) return;

        var detonate = pressed - 1 <= 0;

        if (detonate && activated) DetonateServerRpc();
        PressMineServerRpc(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DetonateServerRpc() {
        DetonateClientRpc();
    }

    [ClientRpc]
    public void DetonateClientRpc() {
        Detonate();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PressMineServerRpc(bool press) {
        PressMineClientRpc(press);
    }

    [ClientRpc]
    public void PressMineClientRpc(bool press) {
        pressed += (press? 1 : -1);

        if (pressed > 1) return;

        var clip = pressedClip[Random.Range(0, pressedClip.Length)];

        mineAudio.PlayOneShot(clip, 1F);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetMineEnabledServerRpc(bool enable) {
        SetMineEnabledClientRpc(enable);
    }

    [ClientRpc]
    public void SetMineEnabledClientRpc(bool enable) {
        enabled = enable;
    }

    public void Detonate() {
        shockwaveAnimator.SetTrigger(_ShockAnimatorHash);

        var detonateClip = mineDetonateClip[Random.Range(0, mineDetonateClip.Length)];

        mineAudio.pitch = Random.Range(0.93F, 1.07F);
        mineAudio.PlayOneShot(detonateClip, 1F);

        var detonateFarClip = farMineDetonateClip[Random.Range(0, farMineDetonateClip.Length)];

        farMineAudio.pitch = Random.Range(0.93F, 1.07F);
        farMineAudio.PlayOneShot(detonateFarClip, 1F);

        Landmine.SpawnExplosion(transform.position + Vector3.up, killRange: 0F, damageRange: 3F, nonLethalDamage: 20, physicsForce: 45F);
    }

    public void PlayBeep() {
        var beep = beepClip[Random.Range(0, beepClip.Length)];

        mineAudio.pitch = Random.Range(0.93F, 1.07F);
        mineAudio.PlayOneShot(beep, 1F);
    }
}