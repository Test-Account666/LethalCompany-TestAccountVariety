using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace TestAccountVariety.Items.Dice;

public class Dice : GrabbableObject {
    private static readonly int _RolledAnimatorHash = Animator.StringToHash("Rolled");
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Animator animator;

    public float animationDuration;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public override void GrabItem() {
        base.GrabItem();

        ResetDiceServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetDiceServerRpc() {
        ResetDiceClientRpc();
    }

    [ClientRpc]
    public void ResetDiceClientRpc() {
        animator.SetInteger(_RolledAnimatorHash, 0);
    }

    public override void ItemActivate(bool used, bool buttonDown = true) {
        base.ItemActivate(used, buttonDown);

        if (playerHeldBy == null || !playerHeldBy) return;

        var localPlayer = StartOfRound.Instance.localPlayerController;

        if (playerHeldBy != localPlayer) return;

        localPlayer.StartCoroutine(localPlayer.waitToEndOfFrameToDiscard());

        SpinDiceServerRpc((int) localPlayer.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpinDiceServerRpc(int player) {
        var random = new Random();

        var roll = random.Next(0, 6) + 1;

        SpinDiceClientRpc(player, roll);
    }

    [ClientRpc]
    public void SpinDiceClientRpc(int player, int result) {
        if (player < 0) return;

        if (player >= StartOfRound.Instance.allPlayerScripts.Length) return;

        var playerScript = StartOfRound.Instance.allPlayerScripts[player];

        if (playerScript == null || !playerScript) return;

        animator.SetInteger(_RolledAnimatorHash, result);

        grabbable = false;

        var username = playerScript.playerUsername;

        StartCoroutine(WaitAndSendResult(animationDuration, username, result));
    }

    public IEnumerator WaitAndSendResult(float secondsToWait, string username, int result) {
        yield return new WaitForSeconds(secondsToWait);

        HUDManager.Instance.AddChatMessage($"<color=yellow>{username}</color> rolled <color=red>{result}</color>!");

        grabbable = true;
    }
}