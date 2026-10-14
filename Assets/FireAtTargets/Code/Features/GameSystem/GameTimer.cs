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

        public async UniTask StartTimerTick(float totalTime, float tickInterval, CancellationToken token, Action<float> onTimerTick = null)
        {
            if (tickInterval <= 0)
                throw new ArgumentOutOfRangeException(nameof(tickInterval), tickInterval, "Tick interval must be positive.");

            var remainingTime = totalTime;
            onTimerTick?.Invoke(remainingTime);

            while (remainingTime > 0)
            {
                // Wait first, then report: the displayed value always matches the real time left.
                var step = Math.Min(tickInterval, remainingTime);
                await UniTask.Delay(TimeSpan.FromSeconds(step), false, PlayerLoopTiming.Update, token);

                remainingTime -= step;
                ElapsedTime += step;
                onTimerTick?.Invoke(remainingTime);
            }
        }

        public void Reset() => ElapsedTime = 0;
    }
}
