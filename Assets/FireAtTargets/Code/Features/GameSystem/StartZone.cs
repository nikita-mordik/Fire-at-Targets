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
        private CancellationTokenSource _countdownCts;

        private IFireAtTargetsSystem _fireAtTargetsSystem;
        private ShootingLevelStaticData _levelConfig;

        private bool IsCountdownRunning => _countdownCts != null;

        [Inject]
        private void Construct(IFireAtTargetsSystem fireAtTargetsSystem, ShootingLevelStaticData levelConfig)
        {
            _fireAtTargetsSystem = fireAtTargetsSystem;
            _levelConfig = levelConfig;
        }

        private void OnDestroy()
        {
            _countdownCts?.Cancel();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other) || _fireAtTargetsSystem.IsGameActive || IsCountdownRunning)
                return;

            TryStartGame().Forget();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsPlayer(other) || _fireAtTargetsSystem.IsGameActive)
                return;

            _countdownCts?.Cancel();
        }

        public void Initialize(GameStartPopUp gameStartPopUp)
        {
            _gameStartPopUp = gameStartPopUp;
        }

        private async UniTaskVoid TryStartGame()
        {
            _countdownCts = new CancellationTokenSource();

            try
            {
                await _gameStartPopUp.StartCountdownAsync(_levelConfig.CountdownTime, _countdownCts.Token);
            }
            catch (OperationCanceledException)
            {
                // Player left the zone before the countdown finished.
                return;
            }
            finally
            {
                _countdownCts.Dispose();
                _countdownCts = null;
            }

            try
            {
                await _fireAtTargetsSystem.StartGame();
            }
            catch (OperationCanceledException)
            {
                // Round was stopped early (e.g. scene unload); the system already reported it as Cancelled.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static bool IsPlayer(Collider other) =>
            other.TryGetComponent<PlayerControllerService>(out _);
    }
}
