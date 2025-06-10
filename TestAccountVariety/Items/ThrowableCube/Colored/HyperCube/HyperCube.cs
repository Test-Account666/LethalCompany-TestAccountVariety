using System;
using TestAccountVariety.Utils;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TestAccountVariety.Items.ThrowableCube.Colored.HyperCube;

public class HyperCube : ThrowableCube {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public MeshRenderer[] starRenderers;
    public MeshRenderer renderer;

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public Random colorRandom;

    public bool choseColor;

    public float red;
    public float green;
    public float blue;

    public float currentColorHSV;
    public bool reverse;

    public const float HSV_INCREMENT = 0.1F;


    public override void Start() {
        base.Start();

        var seed = (uint) (StartOfRound.Instance.randomMapSeed + transform.position.ConvertToInt() + (DateTime.Now.Ticks & 0x0000FFFF));

        colorRandom = new(seed);

        SyncColorServerRpc();
    }

    public override void Update() {
        base.Update();

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

    [ServerRpc(RequireOwnership = false)]
    public void SyncColorServerRpc() {
        if (!choseColor) {
            red = colorRandom.NextFloat(0, 1);
            green = colorRandom.NextFloat(0, 1);
            blue = colorRandom.NextFloat(0, 1);

            choseColor = true;
        }

        SyncColorClientRpc(red, green, blue);
    }

    // ReSharper disable ParameterHidesMember
    [ClientRpc]
    public void SyncColorClientRpc(float red, float green, float blue) {
        this.red = red;
        this.green = green;
        this.blue = blue;

        Color.RGBToHSV(new(this.red, this.green, this.blue, 1F), out currentColorHSV, out var _, out var _);

        UpdateColor(false, true);
    }
    // ReSharper restore ParameterHidesMember

    public void UpdateColor(bool useHSV, bool createNewMaterial) {
        if (useHSV) {
            var color = Color.HSVToRGB(currentColorHSV, 1F, 1F);

            UpdateColor(0, color, createNewMaterial);
            return;
        }

        var newColor = new Color(red, green, blue, 1F);

        for (var index = 0; index < 4; index++) UpdateColor(index, newColor, createNewMaterial);
    }

    public void UpdateColor(int index, Color color, bool createNewMaterial) {
        var meshRenderers = index switch {
            0 => (MeshRenderer[]) [
                ..starRenderers,
            ],
            var _ => (MeshRenderer[]) [
                renderer,
            ],
        };

        var materialIndex = 0;

        if (index >= 1) materialIndex = index - 1;

        foreach (var meshRenderer in meshRenderers) {
            var materials = meshRenderer.sharedMaterials;

            var material = createNewMaterial? new(materials[materialIndex]) : materials[materialIndex];

            color.a = material.color.a;

            material.color = color;
            materials[materialIndex] = material;

            meshRenderer.sharedMaterials = materials;
            meshRenderer.sharedMaterial = materials[0];
        }
    }
}