using Bubbles;
using Field;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour {
    [SerializeField]
    private Field.Field field;

    [SerializeField]
    private BubbleStaticFieldObject bubblePrefab;

    private void Start() {
        this.field.Init(GenerateBubbles());
    }

    private IEnumerable<FieldObjectInfo> GenerateBubbles() {
        for (int y = 0; y < 10; y++) {
            for (int x = 0; x < 10; x++) {
                yield return new FieldObjectInfo(
                    Instantiate(this.bubblePrefab),
                    Color.red,
                    new Vector2Int(x, y));
            }
        }
    }
}
