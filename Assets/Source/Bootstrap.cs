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
    private ProjectileBubble projectileBubblePrefab;

    private void Start() {
        this.field.Init(GenerateBubbles());
        ProjectileBubble projBubble = Instantiate(
            this.projectileBubblePrefab,
            new Vector2(0f, -8f),
            Quaternion.identity);
        projBubble.Init(Color.green);
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
