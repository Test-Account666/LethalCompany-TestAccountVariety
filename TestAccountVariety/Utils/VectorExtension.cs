using UnityEngine;

namespace TestAccountVariety.Utils;

public static class VectorExtension {
    public static int ConvertToInt(this Vector3 position) => (int) (position.x + position.y + position.z);
}