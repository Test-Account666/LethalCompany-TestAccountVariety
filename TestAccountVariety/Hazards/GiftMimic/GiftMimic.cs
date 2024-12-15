using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TestAccountVariety.Config;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Hazards.GiftMimic;

public class GiftMimic : NetworkBehaviour {
    public static readonly List<string> PossibleNames = [
        "Giftt", "Gift Boxx", "Git Box", "Git", "Lunx Box", "Lunch", "Lunch Box", "Don't Touch", "GIFT BOX", "gift box", "Webley Box", "Git Hub", "Present",
        "Haha Box",
    ];

    public string? variationName;

    private Random _random;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public AudioSource audioSource;

    public AudioClip audioClip;

    public InteractTrigger interactTrigger;
    public BoxCollider interactCollider;

    public ParticleSystem particleSystem;

    public ScanNodeProperties scanNodeProperties;

    public GameObject mapRadarPlane;

    public GameObject giftBoxArt;
    public GameObject giftBoxArtAlternate;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        SyncGiftBoxArtServerRpc();
    }

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
        StartCoroutine(SpawnEnemyOrScrap(1F));
    }

    public IEnumerator SpawnEnemyOrScrap(float timeOut) {
        _random = new((uint) (DateTime.Now.Ticks & 0x0000FFFF));

        var rolledChance = _random.NextInt(1, 100);

        yield return rolledChance < GiftMimicConfig.giftMimicScrapChance.Value? SpawnScrap(timeOut) : SpawnEnemy(timeOut);
    }

    public IEnumerator SpawnScrap(float timeOut) {
        List<SpawnableItemWithRarity> spawnableItems = [
            ..StartOfRound.Instance.currentLevel.spawnableScrap,
        ];

        var blackList = GiftMimicConfig.giftMimicScrapBlacklist.Value.Replace(", ", ",").Split(",").ToHashSet();

        spawnableItems.RemoveAll(scrap => blackList.Any(blackListedScrap => scrap.spawnableItem.itemName.ToLower().StartsWith(blackListedScrap.ToLower())));

        while (true) {
            yield return new WaitForEndOfFrame();

            timeOut -= Time.deltaTime;

            if (timeOut <= 0) {
                TestAccountVariety.Logger.LogFatal("Failed to spawn scrap from Gift Mimic! Are scraps available?");
                yield break;
            }

            var spawnIndex = _random.NextInt(0, spawnableItems.Count);

            var spawnableItem = spawnableItems[spawnIndex];

            if (spawnableItem.rarity <= 0) continue;

            var spawnPosition = transform.position + transform.up * 2;

            var itemProperties = spawnableItem.spawnableItem;

            var itemObject = Instantiate(itemProperties.spawnPrefab, spawnPosition, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
            var grabbableObject = itemObject.GetComponent<GrabbableObject>();
            grabbableObject.transform.rotation = Quaternion.Euler(grabbableObject.itemProperties.restingRotation);
            grabbableObject.fallTime = 0.0f;
            grabbableObject.scrapValue = (int) (_random.NextInt(itemProperties.minValue, itemProperties.maxValue) * RoundManager.Instance.scrapValueMultiplier);

            var networkObject = itemObject.GetComponent<NetworkObject>();
            networkObject.Spawn();

            grabbableObject.SetScrapValue(grabbableObject.scrapValue);
            break;
        }

        OpenGiftClientRpc();
    }

    public IEnumerator SpawnEnemy(float timeOut) {
        List<SpawnableEnemyWithRarity> spawnableEnemies = [
            ..StartOfRound.Instance.currentLevel.Enemies,
        ];

        if (GiftMimicConfig.giftMimicSpawnsOutsideEnemies.Value) spawnableEnemies.AddRange(StartOfRound.Instance.currentLevel.OutsideEnemies);

        var blackList = GiftMimicConfig.giftMimicEnemyBlacklist.Value.Replace(", ", ",").Split(",").ToHashSet();

        spawnableEnemies.RemoveAll(enemy => blackList.Any(blackListedEnemy => enemy.enemyType.enemyName.ToLower().StartsWith(blackListedEnemy.ToLower())));

        while (true) {
            yield return new WaitForEndOfFrame();

            timeOut -= Time.deltaTime;

            if (timeOut <= 0) {
                TestAccountVariety.Logger.LogFatal("Failed to spawn enemy from Gift Mimic! Are enemies available?");
                yield break;
            }

            var spawnIndex = _random.NextInt(0, spawnableEnemies.Count);

            var spawnableEnemy = spawnableEnemies[spawnIndex];

            if (spawnableEnemy.rarity <= 0) continue;

            var spawnPosition = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(transform.position, 5f);

            RoundManager.Instance.SpawnEnemyGameObject(spawnPosition, 0, -1, spawnableEnemy.enemyType);
            break;
        }

        OpenGiftClientRpc();
    }

    [ClientRpc]
    public void OpenGiftClientRpc() {
        audioSource.PlayOneShot(audioClip, 1F);

        particleSystem.Play();

        mapRadarPlane.SetActive(false);

        giftBoxArt.SetActive(false);
        giftBoxArtAlternate.SetActive(false);

        //For some reason, interactTrigger can be null here...
        if (interactTrigger) {
            interactTrigger.interactable = false;
            interactTrigger.enabled = false;
            interactCollider.enabled = false;
        }

        scanNodeProperties.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncGiftBoxArtServerRpc() {
        if (string.IsNullOrWhiteSpace(variationName)) {
            var random = new System.Random();
            var generatedChance = random.Next(1, 100);

            variationName = generatedChance > GiftMimicConfig.giftMimicAlternativeVariantChance.Value? "Normal" : "Upturned";
        }

        SyncGiftBoxArtClientRpc(variationName);
    }

    [ClientRpc]
    public void SyncGiftBoxArtClientRpc(string variation) {
        switch (variation) {
            case "Normal":
                giftBoxArt.SetActive(true);
                giftBoxArtAlternate.SetActive(false);
                break;
            case "Upturned":
                giftBoxArt.SetActive(false);
                giftBoxArtAlternate.SetActive(true);
                break;
        }
    }
}