using System;
using TestAccountVariety.Config;
using TestAccountVariety.Utils;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Hazards.LaserEmitter;

public class LaserEmitter : NetworkBehaviour {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public GameObject laser;
    public LaserCollider laserCollider;
    public ParticleSystem laserParticles;
    public AudioSource laserAudio;
    public Light laserLight;

    public float maxCeilingDistance;

    public float maxLeftDistance;
    public float minLeftDistance;

    public float maxRightDistance;
    public float minRightDistance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Vector3 ceilingPosition;
    public float distance;
    public Vector3 leftEndPosition;
    public Vector3 rightEndPosition;

    public float laserSpeed;
    public bool reverse;

    public bool valuesSynced;

    public void Start() => SetupLaserServerRpc();

    public void Update() {
        if (!valuesSynced) return;

        transform.position = Vector3.Lerp(transform.position, reverse? leftEndPosition : rightEndPosition, Time.deltaTime * laserSpeed);

        var endDistance = Vector3.Distance(transform.position, reverse? leftEndPosition : rightEndPosition);

        if (endDistance >= .1) return;
        reverse = !reverse;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetupLaserServerRpc() {
        SetupLaser();
        SetupLaserClientRpc(ceilingPosition, distance, leftEndPosition, rightEndPosition, laserSpeed, reverse);
    }

    public void SetupLaser() {
        var random = new Random((uint) (DateTime.Now.Ticks + transform.position.ConvertToInt()));
        laserSpeed = random.NextFloat(LaserEmitterConfig.laserMinimumMovementSpeed.Value, LaserEmitterConfig.laserMaximumMovementSpeed.Value);

        var hitCeiling = Physics.Linecast(transform.position, transform.position + transform.up * maxCeilingDistance, out var ceilingInfo, 1 << 8);
        distance = maxCeilingDistance;

        if (hitCeiling) distance = ceilingInfo.distance;

        ceilingPosition = transform.position + transform.up * distance;

        var leftDistance = random.NextFloat(minLeftDistance, maxLeftDistance);
        var rightDistance = random.NextFloat(minRightDistance, maxRightDistance);

        rightDistance = GetWallDistance(ceilingPosition - transform.up, transform.right, rightDistance);
        leftDistance = GetWallDistance(ceilingPosition - transform.up, -transform.right, leftDistance);


        leftEndPosition = ceilingPosition + -transform.right * leftDistance;
        rightEndPosition = ceilingPosition + transform.right * rightDistance;

        reverse = random.NextBool();
    }

    public static float GetWallDistance(Vector3 position, Vector3 direction, float maxDistance) {
        var wallDistance = maxDistance;

        var wallHits = new RaycastHit[10];

        var wallHitCount = Physics.RaycastNonAlloc(position, direction, wallHits, maxDistance, 1 << 8 | 1 << 0);

        if (wallHitCount <= 0) return wallDistance;

        for (var index = 0; index < wallHitCount; index++) {
            var wallInfo = wallHits[index];

            if (wallDistance <= wallInfo.distance - .5F) continue;

            wallDistance = wallInfo.distance - .5F;
        }

        return wallDistance;
    }

    // ReSharper disable ParameterHidesMember
    [ClientRpc]
    public void SetupLaserClientRpc(Vector3 ceilingPosition, float distance, Vector3 leftEndPosition, Vector3 rightEndPosition, float laserSpeed, bool reverse) {
        this.ceilingPosition = ceilingPosition;
        this.leftEndPosition = leftEndPosition;
        this.rightEndPosition = rightEndPosition;
        this.laserSpeed = laserSpeed;

        transform.position = ceilingPosition;
        transform.Rotate(0, 0, 180);

        laser.transform.localScale += new Vector3(0, 0, (distance - .5F) * 100F);
        laser.transform.position += transform.up * ((distance - .5F) / 2);

        valuesSynced = true;

        laserParticles.Play();
        laserAudio.Play();
        laserLight.enabled = true;

        laser.GetComponent<MeshRenderer>().enabled = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetLaserEnabledServerRpc(bool enabled) {
        SetLaserEnabledClientRpc(enabled);
    }

    [ClientRpc]
    public void SetLaserEnabledClientRpc(bool enabled) {
        laser.SetActive(false);

        if (enabled) {
            laserParticles.Play();
            laserAudio.Play();
            laserLight.enabled = true;
        } else {
            laserParticles.Stop();
            laserAudio.Stop();
            laserLight.enabled = false;
        }
    }
    // ReSharper restore ParameterHidesMember
}