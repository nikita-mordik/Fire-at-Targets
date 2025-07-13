using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player;
using FreedLOW.FireAtTargets.Code.StaticData;
using FreedLOW.FireAtTargets.Code.UI.PopUp;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Features.GameSystem
{
    public class StartZone : MonoBehaviour
    {
        private GameStartPopUp _gameStartPopUp;
        private CancellationTokenSource _cts;

        private IFireAtTargetsSystem _fireAtTargetsSystem;
        private ShootingLevelStaticData _levelConfig;

        [Inject]
        private void Construct(IFireAtTargetsSystem fireAtTargetsSystem, ShootingLevelStaticData levelConfig)
        {
            _fireAtTargetsSystem = fireAtTargetsSystem;
            _levelConfig = levelConfig;
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerControllerService>(out var player) && !_fireAtTargetsSystem.IsGameActive)
            {
                // TODO: show pop-up with timer before game starting
                Debug.LogError("player entered start zone");
                TryStartGame().Forget();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<PlayerControllerService>(out var player) && !_fireAtTargetsSystem.IsGameActive)
            {
                // TODO: disable pop-up and timer if game not started
                Debug.LogError("player exited start zone");
                _cts?.Cancel();
            }
        }

        public void Initialize(GameStartPopUp gameStartPopUp)
        {
            _gameStartPopUp = gameStartPopUp;
        }

        private async UniTask TryStartGame()
        {
            _cts = new CancellationTokenSource();
            
            try
            {
                await _gameStartPopUp.StartCountdownAsync(_levelConfig.CountdownTime, _cts.Token);
                await _fireAtTargetsSystem.StartGame();
            }
            catch (OperationCanceledException)
            {
                Debug.LogError("Operation canceled!");
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.Message);
            }
        }
    }
}