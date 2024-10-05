using Field;

namespace Bubbles {
    public static class BubbleUtils {
        public static void Destroy(this Bubble bubble, FieldObjectDestroyType type) {
            switch (type) {
                case FieldObjectDestroyType.Normal:
                    bubble.Pop();
                    break;

                case FieldObjectDestroyType.Isolated:
                    bubble.Fall();
                    break;

                case FieldObjectDestroyType.Dispose:
                    bubble.Destroy();
                    return;
            }
        }
    }
}
