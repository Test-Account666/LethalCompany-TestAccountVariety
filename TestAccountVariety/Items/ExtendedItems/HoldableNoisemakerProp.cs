using Unity.Netcode;

namespace TestAccountVariety.Items.ExtendedItems;

public class HoldableNoisemakerProp : NoisemakerProp {
    public override void Update() {
        base.Update();

        if (!noiseAudio.isPlaying) return;

        currentUseCooldown = useCooldown;
    }

    public override void ItemActivate(bool used, bool buttonDown = true) {
        var localPlayer = GameNetworkManager.Instance.localPlayerController;

        if (localPlayer == null) return;
        if (playerHeldBy != localPlayer) return;

        if (!buttonDown) {
            StopSoundServerRpc();
            return;
        }

        var index = noisemakerRandom.Next(0, noiseSFX.Length);

        var loudness = noisemakerRandom.Next((int) (minLoudness * 100.0), (int) (maxLoudness * 100.0)) / 100f;
        var pitch = noisemakerRandom.Next((int) (minPitch * 100.0), (int) (maxPitch * 100.0)) / 100f;

        StartSoundServerRpc(index, loudness, pitch);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopSoundServerRpc() {
        StopSoundClientRpc();
    }

    [ClientRpc]
    public void StopSoundClientRpc() {
        noiseAudio.Stop();
        noiseAudioFar.Stop();
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartSoundServerRpc(int index, float loudness, float pitch) {
        StartSoundClientRpc(index, loudness, pitch);
    }

    [ClientRpc]
    public void StartSoundClientRpc(int index, float loudness, float pitch) {
        noiseAudio.Stop();
        noiseAudioFar.Stop();

        noiseAudio.loop = true;
        noiseAudio.clip = noiseSFX[index];
        noiseAudio.volume = loudness;
        noiseAudio.pitch = pitch;
        noiseAudio.Play();

        noiseAudioFar.loop = true;
        noiseAudioFar.clip = noiseSFXFar[index];
        noiseAudioFar.volume = loudness;
        noiseAudioFar.pitch = pitch;
        noiseAudioFar.Play();

        //WalkieTalkie.TransmitOneShotAudio(noiseAudio, noiseSFX[index], loudness);
        RoundManager.Instance.PlayAudibleNoise(transform.position, noiseRange, loudness, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);

        if (minLoudness < 0.6000000238418579 || playerHeldBy == null) return;
        playerHeldBy.timeSinceMakingLoudNoise = 0.0f;
    }
}