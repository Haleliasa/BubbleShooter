#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using UnityEngine;

public static class Utils {
    public static bool TryGetGameObject(
        this UnityEngine.Object obj,
        [NotNullWhen(true)] out GameObject? gameObject) {
        switch (obj) {
            case GameObject gObj:
                gameObject = gObj;
                return true;
            case Component component:
                gameObject = component.gameObject;
                return true;
            default:
                gameObject = null;
                return false;
        }
    }

    public static bool CheckLayer(this Component component, LayerMask mask) {
        return component.gameObject.CheckLayer(mask);
    }

    public static bool CheckLayer(this GameObject obj, LayerMask mask) {
        return ((1 << obj.layer) & mask) != 0;
    }

    public static Vector2 Rotate(this Vector2 vector, float angleDeg) {
        float rad = angleDeg * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(
            (vector.x * cos) - (vector.y * sin),
            (vector.x * sin) + (vector.y * cos));
    }

    public static async void FireAndForget(this Task task) {
        try {
            await task;
        } catch (Exception e) {
            Debug.LogError(e);
        }
    }
}
