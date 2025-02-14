using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter {
    [CreateAssetMenu(
        fileName = nameof(GameConfig),
        menuName = nameof(GameConfig)
    )]
    public class GameConfig : ScriptableObject {
        [SerializeField]
        private Color[] colors;

        [Min(1)]
        [SerializeField]
        private int matchPoints = 1;

        [Min(1)]
        [SerializeField]
        private int isolatedPoints = 2;

        [Tooltip("sec")]
        [Min(0f)]
        [SerializeField]
        private float winDialogDelay;

        [Tooltip("sec")]
        [Min(0f)]
        [SerializeField]
        private float loseDialogDelay;

        public IReadOnlyList<Color> Colors => this.colors;

        public int MatchPoints => this.matchPoints;

        public int IsolatedPoints => this.isolatedPoints;

        public float WinDialogDelay => this.winDialogDelay;

        public float LoseDialogDelay => this.loseDialogDelay;
    }
}
