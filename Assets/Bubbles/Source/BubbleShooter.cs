#nullable enable

using Shooting;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Bubbles {
    public class BubbleShooter : MonoBehaviour {
        [SerializeField]
        private ProjectileBubble projectilePrefab = null!;

        [SerializeField]
        private Slingshot slingshot = null!;

        [SerializeField]
        private SpriteRenderer projectilePreview = null!;

        [SerializeField]
        private TMP_Text shotCountText = null!;

        [Header("Trajectory")]
        [SerializeField]
        private LineRenderer trajectory = null!;

        [SerializeField]
        private LineRenderer altTrajectory = null!;

        [SerializeField]
        private LayerMask targetLayers;

        [SerializeField]
        private float trajectoryTimeStep = 0.02f;

        [SerializeField]
        private float maxTrajectoryTime = 5f;

        [SerializeField]
        private Color trajectoryColor;

        [SerializeField]
        private Color altTrajectoryColor;

        private readonly List<Color> colors = new();
        private int shotCount;
        private bool subbed = false;
        private ProjectileBubble? projectile;
        private Color nextColor;
        private readonly List<Vector2> trajectoryBuffer = new();

        public int ShotCount => this.shotCount;

        public event Action<BubbleShooter>? BubbleDestroying;

        public void Init(IEnumerable<Color> colors, int shotCount) {
            this.colors.Clear();
            this.colors.AddRange(colors);
            this.nextColor = GetNextColor();
            this.shotCount = Math.Max(shotCount, 1);
            this.slingshot.enabled = false;
            if (!this.subbed) {
                Subscribe();
                this.subbed = true;
            }
        }

        public void Prepare() {
            if (this.colors.Count == 0
                || this.shotCount <= 0) {
                return;
            }

            this.projectile = Instantiate(this.projectilePrefab);
            this.projectile.transform.SetParent(transform);
            this.projectile.transform.localPosition = Vector3.zero;
            this.projectile.Init(this.nextColor);
            this.nextColor = GetNextColor();

            this.slingshot.enabled = true;

            this.shotCount--;
            bool showPreview = this.shotCount > 0;
            this.projectilePreview.enabled = showPreview;
            this.shotCountText.enabled = showPreview;
            if (showPreview) {
                this.projectilePreview.color = this.nextColor;
                this.shotCountText.text = this.shotCount.ToString();
            }
        }

        private void OnEnable() {
            if (this.subbed) {
                Subscribe();
            }
        }

        private void Update() {
            UpdateProjectileTrajectory();
        }

        private void OnDisable() {
            if (this.subbed) {
                Unsubscribe();
            }
        }

        private void Shoot(Slingshot.ShotData data) {
            if (this.projectile == null) {
                return;
            }
            SubscribeProjectile(this.projectile);
            this.projectile.Projectile.Launch(data.direction, data.distance);
            this.slingshot.enabled = false;
        }

        private void OnProjectileStopped(Projectile projectile) {
            if (this.projectile == null) {
                return;
            }
            UnsubscribeProjectile(this.projectile);
            this.projectile = null;
        }

        private void OnBubbleDestroying(Bubble bubble) {
            if (this.projectile == null) {
                return;
            }
            UnsubscribeProjectile(this.projectile);
            this.projectile = null;
            BubbleDestroying?.Invoke(this);
        }

        private void UpdateProjectileTrajectory() {
            if (this.projectile == null
                || this.projectile.Projectile.IsMoving
                || Mathf.Approximately(this.slingshot.Distance, 0f)) {
                this.trajectory.positionCount = 0;
                this.altTrajectory.positionCount = 0;
                return;
            }

            this.projectile.Projectile.GetTrajectory(
                this.slingshot.Direction,
                this.slingshot.Distance,
                this.targetLayers,
                this.trajectoryTimeStep,
                this.maxTrajectoryTime,
                this.trajectoryBuffer,
                out int len,
                out int altLen);

            this.trajectory.positionCount = len;
            this.trajectory.SetPositions(
                this.trajectoryBuffer.Take(len).Select(p => (Vector3)p).ToArray());

            if (altLen > 0) {
                this.trajectory.startColor =
                    this.trajectory.endColor =
                    this.altTrajectory.startColor =
                    this.altTrajectory.endColor =
                    this.altTrajectoryColor;
                this.altTrajectory.positionCount = altLen;
                this.altTrajectory.SetPositions(
                    this.trajectoryBuffer.Skip(len).Take(altLen).Select(p => (Vector3)p).ToArray());
            } else {
                this.trajectory.startColor =
                    this.trajectory.endColor =
                    this.trajectoryColor;
                this.altTrajectory.positionCount = 0;
            }

            this.trajectoryBuffer.Clear();
        }

        private void Subscribe() {
            this.slingshot.Shot += Shoot;
        }

        private void Unsubscribe() {
            this.slingshot.Shot -= Shoot;
        }

        private void SubscribeProjectile(ProjectileBubble projectile) {
            projectile.Projectile.Stopped += OnProjectileStopped;
            projectile.Bubble.Destroying += OnBubbleDestroying;
        }

        private void UnsubscribeProjectile(ProjectileBubble projectile) {
            projectile.Projectile.Stopped -= OnProjectileStopped;
            projectile.Bubble.Destroying -= OnBubbleDestroying;
        }

        private Color GetNextColor() {
            if (this.colors.Count == 0) {
                return Color.white;
            }
            return this.colors[UnityEngine.Random.Range(0, this.colors.Count)];
        }
    }
}
