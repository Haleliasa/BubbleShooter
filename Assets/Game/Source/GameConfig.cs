using System.Collections.Generic;
using UnityEngine;

namespace Game {
    [CreateAssetMenu(
        fileName = nameof(GameConfig),
        menuName = nameof(GameConfig))]
    public class GameConfig : ScriptableObject {
        [SerializeField]
        private Color[] colors;

        public IReadOnlyList<Color> Colors => this.colors;
    }
}
