using System;
using UnityEngine;

namespace TestAccountVariety.Items.CleaningDroneToy;

public class TzpFogZone : MonoBehaviour {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SphereCollider collider;
    public ParticleSystem particleSystem;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public float destroyAfter;
    private float _stopwatch;

    private void Start() => particleSystem.Play();

    public void FixedUpdate() {
        var controller = GameNetworkManager.Instance.localPlayerController;

        if (collider.bounds.Contains(controller.playerEye.position)) {
            controller.increasingDrunknessThisFrame = true;
            var drunknessInertia = controller.drunknessInertia + Time.fixedDeltaTime / 0.25f * controller.drunknessSpeed;
            controller.drunknessInertia = Mathf.Clamp(drunknessInertia, 0.1f, 4.5f);
        }

        _stopwatch += Time.fixedDeltaTime;
        if (_stopwatch >= 1.5f) particleSystem.Stop();

        if (_stopwatch >= destroyAfter) Destroy(gameObject);
    }
}