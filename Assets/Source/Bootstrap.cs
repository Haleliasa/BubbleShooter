using Bubbles;
using Field;
using Shooting;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour {
    [SerializeField]
    private Field.Field field;

    [SerializeField]
    private StaticBubble staticBubblePrefab;

    [SerializeField]
    private BubbleShooter shooter;

    private void Start() {
        this.field.Init(GenerateBubbles());
        this.shooter.Init(new List<Color> { Color.red, Color.green, Color.blue }, 10);
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
