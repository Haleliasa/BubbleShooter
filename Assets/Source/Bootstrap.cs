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
    private LineRenderer trajectoryRenderer;

    private void Start() {
        //this.field.Init(GenerateBubbles());
        ProjectileBubble projBubble = Instantiate(
            this.projectileBubblePrefab,
            new Vector2(0f, -8f),
            Quaternion.identity);
        projBubble.Init(Color.green);
        Projectile proj = projBubble.GetComponent<Projectile>();
        Vector2 dir = new(-0.5f, 0.4f);
        float power = 0.4f;
        List<Vector2> traj = new();
        proj.GetTrajectory(dir, power, this.targetLayers, 0.02f, 100f, traj, out _, out _);
        this.trajectoryRenderer.positionCount = traj.Count;
        this.trajectoryRenderer.SetPositions(traj.Select(p => (Vector3)p).ToArray());
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
