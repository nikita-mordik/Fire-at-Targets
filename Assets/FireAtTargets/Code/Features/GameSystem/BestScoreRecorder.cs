using System;
using FreedLOW.FireAtTargets.Code.Save;
using FreedLOW.FireAtTargets.Code.StaticData;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Features.GameSystem
{
    /// <summary>
    /// Records the round score as the level's best score when a round runs to the end.
    /// </summary>
    public class BestScoreRecorder : IInitializable, IDisposable
    {
        private readonly IFireAtTargetsSystem _fireAtTargetsSystem;
        private readonly ISaveService _saveService;
        private readonly ShootingLevelStaticData _levelConfig;

        public BestScoreRecorder(IFireAtTargetsSystem fireAtTargetsSystem, ISaveService saveService,
            ShootingLevelStaticData levelConfig)
        {
            _fireAtTargetsSystem = fireAtTargetsSystem;
            _saveService = saveService;
            _levelConfig = levelConfig;
        }

        public void Initialize()
        {
            _fireAtTargetsSystem.OnGameCompleted += OnGameCompleted;
        }

        public void Dispose()
        {
            _fireAtTargetsSystem.OnGameCompleted -= OnGameCompleted;
        }

        private void OnGameCompleted(GameResult gameResult)
        {
            // A cancelled round (e.g. leaving the level) is not a finished attempt.
            if (gameResult.CompletionReason != GameCompletionReason.TimeUp)
                return;

            if (!_saveService.TrySubmitScore(_levelConfig.Id, gameResult.PointCount))
                return;

            // A new record is rare and valuable — persist it right away instead of waiting for app pause.
            _saveService.Save();
            Debug.Log($"[BestScoreRecorder] New best score on '{_levelConfig.Id}': {gameResult.PointCount}");
        }
    }
}
