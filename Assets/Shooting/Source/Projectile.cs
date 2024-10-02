#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shooting {
    public class Projectile : MonoBehaviour {
        [SerializeField]
        private ProjectileConfig config = null!;

        [SerializeField]
        private new Rigidbody2D? rigidbody;

        private Vector2? dir;
        private float speed;
        private float gravitySpeed;
        private float gravity;

        public bool IsMoving => this.dir.HasValue;

        public void GetTrajectory(
            Vector2 dir,
            float power,
            LayerMask targetLayers,
            float timeStep,
            float maxTime,
            List<Vector2> result,
            out int length,
            out int alternativeLength) {
            dir.Normalize();
            power = Mathf.Clamp01(power);
            if (Mathf.Approximately(power, 1f)) {
                Vector2 dir1 = dir;
                Vector2 dir2 = dir;
                length = GetTrajectoryInternal(
                    dir1 * this.config.MaxSpeed,
                    this.config.MinGravity,
                    targetLayers,
                    timeStep,
                    maxTime,
                    result);
                alternativeLength = GetTrajectoryInternal(
                    dir2 * this.config.MinSpeed,
                    this.config.MaxGravity,
                    targetLayers,
                    timeStep,
                    maxTime,
                    result);
            } else {
                length = GetTrajectoryInternal(
                    dir * Mathf.Lerp(this.config.MinSpeed, this.config.MaxSpeed, power),
                    Mathf.Lerp(this.config.MaxGravity, this.config.MinGravity, power),
                    targetLayers,
                    timeStep,
                    maxTime,
                    result);
                alternativeLength = 0;
            }
        }

        public void Launch(Vector2 dir, float power) {
            this.dir = dir.normalized;
            power = Mathf.Clamp01(power);
            this.speed = Mathf.Lerp(this.config.MinSpeed, this.config.MaxSpeed, power);
            this.gravitySpeed = 0f;
            this.gravity = Mathf.Lerp(this.config.MaxGravity, this.config.MinGravity, power);
        }

        public void Stop() {
            this.dir = null;
        }

        private void FixedUpdate() {
            if (this.dir.HasValue) {
                Move(
                    this.dir.Value * this.speed,
                    ref this.gravitySpeed,
                    this.gravity,
                    Time.fixedDeltaTime);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            ReflectFromWall(collision);
        }

        private void Move(
            Vector2 velocity,
            ref float gravitySpeed,
            float gravity,
            float deltaTime) {
            Vector2 movement = GetMovement(velocity, ref gravitySpeed, gravity, deltaTime);
            if (this.rigidbody != null) {
                this.rigidbody.MovePosition(this.rigidbody.position + movement);
            } else {
                transform.position += (Vector3)movement;
            }
        }

        private int GetTrajectoryInternal(
            Vector2 velocity,
            float gravity,
            LayerMask targetLayers,
            float timeStep,
            float maxTime,
            List<Vector2> result) {
            Vector2 from =
                this.rigidbody != null
                ? this.rigidbody.position
                : (Vector2)transform.position;
            int startCount = result.Count;
            result.Add(from);
            float time = 0f;
            float gravitySpeed = 0f;
            RaycastHit2D hit;
            ContactFilter2D contactFilter = new() { useTriggers = true };
            contactFilter.SetLayerMask(targetLayers | this.config.WallLayers);
            List<RaycastHit2D> hits = new();
            do {
                time += timeStep;
                Vector2 to = from + GetMovement(velocity, ref gravitySpeed, gravity, timeStep);
                int hitCount = Physics2D.Linecast(from, to, contactFilter, hits);
                hit = hits
                    .Take(hitCount)
                    .Where(h => h.collider.gameObject != gameObject)
                    .OrderBy(h => h.distance)
                    .FirstOrDefault();
                if (hit.collider != null) {
                    result.Add(hit.point);
                    if (hit.collider.CheckLayer(this.config.WallLayers)) {
                        velocity = Vector2.Reflect(velocity, hit.normal);
                        from = hit.point + GetMovement(velocity, ref gravitySpeed, gravity, 1e-6f);
                    }
                } else {
                    from = to;
                    result.Add(from);
                }
                hits.Clear();
            }
            while (time < maxTime
                && (hit.collider == null
                    || !hit.collider.CheckLayer(targetLayers)));
            return result.Count - startCount;
        }

        private Vector2 GetMovement(
            Vector2 velocity,
            ref float gravitySpeed,
            float gravity,
            float deltaTime) {
            gravitySpeed += gravity * deltaTime;
            return (velocity + new Vector2(0f, -gravitySpeed)) * deltaTime;
        }

        private void ReflectFromWall(Collision2D collision) {
            if (this.dir.HasValue
                && collision.gameObject.CheckLayer(this.config.WallLayers)) {
                this.dir = Vector2.Reflect(this.dir.Value, collision.GetContact(0).normal);
            }
        }
    }
}
