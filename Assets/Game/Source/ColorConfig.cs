using System.Collections.Generic;
using UnityEngine;

namespace Game {
    [CreateAssetMenu(
        fileName = nameof(ColorConfig),
        menuName = nameof(ColorConfig))]
    public class ColorConfig : ScriptableObject {
        [SerializeField]
        private Color[] colors;

        public IReadOnlyList<Color> Colors => this.colors;
    }
}
