using System.Collections;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace TestAccountVariety.Hazards.PropulsionMine;

public class NewPropulsionMine : PropulsionMine {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public AudioSource mineAudio;
    public AudioSource farMineAudio;

    public AudioClip[] beepClip;

    public AudioClip[] mineDetonateClip;
    public AudioClip[] farMineDetonateClip;

    public AudioClip[] pressedClip;

    public AudioClip deactivateClip;
    public AudioClip activateClip;

    public Animator pressAnimator;

    public MeshRenderer buttonMeshRenderer;

    public Material defaultMaterial;
    public Material litMaterial;

    public GameObject lightObject;
    public Light light;
    public HDAdditionalLightData lightData;

    public float physicsForce;

    public float damageRange;
    public int nonLethalDamage;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private static readonly int _BoomAnimatorHash = Animator.StringToHash("Boom");
    private static readonly int _PressedAnimatorHash = Animator.StringToHash("Pressed");

    public int pressed;

    public bool activated = true;

    private void Start() => StartCoroutine(PlayBeeps());

    private IEnumerator PlayBeeps() {
        if (!this) yield break;

        yield return new WaitForSeconds(Random.Range(10, 16));

        PlayBeep();

        buttonMeshRenderer.material = litMaterial;
        buttonMeshRenderer.sharedMaterial = litMaterial;

        lightObject.SetActive(true);

        light.enabled = true;
        light.range = 12;
        lightData.intensity = 2610.3F;

        yield return new WaitForSeconds(.2F);

        lightObject.SetActive(false);
        light.enabled = false;

        buttonMeshRenderer.material = defaultMaterial;
        buttonMeshRenderer.sharedMaterial = defaultMaterial;

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

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (press? pressed <= 1 : pressed <= 0) pressAnimator.SetBool(_PressedAnimatorHash, press);

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
        pressAnimator.SetTrigger(_BoomAnimatorHash);

        var detonateClip = mineDetonateClip[Random.Range(0, mineDetonateClip.Length)];

        mineAudio.pitch = Random.Range(0.93F, 1.07F);
        mineAudio.PlayOneShot(detonateClip, 1F);

        var detonateFarClip = farMineDetonateClip[Random.Range(0, farMineDetonateClip.Length)];

        farMineAudio.pitch = Random.Range(0.93F, 1.07F);
        farMineAudio.PlayOneShot(detonateFarClip, 1F);

        Landmine.SpawnExplosion(transform.position + Vector3.up, killRange: 0F,
                                damageRange: damageRange, nonLethalDamage: nonLethalDamage, physicsForce: physicsForce);
    }

    public void PlayBeep() {
        var beep = beepClip[Random.Range(0, beepClip.Length)];

        mineAudio.pitch = Random.Range(0.93F, 1.07F);
        mineAudio.PlayOneShot(beep, 1F);
    }
}