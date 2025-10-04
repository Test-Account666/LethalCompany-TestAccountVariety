using System;
using System.Collections;
using TestAccountVariety.Config;
using TestAccountVariety.Utils;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Hazards.AcidSpitter;

public class AcidSpitter : NetworkBehaviour {
#pragma warning disable CS8618
    public AcidSpitterParticleCollider acidCollider;

    public ParticleSystem acidParticles;
    public AudioSource acidAudio;

    public GameObject rails;
    public Transform spitterTransform;

    public float maxCeilingDistance;
    public float maxLeftDistance;
    public float minLeftDistance;
    public float maxRightDistance;
    public float minRightDistance;
#pragma warning restore CS8618

    private Vector3 _ceilingPosition;
    private Vector3 _leftEndPosition;
    private Vector3 _rightEndPosition;
    private float _spitterSpeed;
    private bool _reverse;

    private readonly NetworkVariable<Vector3> _serverPosition = new();

    private readonly NetworkVariable<Vector3> _serverSpitterPosition = new();

    private readonly NetworkVariable<Vector3> _serverRailsPosition = new();
    private readonly NetworkVariable<Vector3> _serverRailsScale = new();

    private bool _networkSpawned;

    private const float _POSITION_UPDATE_THRESHOLD = 0.01f;
    private const float _CEILING_DISTANCE_BUFFER = .45F;

    public override void OnNetworkSpawn() {
        _serverPosition.OnValueChanged += OnServerPositionChanged;

        _serverSpitterPosition.OnValueChanged += OnServerSpitterPositionChanged;

        _networkSpawned = true;

        if (!IsServer && !IsHost) return;

        InitializeAcidSpitter();
        UpdateAcidSpitterValues();
    }

    public override void OnNetworkDespawn() {
        base.OnNetworkDespawn();

        _serverPosition.OnValueChanged -= OnServerPositionChanged;

        _serverSpitterPosition.OnValueChanged -= OnServerSpitterPositionChanged;
    }

    private void Update() {
        if (!IsServer && !IsHost) return;

        MoveAcidSpitter();
    }

    private void Start() {
        if (IsServer || IsHost) return;

        UpdateAcidColor();
    }

    private void InitializeAcidSpitter() {
        UpdateAcidColor();

        var random = new Random((uint)(DateTime.Now.Ticks + transform.position.ConvertToInt()));
        _spitterSpeed = random.NextFloat(AcidSpitterConfig.spitterMinimumMovementSpeed.Value,
            AcidSpitterConfig.spitterMaximumMovementSpeed.Value);

        var hitCeiling = Physics.Linecast(transform.position, transform.position + transform.up * maxCeilingDistance,
            out var ceilingInfo, 1 << 8);
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

        transform.position = _ceilingPosition;
    }

    public void UpdateAcidSpitterValues() {
        _serverPosition.Value = _ceilingPosition;
        _serverRailsScale.Value = rails.transform.localScale;
        _serverRailsPosition.Value = rails.transform.localPosition;
    }

    private void LateUpdate() {
        rails.transform.localPosition = _serverRailsPosition.Value;
        rails.transform.localScale = _serverRailsScale.Value;
    }

    private void UpdateAcidColor() {
        var hasRenderer = acidParticles.TryGetComponent<ParticleSystemRenderer>(out var renderer);

        if (!hasRenderer) {
            TestAccountVariety.Logger.LogError("Couldn't find particle system renderer!");
            return;
        }

        var red = AcidSpitterConfig.acidColorRed.Value / 255F;
        var green = AcidSpitterConfig.acidColorGreen.Value / 255F;
        var blue = AcidSpitterConfig.acidColorBlue.Value / 255F;

        renderer.sharedMaterial.color = new(red, green, blue);

        var hasSubRenderer = acidParticles.subEmitters.GetSubEmitterSystem(0).TryGetComponent<ParticleSystemRenderer>(out var subRenderer);

        if (!hasSubRenderer) {
            TestAccountVariety.Logger.LogError("Couldn't find particle system sub-renderer!");
            return;
        }

        subRenderer.sharedMaterial.color = new(red, green, blue);
    }

    private void MoveAcidSpitter() {
        var targetPosition = _reverse? _leftEndPosition : _rightEndPosition;
        spitterTransform.position = Vector3.Lerp(spitterTransform.position, targetPosition, Time.deltaTime * _spitterSpeed);

        if (Vector3.Distance(spitterTransform.position, targetPosition) < 0.1f) _reverse = !_reverse;

        if (Vector3.Distance(spitterTransform.position, _serverSpitterPosition.Value) < _POSITION_UPDATE_THRESHOLD) return;
        _serverSpitterPosition.Value = spitterTransform.position;
    }

    private void OnServerPositionChanged(Vector3 previous, Vector3 current) => transform.position = current;

    private void OnServerSpitterPositionChanged(Vector3 previous, Vector3 current) => spitterTransform.position = current;

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