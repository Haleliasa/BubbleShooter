using System.Collections.Generic;
using System.Threading.Tasks;

namespace BubbleShooter.Levels {
    public interface ILevelLoader {
        Task<IReadOnlyList<LevelInfo>> LoadLevels();

        Task<LevelData?> LoadLevel(int index);
    }
}
