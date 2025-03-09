using UnityEngine;

namespace BubbleShooter.Bubbles {
    [CreateAssetMenu(
        fileName = nameof(ProjectileConfig),
        menuName = nameof(ProjectileConfig))]
    public class ProjectileConfig : ScriptableObject {
        [Tooltip("units/sec")]
        [Min(0f)]
        [SerializeField]
        private float minSpeed = 10f;

        [Tooltip("units/sec")]
        [Min(0f)]
        [SerializeField]
        private float maxSpeed = 20f;

        [Tooltip("units/sec2")]
        [Min(0f)]
        [SerializeField]
        private float minGravity = 0f;

        [Tooltip("units/sec2")]
        [Min(0f)]
        [SerializeField]
        private float maxGravity = 5f;

        [SerializeField]
        private LayerMask wallLayers;

        [Tooltip("deg")]
        [Range(0f, 180f)]
        [SerializeField]
        private float maxPowerSpreadArc = 10f;

        public float MinSpeed => this.minSpeed;

        public float MaxSpeed => this.maxSpeed;

        public float MinGravity => this.minGravity;

        public float MaxGravity => this.maxGravity;

        public LayerMask WallLayers => this.wallLayers;

        public float MaxPowerSpreadArc => this.maxPowerSpreadArc;
    }
}
