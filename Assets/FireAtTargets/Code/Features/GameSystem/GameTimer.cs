using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace FreedLOW.FireAtTargets.Code.Features.GameSystem
{
    public class GameTimer : IGameTimer
    {
        private const float UpdateInterval = 1f;

        public float ElapsedTime { get; private set; }

        public async UniTask WaitTick(CancellationToken token)
        {
            ElapsedTime += UpdateInterval;
            await UniTask.Delay(TimeSpan.FromSeconds(UpdateInterval), false, PlayerLoopTiming.Update, token);
        }

        public async UniTask StartTimerTick(float totalTime, float delay, CancellationToken token, Action<float> onTimerTick = null)
        {
            var remainingTime = totalTime;
            onTimerTick?.Invoke(remainingTime);
            
            while (remainingTime > 0)
            {
                token.ThrowIfCancellationRequested();

                remainingTime--;
                ElapsedTime += delay;
                onTimerTick?.Invoke(remainingTime);
                await UniTask.Delay(TimeSpan.FromSeconds(delay), false, PlayerLoopTiming.Update, token);
            }
        }

        public void Reset() => ElapsedTime = 0;
    }
}