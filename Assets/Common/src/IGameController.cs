using System;

public interface IGameController {
    event Action<int> ScoreChanged;
}
