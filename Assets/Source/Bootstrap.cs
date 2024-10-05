using Bubbles;
using Field;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour {
    [SerializeField]
    private Field.Field field;

    [SerializeField]
    private StaticBubble staticBubblePrefab;

    [SerializeField]
    private BubbleShooter shooter;

    private readonly Color[] colors = new Color[] { Color.red, Color.green, Color.blue };

    private void Start() {
        this.field.Init(GenerateBubbles());
        this.shooter.Init(this.colors, 10);
    }

    private IEnumerable<FieldObjectInfo> GenerateBubbles() {
        for (int y = 0; y < 5; y++) {
            for (int x = 0; x < 10; x++) {
                StaticBubble bubble = Instantiate(this.staticBubblePrefab);
                Color color = this.colors[Random.Range(0, this.colors.Length)];
                bubble.Init(color);
                yield return new FieldObjectInfo(bubble, new Vector2Int(x, y), color);
            }
        }
    }
}
