using System.Threading;
using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Common;
using FreedLOW.FireAtTargets.Code.Extensions;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input;
using FreedLOW.FireAtTargets.Code.Infrastructure.Time;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Weapon.UI
{
    public class Reticle : MonoBehaviour
    {
        [SerializeField] private CanvasGroup crosshairCanvasGroup;
        
        [Header("Crosshair settings")]
        [SerializeField] private RectTransform reticle;
        [SerializeField] private float restingSize;
        [SerializeField] private float maxSize;
        [SerializeField] private float speedChangeSize;

        private float _currentSize;
        private bool _isShooting;

        private CancellationTokenSource _cancellationToken = new();

        private IInputService _inputService;
        private ITimeService _timeService;

        [Inject]
        private void Construct(IInputService inputService, ITimeService timeService)
        {
            _inputService = inputService;
            _timeService = timeService;
        }

        private void Start()
        {
            _inputService.OnFire += OnFire;
            _inputService.OnFireReleased += OnFireReleased;
            _inputService.OnScope += OnScope;
            _inputService.OnScopeReleased += OnScopeReleased;
        }

        private void OnDestroy()
        {
            _inputService.OnFire -= OnFire;
            _inputService.OnFireReleased -= OnFireReleased;
            _inputService.OnScope -= OnScope;
            _inputService.OnScopeReleased -= OnScopeReleased;
            
            _cancellationToken?.Dispose();
        }

        private void LateUpdate()
        {
            _currentSize = Mathf.Lerp(_currentSize, (IsMoving() || _isShooting) ? 
                maxSize : restingSize, speedChangeSize * _timeService.DeltaTime);
            reticle.sizeDelta = new Vector2(_currentSize, _currentSize);
        }

        private void ControlVisibleCrosshair(bool isVisible) => 
            crosshairCanvasGroup.State(isVisible);

        private bool IsMoving() =>
            _inputService.MovementAxis.x != 0 || _inputService.MovementAxis.y != 0 ||
            _inputService.RotationAxis.x != 0 || _inputService.RotationAxis.y != 0;

        private void OnFire() => _isShooting = true;

        private void OnFireReleased() => _isShooting = false;

        private void OnScope()
        {
            _cancellationToken?.Cancel();
            _cancellationToken = new CancellationTokenSource();
            ControlVisibleCrosshair(false);
        }

        private async void OnScopeReleased()
        {
            await OnAnimateReticleWithDelay();
        }

        private async UniTask OnAnimateReticleWithDelay()
        {
            await UniTask.WaitForSeconds(Constants.ReticleAnimationDelay, false, 
                PlayerLoopTiming.Update, _cancellationToken.Token);
            ControlVisibleCrosshair(true);
        }
    }
}