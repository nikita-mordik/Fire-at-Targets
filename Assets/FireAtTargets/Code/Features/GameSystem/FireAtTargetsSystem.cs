using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.StaticData;

namespace FreedLOW.FireAtTargets.Code.Features.GameSystem
{
    public class FireAtTargetsSystem : IFireAtTargetsSystem, IDisposable
    {
        public bool IsGameActive => _isGameActive;

        public event Action OnGameStarted; 
        public event Action<GameResult> OnGameCompleted;
        public event Action<float> OnGameTimerTick;
        
        private readonly ShootingLevelStaticData _levelConfig;
        private readonly IGameTimer _gameTimer;

        private CancellationTokenSource _cts;
        private GameResult _gameResult;
        private bool _isGameActive;
        
        public FireAtTargetsSystem(ShootingLevelStaticData levelConfig, IGameTimer gameTimer)
        {
            _levelConfig = levelConfig;
            _gameTimer = gameTimer;
        }
        
        public async UniTask StartGame()
        {
            if (!_levelConfig)
                throw new ArgumentNullException(nameof(_levelConfig));

            if (_isGameActive)
                return;
            
            InitializeGame();
    
            try 
            {
                _isGameActive = true;
                OnGameStarted?.Invoke();
                await RunGameRound();
                CompleteGame(GameCompletionReason.TimeUp);
            }
            catch (OperationCanceledException)
            {
                CompleteGame(GameCompletionReason.Cancelled);
                throw;
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
            if (_isGameActive) 
                _gameResult.PointCount += point;
        }

        public void Dispose()
        {
            StopGame();
            _cts?.Dispose();
        }

        private void InitializeGame()
        {
            _cts = new CancellationTokenSource();
            _gameResult = new GameResult();
            _gameTimer.Reset();
        }

        private async UniTask RunGameRound()
        {
            await _gameTimer.StartTimerTick(_levelConfig.RoundTime, _levelConfig.CountdownTimeIncrement, _cts.Token, OnGameTimerTick);
        }

        private void CompleteGame(GameCompletionReason reason)
        {
            _isGameActive = false;
            _gameResult.CompletionReason = reason;
            _gameResult.GameSessionTime = _levelConfig.RoundTime;
            OnGameCompleted?.Invoke(_gameResult);
        }
    }
}