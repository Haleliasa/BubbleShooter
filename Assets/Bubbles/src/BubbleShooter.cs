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
        private Slingshot slingshot = null!;

        [SerializeField]
        private SpriteRenderer? projectilePreview;

        [SerializeField]
        private TMP_Text? shotCountText;

        [SerializeField]
        private ProjectileBubble? projectilePrefab;

        [Header("Trajectory")]
        [SerializeField]
        private LineRenderer? trajectory;

        [SerializeField]
        private LineRenderer? altTrajectory;

        [SerializeField]
        private LayerMask targetLayers;

        [Tooltip("sec")]
        [Min(0.001f)]
        [SerializeField]
        private float trajectoryTimeStep = 0.02f;

        [Tooltip("sec")]
        [Min(0.001f)]
        [SerializeField]
        private float maxTrajectoryTime = 5f;

        [SerializeField]
        private Color trajectoryColor;

        [SerializeField]
        private Color altTrajectoryColor;

        private readonly List<Color> colors = new();
        private int shotCount;
        private bool subbed = false;
        private IObjectPool<ProjectileBubble>? projectilePool;
        private ProjectileBubble? projectile;
        private Color nextColor;
        private readonly List<Vector2> trajectoryBuffer = new();

        public int ShotCount => this.shotCount;

        public event Action<BubbleShooter>? BubbleDestroying;

        public void Init(
            IEnumerable<Color> colors,
            int shotCount,
            IObjectPool<ProjectileBubble>? projectilePool = null) {
            this.colors.Clear();
            this.colors.AddRange(colors);
            this.nextColor = GetNextColor();
            this.shotCount = Math.Max(shotCount, 1);
            this.slingshot.enabled = false;
            if (!this.subbed) {
                Subscribe();
                this.subbed = true;
            }
            this.projectilePool = projectilePool;
        }

        public void Prepare() {
            if (this.colors.Count == 0
                || this.shotCount <= 0) {
                return;
            }

            PooledOrInst<ProjectileBubble> createdProj =
                PooledOrInst<ProjectileBubble>.Create(this.projectilePool, this.projectilePrefab);
            this.projectile = createdProj.Object;
            this.projectile.transform.SetParent(transform);
            this.projectile.transform.localPosition = Vector3.zero;
            this.projectile.Init(this.nextColor, pooled: createdProj.Pooled);
            this.nextColor = GetNextColor();

            this.slingshot.enabled = true;

            this.shotCount--;
            bool showPreview = this.shotCount > 0;

            if (this.projectilePreview != null) {
                this.projectilePreview.enabled = showPreview;
                if (showPreview) {
                    this.projectilePreview.color = this.nextColor;
                }
            }

            if (this.shotCountText != null) {
                this.shotCountText.enabled = showPreview;
                if (showPreview) {
                    this.shotCountText.text = this.shotCount.ToString();
                }
            }
        }

        private void OnEnable() {
            if (this.subbed) {
                Subscribe();
            }
            if (this.projectile != null
                && this.projectile.Projectile.IsMoving) {
                SubscribeProjectile(this.projectile);
            }
        }

        private void Update() {
            UpdateProjectileTrajectory();
        }

        private void OnDisable() {
            if (this.subbed) {
                Unsubscribe();
            }
            if (this.projectile != null
                && this.projectile.Projectile.IsMoving) {
                UnsubscribeProjectile(this.projectile);
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
            if (this.trajectory == null
                || this.altTrajectory == null) {
                return;
            }

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
