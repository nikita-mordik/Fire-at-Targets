using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Event;
using FreedLOW.FireAtTargets.Code.StaticData;

namespace FreedLOW.FireAtTargets.Code.Features.GameSystem
{
    public class FireAtTargetsSystem : IFireAtTargetsSystem, IDisposable
    {
        public bool IsGameActive => _isGameActive;
        public int CurrentPoints => _gameResult.PointCount;

        public event Action OnGameStarted;
        public event Action<GameResult> OnGameCompleted;
        public event Action<float> OnGameTimerTick;
        public event Action<int> OnPointsChanged;

        private readonly ShootingLevelStaticData _levelConfig;
        private readonly IGameTimer _gameTimer;
        private readonly IWeaponEventHandlerService _weaponEventHandlerService;

        private CancellationTokenSource _cts;
        private GameResult _gameResult;
        private bool _isGameActive;

        public FireAtTargetsSystem(ShootingLevelStaticData levelConfig, IGameTimer gameTimer,
            IWeaponEventHandlerService weaponEventHandlerService)
        {
            _levelConfig = levelConfig;
            _gameTimer = gameTimer;
            _weaponEventHandlerService = weaponEventHandlerService;
        }

        public async UniTask StartGame()
        {
            if (!_levelConfig)
                throw new ArgumentNullException(nameof(_levelConfig));

            if (_isGameActive)
                return;

            // The round owns its token source; Dispose()/StopGame() only cancel it.
            using var cts = new CancellationTokenSource();
            _cts = cts;
            InitializeGame();

            try
            {
                _isGameActive = true;
                _weaponEventHandlerService.InvokeOnSetAmmo();
                OnGameStarted?.Invoke();
                OnPointsChanged?.Invoke(_gameResult.PointCount);
                await RunGameRound(cts.Token);
                CompleteGame(GameCompletionReason.TimeUp);
            }
            catch (OperationCanceledException)
            {
                CompleteGame(GameCompletionReason.Cancelled);
                throw;
            }
            finally
            {
                if (_cts == cts)
                    _cts = null;
            }
        }

        public void StopGame()
        {
            if (!_isGameActive)
                return;

            _cts?.Cancel();
        }

        public void AddPoint(int point)
        {
            if (!_isGameActive || point <= 0)
                return;

            _gameResult.PointCount += point;
            OnPointsChanged?.Invoke(_gameResult.PointCount);
        }

        public void Dispose()
        {
            StopGame();
        }

        private void InitializeGame()
        {
            _gameResult = new GameResult();
            _gameTimer.Reset();
        }

        private async UniTask RunGameRound(CancellationToken token)
        {
            await _gameTimer.StartTimerTick(_levelConfig.RoundTime, _levelConfig.CountdownTimeIncrement, token, OnGameTimerTick);
        }

        private void CompleteGame(GameCompletionReason reason)
        {
            _isGameActive = false;
            _weaponEventHandlerService.InvokeOnClearAmmo();

            _gameResult.CompletionReason = reason;
            _gameResult.GameSessionTime = _gameTimer.ElapsedTime;
            OnGameCompleted?.Invoke(_gameResult);
        }
    }
}
