using Field;
using UnityEngine;

namespace Bubbles {
    public static class BubbleUtils {
        public static void Destroy(this Bubble bubble, FieldObjectDestroyType type) {
            if (type == FieldObjectDestroyType.Dispose) {
                Object.Destroy(bubble.gameObject);
                return;
            }
            bubble.transform.SetParent(null, worldPositionStays: true);
            switch (type) {
                case FieldObjectDestroyType.Normal:
                    bubble.Pop();
                    break;

                case FieldObjectDestroyType.Detach:
                    bubble.Fall();
                    break;
            }
        }
    }
}
