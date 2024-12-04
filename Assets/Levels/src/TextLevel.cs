using System;
using System.Collections.Generic;
using UnityEngine;

namespace Levels {
    public static class TextLevel {
        public static LevelData? Parse(string text) {
            return Parse(text.Split(
                new string[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries));
        }

        public static LevelData? Parse(IEnumerable<string> lines) {
            IEnumerator<string> iter = lines.GetEnumerator();
            
            if (!iter.MoveNext()) {
                return null;
            }

            if (!int.TryParse(iter.Current, out int shotCount)) {
                return null;
            }

            List<LevelItem> levelItems = new();
            int y = 0;
            while (iter.MoveNext()) {
                string fieldLine = iter.Current;
                for (int x = 0; x < fieldLine.Length; x++) {
                    char colorIndexChar = fieldLine[x];

                    if (!char.IsDigit(colorIndexChar)) {
                        return null;
                    }

                    levelItems.Add(new LevelItem(
                        new Vector2Int(x, y),
                        colorIndexChar - '0'));
                }
                y++;
            }

            return new LevelData(levelItems, shotCount);
        }
    }
}
