using GameNetcodeStuff;
using UnityEngine;

namespace TestAccountVariety.Hazards.PrisonMine;

public class PrisonMineEnemyTrigger : MonoBehaviour {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public PrisonMine prisonMine;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private void OnTriggerEnter(Collider other) {
        if (!prisonMine.IsHost || VarietyConfig.useBigEnemyCollider.Value) return;

        if (prisonMine.nextTrigger > 0) return;

        var hasPlayer = other.TryGetComponent<PlayerControllerB>(out var _);

        if (hasPlayer) return;

        prisonMine.TriggerAsEnemy(other);
    }
}