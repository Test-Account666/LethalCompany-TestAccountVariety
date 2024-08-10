using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Hazards.GiftMimic;

public class GiftMimic : NetworkBehaviour {
    public static readonly List<string> PossibleNames = [
        "Giftt", "Gift Boxx", "Git Box", "Git", "Lunx Box", "Lunch", "Lunch Box", "Don't Touch", "GIFT BOX", "gift box", "Webley Box",
    ];

    private Random _random;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public AudioSource audioSource;

    public AudioClip audioClip;

    public MeshRenderer meshRenderer;

    public InteractTrigger interactTrigger;

    public ParticleSystem particleSystem;

    public ScanNodeProperties scanNodeProperties;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private void Awake() {
        var currentPosition = transform.position;

        var searchPosition = currentPosition + transform.up * 3;

        var result = Physics.Raycast(searchPosition, -transform.up, out var hitInfo, 20, StartOfRound.Instance.collidersAndRoomMaskAndDefault);

        if (!result) {
            transform.position += transform.up * (transform.localScale.y * .5F);
            return;
        }

        transform.position = hitInfo.point + transform.up * (transform.localScale.y * .5F);
    }

    private void Start() {
        var randomName = PossibleNames[UnityEngine.Random.Range(0, PossibleNames.Count)];

        var randomValue = UnityEngine.Random.Range(23, 53);

        scanNodeProperties.headerText = randomName;
        scanNodeProperties.subText = $"Value: ${randomValue}";
        scanNodeProperties.scrapValue = randomValue;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OpenGiftServerRpc() {
        _random = new((uint) (DateTime.Now.Ticks & 0x0000FFFF));

        List<SpawnableEnemyWithRarity> spawnableEnemies = [
            ..StartOfRound.Instance.currentLevel.Enemies,
        ];

        if (VarietyConfig.giftMimicSpawnsOutsideEnemies.Value) spawnableEnemies.AddRange(StartOfRound.Instance.currentLevel.OutsideEnemies);

        EnemyAI enemyAI;

        while (true) {
            var spawnIndex = _random.NextInt(0, spawnableEnemies.Count);

            var spawnableEnemy = spawnableEnemies[spawnIndex];

            var enemy = Instantiate(spawnableEnemy.enemyType.enemyPrefab, transform.position, Quaternion.identity);

            var hasEnemyAi = enemy.TryGetComponent(out enemyAI);

            if (hasEnemyAi) break;
        }

        enemyAI.isOutside = false;

        var networkObject = enemyAI.NetworkObject;

        networkObject.Spawn();

        OpenGiftClientRpc();
    }

    [ClientRpc]
    public void OpenGiftClientRpc() {
        audioSource.PlayOneShot(audioClip, 1F);

        particleSystem.Play();

        meshRenderer.enabled = false;
        interactTrigger.interactable = false;
        interactTrigger.enabled = false;

        scanNodeProperties.gameObject.SetActive(false);
    }
}