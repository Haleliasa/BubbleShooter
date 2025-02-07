#nullable enable

using System;
using UnityEngine;

namespace UI {
    public class UiCommand : MonoBehaviour {
        public event Action<UiCommand>? Executed;

        public void Execute() {
            this.Executed?.Invoke(this);
        }
    }
}
