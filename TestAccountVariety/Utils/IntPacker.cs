using UnityEngine;

namespace TestAccountVariety.Utils;

public static class IntPacker {
    private const int _COLOR_MAX_INT = 63;
    private const float _COLOR_MAX = 63f;
    private const float _ALPHA_MAX = 31f;
    private const int _ALPHA_MAX_INT = 31;
    private const float _ALPHA_MIN_VALUE = 0.1f;
    private const float _ALPHA_MAX_VALUE = 0.9f;

    public static int Pack(float red, float green, float blue, float alpha, int sizeIndex, bool enableLights, bool isRainbow) {
        // Quantize RGB to 0..63
        var r = Mathf.Clamp(Mathf.RoundToInt(red * _COLOR_MAX), 0, _COLOR_MAX_INT);
        var g = Mathf.Clamp(Mathf.RoundToInt(green * _COLOR_MAX), 0, _COLOR_MAX_INT);
        var b = Mathf.Clamp(Mathf.RoundToInt(blue * _COLOR_MAX), 0, _COLOR_MAX_INT);

        // Map alpha from [0.1,1] to 0..31
        var aNorm = Mathf.Clamp(alpha, _ALPHA_MIN_VALUE, 1f);
        var a = Mathf.Clamp(Mathf.RoundToInt(((aNorm - _ALPHA_MIN_VALUE) / _ALPHA_MAX_VALUE) * _ALPHA_MAX), 0, _ALPHA_MAX_INT);

        var size = Mathf.Clamp(sizeIndex, 0, 3);
        var lights = enableLights? 1 : 0;
        var rainbow = isRainbow? 1 : 0;

        var packed = 0;
        packed |= (rainbow & 0x1) << 0; // bit 0
        packed |= (lights & 0x1) << 1; // bit 1
        packed |= (size & 0x3) << 2; // bits 2-3
        packed |= (a & 0x1F) << 4; // bits 4-8
        packed |= (b & 0x3F) << 9; // bits 9-14
        packed |= (g & 0x3F) << 15; // bits 15-20
        packed |= (r & 0x3F) << 21; // bits 21-26

        return packed;
    }

    public static void Unpack(int packed, out float red, out float green, out float blue, out float alpha, out int sizeIndex,
        out bool enableLights, out bool isRainbow) {
        var rainbow = (packed >> 0) & 0x1;
        var lights = (packed >> 1) & 0x1;
        var size = (packed >> 2) & 0x3;
        var a = (packed >> 4) & 0x1F;
        var b = (packed >> 9) & 0x3F;
        var g = (packed >> 15) & 0x3F;
        var r = (packed >> 21) & 0x3F;

        // De-quantize
        red = r / _COLOR_MAX;
        green = g / _COLOR_MAX;
        blue = b / _COLOR_MAX;

        // Alpha back to [0.1, 1]
        var aNorm = a / _ALPHA_MAX;
        alpha = _ALPHA_MIN_VALUE + aNorm * _ALPHA_MAX_VALUE;

        sizeIndex = size;
        enableLights = lights == 1;
        isRainbow = rainbow == 1;
    }
}