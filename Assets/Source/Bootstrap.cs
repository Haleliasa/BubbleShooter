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
    private StaticBubble staticBubblePrefab;

    [SerializeField]
    private ProjectileBubble projectileBubblePrefab;

    [SerializeField]
    private LayerMask targetLayers;

    [SerializeField]
    private LineRenderer trajectory;

    [SerializeField]
    private LineRenderer altTrajectory;

    private void Start() {
        this.field.Init(GenerateBubbles());
        Invoke(nameof(LaunchProjectile), 0.1f);
    }

    private void LaunchProjectile() {
        ProjectileBubble projBubble = Instantiate(
            this.projectileBubblePrefab,
            new Vector2(0f, -8f),
            Quaternion.identity);
        projBubble.Init(Color.green);
        Projectile proj = projBubble.GetComponent<Projectile>();
        Vector2 dir = new(0.8f, 0.45f);
        float power = 0.5f;
        List<Vector2> traj = new();
        proj.GetTrajectory(
            dir,
            power,
            this.targetLayers,
            0.02f,
            100f,
            traj,
            out int len,
            out int altLen);
        this.trajectory.positionCount = len;
        this.trajectory.SetPositions(traj.Take(len).Select(p => (Vector3)p).ToArray());
        this.altTrajectory.positionCount = altLen;
        this.altTrajectory.SetPositions(traj.Skip(len).Take(altLen).Select(p => (Vector3)p).ToArray());
        proj.Launch(dir, power);
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
