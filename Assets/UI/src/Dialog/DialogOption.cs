namespace UI.Dialog {
    public readonly struct DialogOption<T> {
        public DialogOption(string text, T value) {
            this.text = text;
            this.value = value;
        }

        public readonly string text;
        public readonly T value;
    }
}
