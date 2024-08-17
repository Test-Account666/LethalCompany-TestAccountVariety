using UnityEngine;

namespace TestAccountVariety.Items.WebleyPyramid;

[CreateAssetMenu(menuName = "ScriptableObjects/AudioClipWithWeight", order = 1)]
public class AudioClipWithWeight : ScriptableObject {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public AudioClip audioClip;

    public int weight;

    public bool isDeadly;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}