using System.Globalization;
using FreedLOW.FireAtTargets.Code.Extensions;
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

        [Inject]
        private void Construct(IFireAtTargetsSystem fireAtTargetsSystem)
        {
            _fireAtTargetsSystem = fireAtTargetsSystem;
        }

        private void Start()
        {
            _fireAtTargetsSystem.OnGameStarted += OnGameStarted;
            _fireAtTargetsSystem.OnGameTimerTick += OnTimerTick;
            _fireAtTargetsSystem.OnGameCompleted += OnGameCompleted;
            _fireAtTargetsSystem.OnPointsChanged += OnPointsChanged;
        }

        private void OnDestroy()
        {
            _fireAtTargetsSystem.OnGameStarted -= OnGameStarted;
            _fireAtTargetsSystem.OnGameTimerTick -= OnTimerTick;
            _fireAtTargetsSystem.OnGameCompleted -= OnGameCompleted;
            _fireAtTargetsSystem.OnPointsChanged -= OnPointsChanged;
        }

        private void OnGameStarted()
        {
            _gameRoundCanvasGroup.State(true);
        }

        private void OnTimerTick(float time)
        {
            _timerText.text = string.Format(TimerFormat, time.ToString(CultureInfo.InvariantCulture));
        }

        private void OnPointsChanged(int points)
        {
            _targetsHItText.text = string.Format(PointsFormat, points);
        }

        private void OnGameCompleted(GameResult gameResult)
        {
            _gameRoundCanvasGroup.State(false);
        }
    }
}
