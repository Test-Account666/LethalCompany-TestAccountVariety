using TestAccountVariety.Config;
using TestAccountVariety.Utils;
using UnityEngine;

namespace TestAccountVariety.Items.ChristmasRadio;

public class ChristmasRadio : BoomboxItem {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public AudioClip[] copyRightMusicAudios;
    public AudioClip[] copyRightSafeMusicAudios;
    public AudioClip[] googleMusicAudios;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public void Awake() {
        if (ChristmasRadioConfig.radioMusicType == null!) {
            musicAudios = copyRightSafeMusicAudios;
            return;
        }

        SetMusic();

        ChristmasRadioConfig.radioMusicType.SettingChanged += (_, _) => SetMusic();
    }

    public override void Update() {
        if (!hasBeenHeld) insertedBattery.charge = 1F;

        base.Update();
    }

    public void SetMusic() {
        musicAudios = ChristmasRadioConfig.radioMusicType.Value switch {
            MusicType.COPYRIGHT_SAFE => copyRightSafeMusicAudios,
            MusicType.GOOGLE_TRANSLATE => googleMusicAudios,
            var _ => copyRightMusicAudios,
        };
    }

    public override void Start() {
        base.Start();

        if (!isInFactory) return;

        musicRandomizer = new(playersManager.randomMapSeed - 10 + transform.position.ConvertToInt());

        StartMusic(true);
    }

    public enum MusicType {
        COPYRIGHT,
        COPYRIGHT_SAFE,
        GOOGLE_TRANSLATE,
    }
}