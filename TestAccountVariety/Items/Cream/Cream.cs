using System.Collections;
using UnityEngine;

namespace TestAccountVariety.Items.Cream;

public class Cream : NoisemakerProp {
    private static readonly int _ShakeItemAnimatorHash = Animator.StringToHash("shakeItem");
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public AudioClip sprayCanShakeEmptySfx;
    public AudioClip[] sprayCanShakeSfx;

    public AudioClip spraySfx;

    public ParticleSystem sprayParticles;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public override void ItemActivate(bool used, bool buttonDown = true) {
        base.ItemActivate(used, buttonDown);

        StartCoroutine(WaitAndSpray());
    }

    public IEnumerator WaitAndSpray() {
        yield return new WaitUntil(() => !noiseAudio.isPlaying);

        noiseAudio.PlayOneShot(spraySfx);
        WalkieTalkie.TransmitOneShotAudio(noiseAudio, spraySfx);

        sprayParticles.Play();

        yield return new WaitForSeconds(1F);
        sprayParticles.Stop();

        yield return new WaitForSeconds(.5F);
        sprayParticles.Clear();
    }

    public override void EquipItem() {
        base.EquipItem();

        playerHeldBy.equippedUsableItemQE = true;
    }

    public override void PocketItem() {
        playerHeldBy.equippedUsableItemQE = false;

        base.PocketItem();
    }

    public override void DiscardItem() {
        playerHeldBy.equippedUsableItemQE = false;

        base.DiscardItem();
    }

    public override void ItemInteractLeftRight(bool right) {
        base.ItemInteractLeftRight(right);

        if (right) return;

        RoundManager.PlayRandomClip(noiseAudio, sprayCanShakeSfx);
        WalkieTalkie.TransmitOneShotAudio(noiseAudio, sprayCanShakeEmptySfx);

        playerHeldBy.playerBodyAnimator.SetTrigger(_ShakeItemAnimatorHash);
    }
}