using System;
using Cysharp.Threading.Tasks;

namespace FreedLOW.FireAtTargets.Code.Features.GameSystem
{
    public interface IFireAtTargetsSystem
    {
        bool IsGameActive { get; }
        event Action OnGameStarted;
        event Action<GameResult> OnGameCompleted;
        event Action<float> OnGameTimerTick;
        
        UniTask StartGame();
        void StopGame();
        void AddPoint(int point);
    }
}