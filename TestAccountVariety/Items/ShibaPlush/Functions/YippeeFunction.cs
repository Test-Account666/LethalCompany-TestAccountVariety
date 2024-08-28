using UnityEngine;

namespace TestAccountVariety.Items.ShibaPlush.Functions;

public class YippeeFunction : SoundMakerPlushFunction {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ParticleSystem particleSystem;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public override void Run() {
        if (GameNetworkManager.Instance.localPlayerController == null) return;

        if (!particleSystem) return;

        base.Run();

        particleSystem.time = 0F;
        particleSystem.Play();
    }
}