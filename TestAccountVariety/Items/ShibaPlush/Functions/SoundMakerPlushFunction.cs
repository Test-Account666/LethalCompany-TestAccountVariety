using UnityEngine;

namespace TestAccountVariety.Items.ShibaPlush.Functions;

public abstract class SoundMakerPlushFunction : ShibaPlushFunction {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public AudioClip audioClip;
    public AudioClip farAudioClip;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public override void Run() {
        if (GameNetworkManager.Instance.localPlayerController == null) return;

        var volume = shibaPlush.noisemakerRandom.Next((int) (shibaPlush.minLoudness * 100.0), (int) (shibaPlush.maxLoudness * 100.0)) / 100f;
        var pitch = shibaPlush.noisemakerRandom.Next((int) (shibaPlush.minPitch * 100.0), (int) (shibaPlush.maxPitch * 100.0)) / 100f;

        shibaPlush.noiseAudio.pitch = pitch;
        shibaPlush.noiseAudio.PlayOneShot(audioClip, volume);

        shibaPlush.noiseAudioFar.pitch = pitch;
        shibaPlush.noiseAudioFar.PlayOneShot(farAudioClip, volume);

        WalkieTalkie.TransmitOneShotAudio(shibaPlush.noiseAudio, audioClip, volume);
        RoundManager.Instance.PlayAudibleNoise(transform.position, shibaPlush.noiseRange, volume,
                                               noiseIsInsideClosedShip: shibaPlush.isInElevator && StartOfRound.Instance.hangarDoorsClosed);

        if (shibaPlush.minLoudness < 0.6000000238418579 || !(shibaPlush.playerHeldBy != null)) return;
        shibaPlush.playerHeldBy.timeSinceMakingLoudNoise = 0.0f;
    }
}