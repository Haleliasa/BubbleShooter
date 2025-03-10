﻿using BubbleShooter.Field;

namespace BubbleShooter.Bubbles {
    public static class BubbleUtils {
        public static void Destroy(this Bubble bubble, FieldObjectDestroyType type) {
            switch (type) {
                case FieldObjectDestroyType.Match:
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
