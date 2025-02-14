#nullable enable

using BubbleShooter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BubbleShooter.Bubbles {
    public class Projectile : MonoBehaviour {
        [SerializeField]
        private ProjectileConfig config = null!;

        [SerializeField]
        private new Rigidbody2D? rigidbody;

        [Tooltip("units")]
        [Min(0f)]
        [SerializeField]
        private float radius = 0.5f;

        [Tooltip("units")]
        [Min(0f)]
        [SerializeField]
        private float mandatoryWallBounce = 0.1f;

        private Vector2? dir;
        private float speed;
        private float gravitySpeed;
        private float gravity;
        private readonly List<RaycastHit2D> hitBuffer = new(2);

        public bool IsMoving => this.dir.HasValue;

        public Vector2 Position =>
            this.rigidbody != null
            ? this.rigidbody.position
            : (Vector2)transform.position;

        public float Power { get; private set; }

        public event Action<Projectile>? Stopped;

        public void GetTrajectory(
            Vector2 dir,
            float power,
            LayerMask targetLayers,
            float timeStep,
            float maxTime,
            List<Vector2> result,
            out int length,
            out int altLength) {
            dir.Normalize();
            power = Mathf.Clamp01(power);
            if (Mathf.Approximately(power, 1f)) {
                float spreadHalf = this.config.MaxPowerSpreadArc / 2f;
                length = GetTrajectoryInternal(
                    dir.Rotate(spreadHalf),
                    this.config.MaxSpeed,
                    this.config.MinGravity,
                    targetLayers,
                    timeStep,
                    maxTime,
                    result);
                altLength = GetTrajectoryInternal(
                    dir.Rotate(-spreadHalf),
                    this.config.MaxSpeed,
                    this.config.MinGravity,
                    targetLayers,
                    timeStep,
                    maxTime,
                    result);
            } else {
                (float speed, float gravity) = GetSpeedAndGravity(power);
                length = GetTrajectoryInternal(
                    dir,
                    speed,
                    gravity,
                    targetLayers,
                    timeStep,
                    maxTime,
                    result);
                altLength = 0;
            }
        }

        public void Launch(Vector2 dir, float power) {
            this.dir = dir.normalized;
            Power = Mathf.Clamp01(power);
            if (Mathf.Approximately(Power, 1f)) {
                float spreadHalf = this.config.MaxPowerSpreadArc / 2f;
                this.dir = this.dir.Value.Rotate(
                    UnityEngine.Random.Range(-spreadHalf, spreadHalf));
            }
            (this.speed, this.gravity) = GetSpeedAndGravity(Power);
            this.gravitySpeed = 0f;
        }

        public void Stop() {
            this.dir = null;
            Stopped?.Invoke(this);
        }

        private void FixedUpdate() {
            if (this.dir.HasValue) {
                Vector2 dir = this.dir.Value;
                Move(
                    ref dir,
                    this.speed,
                    ref this.gravitySpeed,
                    this.gravity,
                    Time.fixedDeltaTime);
                this.dir = dir;
            }
        }

        private void OnDisable() {
            Stop();
        }

        private void Move(
            ref Vector2 dir,
            float speed,
            ref float gravitySpeed,
            float gravity,
            float deltaTime) {
            Vector2 movement = GetMovement(
                Position,
                ref dir,
                speed,
                ref gravitySpeed,
                gravity,
                deltaTime,
                out _);
            if (this.rigidbody != null) {
                this.rigidbody.MovePosition(this.rigidbody.position + movement);
            } else {
                transform.position += (Vector3)movement;
            }
        }

        private int GetTrajectoryInternal(
            Vector2 dir,
            float speed,
            float gravity,
            LayerMask targetLayers,
            float timeStep,
            float maxTime,
            List<Vector2> result) {
            Vector2 pos = Position;
            int startCount = result.Count;
            result.Add(pos);
            float time = 0f;
            float gravitySpeed = 0f;
            bool reachedTarget;
            do {
                pos += GetMovement(
                    pos,
                    ref dir,
                    speed,
                    ref gravitySpeed,
                    gravity,
                    timeStep,
                    out reachedTarget,
                    targetLayers: targetLayers);
                result.Add(pos);
                time += timeStep;
            }
            while (time < maxTime && !reachedTarget);
            return result.Count - startCount;
        }

        private Vector2 GetMovement(
            Vector2 from,
            ref Vector2 dir,
            float speed,
            ref float gravitySpeed,
            float gravity,
            float deltaTime,
            out bool reachedTarget,
            LayerMask? targetLayers = null) {
            reachedTarget = false;

            float deltaTimeHalf = deltaTime / 2f;
            gravitySpeed += gravity * deltaTimeHalf;
            Vector2 movement = ((dir * speed) + new Vector2(0f, -gravitySpeed)) * deltaTime;
            gravitySpeed += gravity * deltaTimeHalf;
            float dist = movement.magnitude;

            if (Mathf.Approximately(dist, 0f)) {
                return Vector2.zero;
            }

            Vector2 finalDir = movement / dist;

            int layers = this.config.WallLayers;
            if (targetLayers.HasValue) {
                layers |= targetLayers.Value;
            }
            ContactFilter2D contactFilter = new() { useTriggers = true };
            contactFilter.SetLayerMask(layers);
            int hits = Physics2D.CircleCast(
                from,
                this.radius,
                finalDir,
                contactFilter,
                this.hitBuffer,
                distance: dist);
            RaycastHit2D hit = this.hitBuffer
                .Take(hits)
                .Where(h => h.collider.gameObject != gameObject)
                .OrderBy(h => h.distance)
                .FirstOrDefault();
            this.hitBuffer.Clear();

            if (hit.collider == null) {
                return movement;
            }

            movement = finalDir * hit.distance;
            if (hit.collider.CheckLayer(this.config.WallLayers)) {
                dir = Vector2.Reflect(dir, hit.normal);
                movement += (Vector2.Reflect(finalDir, hit.normal) * (dist - hit.distance))
                    + (hit.normal * this.mandatoryWallBounce);
            } else {
                reachedTarget = true;
            }
            return movement;
        }

        private (float, float) GetSpeedAndGravity(float power) {
            return (Mathf.Lerp(this.config.MinSpeed, this.config.MaxSpeed, power),
                Mathf.Lerp(this.config.MaxGravity, this.config.MinGravity, power));
        }
    }
}
