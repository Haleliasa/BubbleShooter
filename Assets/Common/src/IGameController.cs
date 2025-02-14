using System;

namespace BubbleShooter {
    public interface IGameController {
        event Action<int> ScoreChanged;

        void StartGame();
    }
}
