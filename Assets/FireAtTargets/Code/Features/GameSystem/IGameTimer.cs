using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace FreedLOW.FireAtTargets.Code.Features.GameSystem
{
    public interface IGameTimer
    {
        float ElapsedTime { get; }
        UniTask WaitTick(CancellationToken token);
        UniTask StartTimerTick(float totalTime, float delay, CancellationToken token, Action<float> onTimerTick = null);
        void Reset();
    }
}