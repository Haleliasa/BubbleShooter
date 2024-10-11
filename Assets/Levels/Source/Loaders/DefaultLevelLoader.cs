#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Levels {
    public class DefaultLevelLoader : ILevelLoader {
        public const string FilePattern = "*.lvl";

        private readonly string[] directories;
        private readonly List<LevelInfo> levels = new();
        private readonly List<FileInfo> levelFiles = new();
        
        public DefaultLevelLoader(params string[] directories) {
            this.directories = directories;
        }

        public Task<IReadOnlyList<LevelInfo>> LoadLevels() {
            this.levels.Clear();
            this.levelFiles.Clear();
            foreach (string dir in this.directories.Where(Directory.Exists)) {
                foreach (string fileName in Directory.EnumerateFiles(dir, FilePattern)) {
                    FileInfo file = new(fileName);
                    this.levels.Add(new LevelInfo(this.levels.Count, file.Name));
                    this.levelFiles.Add(file);
                }
            }
            return Task.FromResult<IReadOnlyList<LevelInfo>>(this.levels);
        }

        public async Task<LevelData?> LoadLevel(int index) {
            if (index < 0 || index >= this.levels.Count) {
                return null;
            }

            FileInfo file = this.levelFiles[index];
            
            if (!file.Exists) {
                return null;
            }

            using StreamReader reader = file.OpenText();
            string? shotCountLine = await reader.ReadLineAsync();

            if (!int.TryParse(shotCountLine, out int shotCount)) {
                return null;
            }

            List<LevelItem> levelItems = new();
            int y = 0;
            while (!reader.EndOfStream) {
                string fieldLine = await reader.ReadLineAsync();
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
