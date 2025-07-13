using System.Globalization;
using FreedLOW.FireAtTargets.Code.Extensions;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Point;
using TMPro;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Features.GameSystem.UI
{
    public class GameRoundView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _gameRoundCanvasGroup;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _targetsHItText;

        private const string TimerFormat = "Time left: {0} sec";
        private const string PointsFormat = "Points shot: {0}";

        private IFireAtTargetsSystem _fireAtTargetsSystem;
        private IPointService _pointService;

        [Inject]
        private void Construct(IFireAtTargetsSystem fireAtTargetsSystem, IPointService pointService)
        {
            _fireAtTargetsSystem = fireAtTargetsSystem;
            _pointService = pointService;
        }

        private void Start()
        {
            _fireAtTargetsSystem.OnGameStarted += OnGameStarted;
            _fireAtTargetsSystem.OnGameTimerTick += OnTimerTick;
            _fireAtTargetsSystem.OnGameCompleted += OnGameCompleted;
            _pointService.OnPointsChanged += OnTargetHit;
        }

        private void OnDestroy()
        {
            _fireAtTargetsSystem.OnGameStarted -= OnGameStarted;
            _fireAtTargetsSystem.OnGameTimerTick -= OnTimerTick;
            _fireAtTargetsSystem.OnGameCompleted -= OnGameCompleted;
            _pointService.OnPointsChanged -= OnTargetHit;
        }

        private void OnGameStarted()
        {
            _gameRoundCanvasGroup.State(true);
        }

        private void OnTimerTick(float time)
        {
            _timerText.text = string.Format(TimerFormat, time.ToString(CultureInfo.InvariantCulture));
        }

        private void OnTargetHit(int point)
        {
            _targetsHItText.text = string.Format(PointsFormat, point);
        }

        private void OnGameCompleted(GameResult gameResult)
        {
            _gameRoundCanvasGroup.State(false);
        }
    }
}