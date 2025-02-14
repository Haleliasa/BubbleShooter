using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BubbleShooter.Levels {
    public class AddressablesLevelLoader : ILevelLoader {
        private readonly object[] keys;
        private readonly List<LevelInfo> levels = new();
        private readonly List<TextAsset> levelAssets = new();

        public AddressablesLevelLoader(params object[] keys) {
            this.keys = keys;
        }

        public async Task<IReadOnlyList<LevelInfo>> LoadLevels() {
            this.levels.Clear();
            this.levelAssets.Clear();
            foreach (object key in this.keys) {
                IList<TextAsset> assets =
                    await Addressables.LoadAssetsAsync<TextAsset>(key, _ => { }).Task;
                this.levelAssets.AddRange(assets);
            }
            this.levels.AddRange(
                this.levelAssets.Select((a, i) => new LevelInfo(i, a.name)));
            return this.levels;
        }

        public Task<LevelData?> LoadLevel(int index) {
            if (index < 0 || index >= this.levels.Count) {
                return Task.FromResult<LevelData?>(null);
            }
            LevelData? data = TextLevel.Parse(this.levelAssets[index].text);
            return Task.FromResult(data);
        }
    }
}
