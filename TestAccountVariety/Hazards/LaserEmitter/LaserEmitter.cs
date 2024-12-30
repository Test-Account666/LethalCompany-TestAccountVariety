using System;
using System.Collections;
using TestAccountVariety.Config;
using TestAccountVariety.Utils;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Hazards.LaserEmitter;

public class LaserEmitter : NetworkBehaviour {
#pragma warning disable CS8618
    public LaserParticleCollider laserCollider;
    public ParticleSystem laserParticles;
    public AudioSource laserAudio;
    public Light laserLight;

    public float maxCeilingDistance;
    public float maxLeftDistance;
    public float minLeftDistance;
    public float maxRightDistance;
    public float minRightDistance;
#pragma warning restore CS8618

    private Vector3 _ceilingPosition;
    private Vector3 _leftEndPosition;
    private Vector3 _rightEndPosition;
    private float _laserSpeed;
    private bool _reverse;

    private readonly NetworkVariable<Vector3> _serverPosition = new();

    private bool _networkSpawned;

    private const float _POSITION_UPDATE_THRESHOLD = 0.01f;

    public override void OnNetworkSpawn() {
        _serverPosition.OnValueChanged += OnServerPositionChanged;

        _networkSpawned = true;
    }

    private void Start() {
        if (!IsServer && !IsHost) {
            transform.Rotate(0, 0, 180);
            return;
        }

        StartCoroutine(WaitUntilNetworkSpawned());
    }

    public IEnumerator WaitUntilNetworkSpawned() {
        yield return new WaitUntil(() => _networkSpawned);
        InitializeLaser();
    }

    public override void OnNetworkDespawn() {
        base.OnNetworkDespawn();

        _serverPosition.OnValueChanged -= OnServerPositionChanged;
    }

    private void Update() {
        if (!IsServer && !IsHost) return;

        MoveLaser();
    }

    private void InitializeLaser() {
        var random = new Random((uint) (DateTime.Now.Ticks + transform.position.ConvertToInt()));
        _laserSpeed = random.NextFloat(LaserEmitterConfig.laserMinimumMovementSpeed.Value, LaserEmitterConfig.laserMaximumMovementSpeed.Value);

        var hitCeiling = Physics.Linecast(transform.position, transform.position + transform.up * maxCeilingDistance, out var ceilingInfo, 1 << 8);
        var distance = hitCeiling? ceilingInfo.distance : maxCeilingDistance;

        _ceilingPosition = transform.position + transform.up * distance;

        var leftDistance = random.NextFloat(minLeftDistance, maxLeftDistance);
        var rightDistance = random.NextFloat(minRightDistance, maxRightDistance);

        leftDistance = GetWallDistance(_ceilingPosition - transform.up, -transform.right, leftDistance);
        rightDistance = GetWallDistance(_ceilingPosition - transform.up, transform.right, rightDistance);

        _leftEndPosition = _ceilingPosition + -transform.right * leftDistance;
        _rightEndPosition = _ceilingPosition + transform.right * rightDistance;

        _reverse = random.NextBool();
        _serverPosition.Value = _ceilingPosition;

        transform.position = _ceilingPosition;
        transform.Rotate(0, 0, 180);
    }

    private void MoveLaser() {
        var targetPosition = _reverse? _leftEndPosition : _rightEndPosition;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * _laserSpeed);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f) _reverse = !_reverse;

        if (Vector3.Distance(transform.position, _serverPosition.Value) >= _POSITION_UPDATE_THRESHOLD) _serverPosition.Value = transform.position;
    }

    private void OnServerPositionChanged(Vector3 previous, Vector3 current) => transform.position = current;

    public static float GetWallDistance(Vector3 position, Vector3 direction, float maxDistance) {
        var wallDistance = maxDistance;
        var wallHits = new RaycastHit[10];

        var wallHitCount = Physics.RaycastNonAlloc(position, direction, wallHits, maxDistance, 1 << 8 | 1 << 0);
        if (wallHitCount <= 0) return wallDistance;

        for (var index = 0; index < wallHitCount; index++) {
            var wallInfo = wallHits[index];
            if (wallDistance <= wallInfo.distance - 0.5f) continue;

            wallDistance = wallInfo.distance - 0.5f;
        }

        return wallDistance;
    }
}