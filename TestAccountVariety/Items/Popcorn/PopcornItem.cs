using System;
using System.Collections;
using TestAccountVariety.Utils;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Items.Popcorn;

public class PopcornItem : PhysicsProp {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public AudioSource popcornSound;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Random _random;

    public override void Start() {
        base.Start();

        _random = new((uint)(DateTime.Now.Ticks + transform.position.ConvertToInt()));
    }

    public override void ItemActivate(bool used, bool buttonDown = true) {
        base.ItemActivate(used, buttonDown);

        var localPlayer = StartOfRound.Instance.localPlayerController;
        if (playerHeldBy != localPlayer) return;

        localPlayer.StartCoroutine(localPlayer.waitToEndOfFrameToDiscard());
        EatPopcornServerRpc((uint)localPlayer.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EatPopcornServerRpc(uint playerId) {
        var effect = RollEffect();
        var amount = RollAmount();

        EatPopcornClientRpc(playerId, effect, amount);
        ApplyEffectServer(effect, amount);
    }

    private void ApplyEffectServer(EffectType effectType, int amount) {
        switch (effectType) {
            case EffectType.OVER_HEAL: {
                DestroyItemClientRpc();
                break;
            }
            case EffectType.DEATH_ADDITION: {
                var newValue = scrapValue + (amount * .4);
                SetValueClientRpc((int)newValue);
                break;
            }
            case EffectType.DEATH_REDUCTION: {
                var newValue = scrapValue - (amount * .4);
                SetValueClientRpc((int)newValue);
                break;
            }
            default: throw new ArgumentOutOfRangeException(nameof(effectType), effectType, null);
        }
    }

    [ClientRpc]
    public void EatPopcornClientRpc(uint playerId, EffectType effectType, int amount) {
        popcornSound.Play();

        var localPlayer = StartOfRound.Instance.localPlayerController;
        if (localPlayer.playerClientId != playerId) return;

        ApplyEffect(effectType, amount);
    }

    private int RollAmount() => _random.NextInt(50, 201);

    private static void ApplyEffect(EffectType effectType, int amount) {
        HUDManager.Instance.DisplayTip("Popcorn", "I don't feel so good...", true);
        StartOfRound.Instance.localPlayerController.StartCoroutine(WaitAndApplyEffect(effectType, amount));
    }

    private static IEnumerator WaitAndApplyEffect(EffectType effectType, int amount) {
        yield return new WaitForSeconds(3F);
        yield return new WaitForEndOfFrame();

        var localPlayer = StartOfRound.Instance.localPlayerController;
        if (localPlayer is not {
                isPlayerControlled: true,
                isPlayerDead: false,
            }) yield break;

        switch (effectType) {
            case EffectType.OVER_HEAL: {
                localPlayer.health += amount + 1;
                localPlayer.DamagePlayer(1, false);
                break;
            }

            case EffectType.DEATH_ADDITION:
            case EffectType.DEATH_REDUCTION: {
                localPlayer.DamagePlayer(localPlayer.health + 1);
                break;
            }
            default: throw new ArgumentOutOfRangeException(nameof(effectType), effectType, null);
        }
    }

    private EffectType RollEffect() {
        var roll = _random.NextInt(1, 101);

        return roll switch {
            <= 50 => EffectType.OVER_HEAL,
            <= 60 => EffectType.DEATH_REDUCTION, // 10%
            _ => EffectType.DEATH_ADDITION, // 40%
        };
    }

    [ClientRpc]
    public void DestroyItemClientRpc() => this.DestroyItem();

    [ClientRpc]
    public void SetValueClientRpc(int value) => SetScrapValue(value);
}

public enum EffectType {
    OVER_HEAL = 0,
    DEATH_REDUCTION = 1,
    DEATH_ADDITION = 2,
}