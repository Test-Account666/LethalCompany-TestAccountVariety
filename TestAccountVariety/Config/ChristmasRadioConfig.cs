using BepInEx.Configuration;
using TestAccountVariety.Items.ChristmasRadio;

namespace TestAccountVariety.Config;

// ReSharper disable once ClassNeverInstantiated.Global
internal class ChristmasRadioConfig : VarietyConfig {
    public static ConfigEntry<ChristmasRadio.MusicType> radioMusicType = null!;

    public override void Initialize(ConfigFile configFile) {
        radioMusicType = configFile.Bind("Christmas Radio", "6. Music Type", ChristmasRadio.MusicType.COPYRIGHT_SAFE,
                                         "What music type to play. Beware that only 'copyright safe' can be used for streams and videos!");
    }
}