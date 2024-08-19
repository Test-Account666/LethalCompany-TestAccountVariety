using Unity.Netcode;

namespace TestAccountVariety.Items.ToyCar;

public class ToyCar : NoisemakerProp {
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
            StopHonkServerRpc();
            return;
        }

        var index = noisemakerRandom.Next(0, noiseSFX.Length);

        var loudness = noisemakerRandom.Next((int) (minLoudness * 100.0), (int) (maxLoudness * 100.0)) / 100f;
        var pitch = noisemakerRandom.Next((int) (minPitch * 100.0), (int) (maxPitch * 100.0)) / 100f;

        HonkServerRpc(index, loudness, pitch);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopHonkServerRpc() {
        StopHonkClientRpc();
    }

    [ClientRpc]
    public void StopHonkClientRpc() {
        noiseAudio.Stop();
        noiseAudioFar.Stop();
    }

    [ServerRpc(RequireOwnership = false)]
    public void HonkServerRpc(int index, float loudness, float pitch) {
        HonkClientRpc(index, loudness, pitch);
    }

    [ClientRpc]
    public void HonkClientRpc(int index, float loudness, float pitch) {
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

        WalkieTalkie.TransmitOneShotAudio(noiseAudio, noiseSFX[index], loudness);
        RoundManager.Instance.PlayAudibleNoise(transform.position, noiseRange, loudness, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);

        if (minLoudness < 0.6000000238418579 || playerHeldBy == null) return;
        playerHeldBy.timeSinceMakingLoudNoise = 0.0f;
    }
}