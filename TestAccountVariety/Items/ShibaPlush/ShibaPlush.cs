using System;
using System.Collections.Generic;
using TestAccountVariety.Items.ShibaPlush.Functions;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Items.ShibaPlush;

public class ShibaPlush : NoisemakerProp {
    private static readonly int _SqueezeAnimatorHash = Animator.StringToHash("Squeeze");
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    // ReSharper disable once UnassignedField.Global
    // ReSharper disable once CollectionNeverUpdated.Global
    public ShibaPlushFunction[] shibaPlushFunctions;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public override void ItemActivate(bool used, bool buttonDown = true) {
        var random = new Random((uint) (DateTime.Now.Ticks & 0x0000FFFF));

        RunActionServerRpc(random.NextInt(0, shibaPlushFunctions.Length));
    }

    [ServerRpc(RequireOwnership = false)]
    public void RunActionServerRpc(int index) {
        RunActionClientRpc(index);
    }

    [ClientRpc]
    public void RunActionClientRpc(int index) {
        triggerAnimator.SetTrigger(_SqueezeAnimatorHash);

        var shibaPlushFunction = shibaPlushFunctions[index];

        shibaPlushFunction.Run();
    }
}