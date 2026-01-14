using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TestAccountVariety.Items.Parrot;

public class Parrot : NoisemakerProp {
    private static readonly IList<ulong> _INSIDER_JOKES_IDS = new List<ulong> {76561198195911589L, 76561198984467725L,}.AsReadOnly();

    public AudioClip[] insiderJokes = [];
    public AudioClip[] normalLines = [];
    private AudioClip[] _allLines = [];

    #region Overrides of NoisemakerProp

    public override void Start() {
        base.Start();

        var allLines = new List<AudioClip>(normalLines);
        allLines.AddRange(insiderJokes.Repeat(3));

        _allLines = allLines.ToArray();
    }

    public override void Update() {
        base.Update();

        if (noiseAudio.isPlaying) currentUseCooldown = useCooldown;
    }

    public override void ItemActivate(bool used, bool buttonDown = true) {
        if (noisemakerRandom.Next(0, 100) <= 3) {
            noiseSFX = insiderJokes;
            base.ItemActivate(used, buttonDown);
            return;
        }

        noiseSFX = !playerHeldBy || !_INSIDER_JOKES_IDS.Contains(playerHeldBy.playerSteamId)? normalLines : _allLines;

        base.ItemActivate(used, buttonDown);
    }

    #endregion
}