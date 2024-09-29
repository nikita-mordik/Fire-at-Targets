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

        private float currentSize;
        private bool isShooting;
        private bool isScope;

        private IInputService inputService;
        private ITimeService timeService;

        [Inject]
        private void Construct(IInputService inputService, ITimeService timeService)
        {
            this.inputService = inputService;
            this.timeService = timeService;
        }

        private void Start()
        {
            inputService.OnShoot += OnShoot;
            inputService.OnShootStop += OnShootStop;
            inputService.OnScope += OnScope;
        }

        private void OnDestroy()
        {
            inputService.OnShoot -= OnShoot;
            inputService.OnShootStop -= OnShootStop;
            inputService.OnScope -= OnScope;
        }

        private void LateUpdate()
        {
            currentSize = Mathf.Lerp(currentSize, (IsMoving() || isShooting) ? 
                maxSize : restingSize, speedChangeSize * timeService.DeltaTime);
            reticle.sizeDelta = new Vector2(currentSize, currentSize);
        }

        private void ControlVisibleCrosshair(bool isVisible) => 
            crosshairCanvasGroup.State(isVisible);

        private bool IsMoving() =>
            inputService.MovementAxis.x != 0 || inputService.MovementAxis.y != 0 ||
            inputService.RotationAxis.x != 0 || inputService.RotationAxis.y != 0;

        private void OnShoot()
        {
            isShooting = true;
        }

        private void OnShootStop()
        {
            isShooting = false;
        }

        private void OnScope()
        {
            isScope = !isScope;
            ControlVisibleCrosshair(!isScope);
        }
    }
}