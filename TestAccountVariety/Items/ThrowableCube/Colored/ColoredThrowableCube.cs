using System;
using TestAccountVariety.Config;
using TestAccountVariety.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Items.ThrowableCube.Colored;

public class ColoredThrowableCube : ThrowableCube {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public MeshRenderer renderer;

    public Material opaqueMaterial;
    public Material transparentMaterial;

    [FormerlySerializedAs("light")]
    public Light[] lights;

    public Item[] itemPropertiesBySize;
    public float[] sizes;

    public LightList[] lightLists;

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public Random colorRandom;

    public bool choseColor;

    public float red;
    public float green;
    public float blue;

    public float alpha;

    public int sizeIndex;

    public bool enableLights;

    public bool isRainbow;

    public float currentColorHSV;
    public bool reverse;

    public const float HSV_INCREMENT = 0.1F;


    public override void Start() {
        base.Start();

        if (itemPropertiesBySize.Length > 0)
            foreach (var item in itemPropertiesBySize)
                item.itemId = itemProperties.itemId;

        foreach (var lightList in lightLists)
            foreach (var light in lightList.lights)
                light.enabled = false;

        var seed = (uint) (StartOfRound.Instance.randomMapSeed + transform.position.ConvertToInt() + (DateTime.Now.Ticks & 0x0000FFFF));

        colorRandom = new(seed);

        SyncColorServerRpc();
    }

    private void FixedUpdate() {
        if (!isRainbow) return;

        currentColorHSV += (reverse? -HSV_INCREMENT : HSV_INCREMENT) * Time.deltaTime;

        switch (currentColorHSV) {
            case >= 1F when !reverse:
                currentColorHSV = 1F;
                reverse = true;
                break;
            case <= 0F when reverse:
                currentColorHSV = 0F;
                reverse = false;
                break;
        }

        UpdateColor(true, false);
    }

    public override void EquipItem() {
        base.EquipItem();

        ActivateLights(true);
    }

    public override void GrabItem() {
        base.GrabItem();

        ActivateLights(true);
    }

    public override void PocketItem() {
        base.PocketItem();

        ActivateLights(false);
    }

    public void ActivateLights(bool enable) {
        foreach (var light in lights) light.enabled = enableLights;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncColorServerRpc() {
        if (!choseColor) {
            red = colorRandom.NextFloat(0, 1);
            green = colorRandom.NextFloat(0, 1);
            blue = colorRandom.NextFloat(0, 1);

            alpha = colorRandom.NextFloat(.2F, 1);

            sizeIndex = itemPropertiesBySize.Length <= 0? -1 : colorRandom.NextInt(0, itemPropertiesBySize.Length);

            enableLights = colorRandom.NextInt(1, 100) <= CubeConfig.ColoredCubeConfig.coloredCubeLightChance.Value;
            isRainbow = colorRandom.NextInt(1, 100) <= CubeConfig.ColoredCubeConfig.coloredCubeRainbowChance.Value;

            choseColor = true;
        }

        SyncColorClientRpc(red, green, blue, alpha, sizeIndex, enableLights, isRainbow);
    }

    // ReSharper disable ParameterHidesMember
    [ClientRpc]
    public void SyncColorClientRpc(float red, float green, float blue, float alpha, int sizeIndex, bool enableLights, bool isRainbow) {
        this.red = red;
        this.green = green;
        this.blue = blue;
        this.alpha = alpha;
        this.sizeIndex = sizeIndex;
        this.enableLights = enableLights;
        this.isRainbow = isRainbow;

        Color.RGBToHSV(new(this.red, this.green, this.blue, this.alpha), out currentColorHSV, out var _, out var _);

        if (this.sizeIndex >= 0) {
            var size = sizes[this.sizeIndex];

            itemProperties = itemPropertiesBySize[this.sizeIndex];
            transform.localScale = new(size, size, size);
            originalScale = new(size, size, size);

            lights = lightLists[this.sizeIndex].lights;
        }

        UpdateColor(false, true);
    }
    // ReSharper restore ParameterHidesMember

    public void UpdateColor(bool useHSV, bool createNewMaterial) {
        if (useHSV) {
            var color = Color.HSVToRGB(currentColorHSV, 1F, 1F);

            red = color.r;
            green = color.g;
            blue = color.b;
        }

        var newColor = new Color(red, green, blue, alpha);

        var material = !createNewMaterial
            ? renderer.material
            : new(alpha.Approx(1F, .1F)? opaqueMaterial : transparentMaterial) {
                color = newColor,
            };

        if (!createNewMaterial) material.color = newColor;

        renderer.material = material;

        foreach (var light in lights) {
            light.color = newColor;
            light.enabled = enableLights;
        }
    }
}