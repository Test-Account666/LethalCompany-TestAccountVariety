using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace TestAccountVariety.Items.CleaningDroneToy;

public class CleaningDroneToy : GrabbableObject {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public GameObject fogPrefab;

    public AudioSource audioSource;

    public AudioClip audioClip;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public float gasTank = 1F;
    private int _gassed;

    public Coroutine? gasCoroutine;

    public override void ItemActivate(bool used, bool buttonDown = true) {
        base.ItemActivate(used, buttonDown);

        if (gasCoroutine != null) StopCoroutine(gasCoroutine);

        if (!buttonDown) {
            StopSoundServerRpc();
            return;
        }

        gasCoroutine = StartCoroutine(GasEveryone());
    }

    public override void DiscardItem() {
        StopSoundServerRpc();
        if (gasCoroutine != null) StopCoroutine(gasCoroutine);

        base.DiscardItem();
    }

    public override void PocketItem() {
        StopSoundServerRpc();
        if (gasCoroutine != null) StopCoroutine(gasCoroutine);

        base.PocketItem();
    }

    #region Overrides of GrabbableObject

    public override void Update() {
        base.Update();

        if (!IsHost && !IsServer) return;

        if (gasTank <= 0) return;
        gasTank -= Time.deltaTime * _gassed;
        _gassed = 0;
    }

    #endregion

    public IEnumerator GasEveryone() {
        var position = transform.position;

        SpawnGasServerRpc(position);

        yield return new WaitForSeconds(.5F);
        yield return GasEveryone();
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopSoundServerRpc() {
        StopSoundClientRpc();
    }

    [ClientRpc]
    public void StopSoundClientRpc() {
        audioSource.Stop();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnGasServerRpc(Vector3 position) {
        if (gasTank <= 0) {
            StopSoundClientRpc();
            return;
        }

        _gassed++;
        SpawnGasClientRpc(position);
    }

    [ClientRpc]
    public void SpawnGasClientRpc(Vector3 position) {
        if (!audioSource.isPlaying) {
            audioSource.PlayOneShot(audioClip, 1F);
            audioSource.loop = true;
        }

        var fog = Instantiate(fogPrefab);
        fog.transform.SetPositionAndRotation(position, Quaternion.identity);
    }
}