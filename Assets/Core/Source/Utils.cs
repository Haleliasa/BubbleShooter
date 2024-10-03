using UnityEngine;

public static class Utils {
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
}
