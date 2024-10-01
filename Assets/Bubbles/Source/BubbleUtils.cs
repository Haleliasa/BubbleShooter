using Field;
using System.Collections;

namespace Bubbles {
    public static class BubbleUtils {
        public static IEnumerator Destroy(this Bubble bubble, FieldObjectDestroyType type) {
            switch (type) {
                case FieldObjectDestroyType.Normal:
                    yield return bubble.Pop();
                    break;

                case FieldObjectDestroyType.Detach:
                    yield return bubble.Fall();
                    break;
            }
        }
    }
}
