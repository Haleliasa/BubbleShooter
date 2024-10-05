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
        private LayerMask targetLayers;

        [SerializeField]
        private LineRenderer trajectory = null!;

        [SerializeField]
        private LineRenderer altTrajectory = null!;

        [SerializeField]
        private Color trajectoryColor;

        [SerializeField]
        private Color altTrajectoryColor;

        [SerializeField]
        private SpriteRenderer projectilePreview = null!;

        [SerializeField]
        private TMP_Text shotCountText = null!;

        private readonly List<Color> colors = new();
        private int shotCount;
        private bool inited = false;
        private Projectile? preparedProjectile;
        private Color nextColor;
        private readonly List<Vector2> trajectoryBuffer = new();

        public void Init(IEnumerable<Color> colors, int shotCount) {
            this.colors.Clear();
            this.colors.AddRange(colors);
            if (this.colors.Count > 0) {
                this.nextColor = this.colors[0];
            }
            this.shotCount = Math.Max(shotCount, 1);
            if (!this.inited) {
                this.slingshot.Shoot += Shoot;
                this.inited = true;
            }
            PrepareProjectile();
        }

        private void Update() {
            DrawProjectileTrajectory();
        }

        private void PrepareProjectile() {
            if (this.colors.Count == 0
                || this.shotCount <= 0) {
                return;
            }

            ProjectileBubble projectile = Instantiate(
                this.projectilePrefab,
                transform.position,
                Quaternion.identity);
            projectile.Init(this.nextColor);
            this.preparedProjectile = projectile.Projectile;
            this.nextColor = this.colors[UnityEngine.Random.Range(0, this.colors.Count)];

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

        private void Shoot(Slingshot.EventData data) {
            if (this.preparedProjectile == null) {
                return;
            }
            this.preparedProjectile.Launch(data.direction, data.distance);
            this.preparedProjectile.Stopped += OnProjectileStopped;
            this.preparedProjectile = null;
            this.slingshot.enabled = false;
        }

        private void OnProjectileStopped(Projectile projectile) {
            projectile.Stopped -= OnProjectileStopped;
            PrepareProjectile();
        }

        private void DrawProjectileTrajectory() {
            if (this.preparedProjectile == null
                || Mathf.Approximately(this.slingshot.Distance, 0f)) {
                this.trajectory.positionCount = 0;
                this.altTrajectory.positionCount = 0;
                return;
            }

            this.preparedProjectile.GetTrajectory(
                this.slingshot.Direction,
                this.slingshot.Distance,
                this.targetLayers,
                0.02f,
                5f,
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
    }
}
