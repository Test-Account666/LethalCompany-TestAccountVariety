using System.Linq;
using UnityEngine;
using static UnityEngine.Object;

namespace TestAccountVariety.Utils;

public static class GrabbableObjectExtension {
    public static void DestroyItem(this GrabbableObject item) {
        item.grabbable = false;
        item.grabbableToEnemies = false;
        item.deactivated = true;

        if (item.playerHeldBy) item.playerHeldBy.activatingItem = false;
        if (item.radarIcon) Destroy(item.radarIcon.gameObject);

        item.DestroyAll<MeshRenderer>();
        item.DestroyAll<Collider>();
    }

    private static void DestroyAll<T>(this Component item) where T : Object => item.GetComponentsInChildren<T>().ToList().ForEach(Destroy);
}