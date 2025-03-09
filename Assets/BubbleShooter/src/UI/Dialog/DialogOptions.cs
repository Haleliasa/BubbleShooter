using System.Collections.Generic;

namespace BubbleShooter.UI.Dialog {
    public static class DialogOptions {
        public static IEnumerable<DialogOption<bool>> YesNo() {
            yield return new DialogOption<bool>("Yes", true);
            yield return new DialogOption<bool>("No", false);
        }

        public static IEnumerable<string> Ok() {
            yield return "OK";
        }
    }
}
