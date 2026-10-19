using System;
using Cysharp.Threading.Tasks;

namespace FreedLOW.FireAtTargets.Code.Features.GameSystem
{
    public interface IFireAtTargetsSystem
    {
        bool IsGameActive { get; }
        int CurrentPoints { get; }
        event Action OnGameStarted;
        event Action<GameResult> OnGameCompleted;
        event Action<float> OnGameTimerTick;
        event Action<int> OnPointsChanged;

        UniTask StartGame();
        void StopGame();
        void AddPoint(int point);
    }
}
