using UnityEngine;

public static class Utils {
    public static bool CheckLayer(this Component component, LayerMask mask) {
        return component.gameObject.CheckLayer(mask);
    }

    public static bool CheckLayer(this GameObject obj, LayerMask mask) {
        return ((1 << obj.layer) & mask) != 0;
    }
}
