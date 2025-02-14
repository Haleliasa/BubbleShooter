#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BubbleShooter.Levels {
    public class FileLevelLoader : ILevelLoader {
        public const string FilePattern = "*.txt";

        private readonly string[] directories;
        private readonly List<LevelInfo> levels = new();
        private readonly List<FileInfo> levelFiles = new();
        
        public FileLevelLoader(params string[] directories) {
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
            string text = await reader.ReadToEndAsync();
            LevelData? data = TextLevel.Parse(text);
            return data;
        }
    }
}
