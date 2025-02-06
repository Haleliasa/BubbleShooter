using System.Threading.Tasks;

namespace UI.Dialog {
    public readonly struct DialogResult<T> {
        public readonly Dialog dialog;
        public readonly Task<T> result;

        public DialogResult(Dialog dialog, Task<T> result) {
            this.dialog = dialog;
            this.result = result;
        }
    }
}
