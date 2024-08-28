using UnityEngine;

namespace TestAccountVariety.Items.ShibaPlush.Functions;

public abstract class ShibaPlushFunction : MonoBehaviour {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ShibaPlush shibaPlush;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public abstract void Run();
}