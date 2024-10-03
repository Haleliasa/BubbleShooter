using Bubbles;
using Field;
using Shooting;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bootstrap : MonoBehaviour {
    [SerializeField]
    private Field.Field field;

    [SerializeField]
    private Slingshot slingshot;

    [SerializeField]
    private StaticBubble staticBubblePrefab;

    [SerializeField]
    private ProjectileBubble projectileBubblePrefab;

    [SerializeField]
    private Transform projectilePosition;

    [SerializeField]
    private LayerMask targetLayers;

    [SerializeField]
    private LineRenderer trajectory;

    [SerializeField]
    private LineRenderer altTrajectory;

    private Projectile projectile;
    private readonly List<Vector2> trajectoryBuffer = new();

    private void Start() {
        this.field.Init(GenerateBubbles());

        ProjectileBubble projBubble = Instantiate(
            this.projectileBubblePrefab,
            this.projectilePosition.position,
            Quaternion.identity);
        projBubble.Init(Color.green);
        this.projectile = projBubble.GetComponent<Projectile>();

        this.slingshot.Shoot += OnSlingshotShoot;
    }

    private void Update() {
        DrawProjectileTrajectory();
    }

    private void OnDestroy() {
        this.slingshot.Shoot -= OnSlingshotShoot;
    }

    private void DrawProjectileTrajectory() {
        if (Mathf.Approximately(this.slingshot.Distance, 0f)) {
            this.trajectory.positionCount = 0;
            this.altTrajectory.positionCount = 0;
            return;
        }

        this.projectile.GetTrajectory(
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
                Color.red;
            this.altTrajectory.positionCount = altLen;
            this.altTrajectory.SetPositions(
                this.trajectoryBuffer.Skip(len).Take(altLen).Select(p => (Vector3)p).ToArray());
        } else {
            this.trajectory.startColor =
                this.trajectory.endColor =
                Color.black;
            this.altTrajectory.positionCount = 0;
        }

        this.trajectoryBuffer.Clear();
    }

    private void OnSlingshotShoot(Slingshot.EventData data) {
        this.projectile.Launch(data.direction, data.distance);
    }

    private IEnumerable<FieldObjectInfo> GenerateBubbles() {
        for (int y = 0; y < 5; y++) {
            for (int x = 0; x < 10; x++) {
                StaticBubble bubble = Instantiate(this.staticBubblePrefab);
                bubble.Init(Color.red);
                yield return new FieldObjectInfo(bubble, Color.red, new Vector2Int(x, y));
            }
        }
    }
}
