using System;
using System.Collections;
using TestAccountVariety.Config;
using TestAccountVariety.Utils;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Hazards.LaserEmitter;

public class NewLaserEmitter : LaserEmitter {
#pragma warning disable CS8618
    public NewLaserParticleCollider laserCollider;

    public ParticleSystem laserParticles;
    public AudioSource laserAudio;

    [Obsolete]
    public Light laserLight;

    public GameObject rails;
    public Transform laserTransform;

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

    private readonly NetworkVariable<Vector3> _serverLaserPosition = new();

    private readonly NetworkVariable<Vector3> _serverRailsPosition = new();
    private readonly NetworkVariable<Vector3> _serverRailsScale = new();

    private bool _networkSpawned;

    private const float _POSITION_UPDATE_THRESHOLD = 0.01f;
    private const float _CEILING_DISTANCE_BUFFER = .45F;

    public override void OnNetworkSpawn() {
        _serverPosition.OnValueChanged += OnServerPositionChanged;

        _serverLaserPosition.OnValueChanged += OnServerLaserPositionChanged;

        _serverRailsPosition.OnValueChanged += OnServerRailsPositionChanged;
        _serverRailsScale.OnValueChanged += OnServerRailsScaleChanged;

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

        _serverLaserPosition.OnValueChanged -= OnServerLaserPositionChanged;

        _serverRailsPosition.OnValueChanged -= OnServerRailsPositionChanged;
        _serverRailsScale.OnValueChanged -= OnServerRailsScaleChanged;
    }

    private void Update() {
        if (!IsServer && !IsHost) return;

        MoveLaser();
    }

    private void InitializeLaser() {
        var random = new Random((uint) (DateTime.Now.Ticks + transform.position.ConvertToInt()));
        _laserSpeed = random.NextFloat(LaserEmitterConfig.laserMinimumMovementSpeed.Value, LaserEmitterConfig.laserMaximumMovementSpeed.Value);

        var hitCeiling = Physics.Linecast(transform.position, transform.position + transform.up * maxCeilingDistance, out var ceilingInfo, 1 << 8);
        var distance = hitCeiling? ceilingInfo.distance - _CEILING_DISTANCE_BUFFER : maxCeilingDistance;

        _ceilingPosition = transform.position + transform.up * distance;

        var leftDistance = random.NextFloat(minLeftDistance, maxLeftDistance);
        var rightDistance = random.NextFloat(minRightDistance, maxRightDistance);

        leftDistance = GetWallDistance(_ceilingPosition - transform.up, -transform.forward, leftDistance);
        rightDistance = GetWallDistance(_ceilingPosition - transform.up, transform.forward, rightDistance);


        var fullDistance = leftDistance + rightDistance;

        rails.transform.localScale += new Vector3(0, 0, fullDistance);

        var centerOffset = (rightDistance - leftDistance) / 2;
        rails.transform.localPosition += new Vector3(0, 0, centerOffset);

        _leftEndPosition = _ceilingPosition + -transform.forward * leftDistance;
        _rightEndPosition = _ceilingPosition + transform.forward * rightDistance;

        _reverse = random.NextBool();
        _serverPosition.Value = _ceilingPosition;

        transform.position = _ceilingPosition;
    }

    private void MoveLaser() {
        var targetPosition = _reverse? _leftEndPosition : _rightEndPosition;
        laserTransform.position = Vector3.Lerp(laserTransform.position, targetPosition, Time.deltaTime * _laserSpeed);

        if (Vector3.Distance(laserTransform.position, targetPosition) < 0.1f) _reverse = !_reverse;

        if (Vector3.Distance(laserTransform.position, _serverLaserPosition.Value) >= _POSITION_UPDATE_THRESHOLD)
            _serverLaserPosition.Value = laserTransform.position;
    }

    private void OnServerPositionChanged(Vector3 previous, Vector3 current) => transform.position = current;

    private void OnServerLaserPositionChanged(Vector3 previous, Vector3 current) => laserTransform.position = current;

    private void OnServerRailsPositionChanged(Vector3 previous, Vector3 current) => rails.transform.position = current;

    private void OnServerRailsScaleChanged(Vector3 previous, Vector3 current) => rails.transform.localScale = current;

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