using FreedLOW.FireAtTargets.Code.Extensions;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Weapon
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

        [Inject]
        private void Construct(IInputService inputService)
        {
            this.inputService = inputService;
            this.inputService.OnShoot += OnShoot;
            this.inputService.OnShootStop += OnShootStop;
            this.inputService.OnScope += OnScope;
        }

        private void OnDestroy()
        {
            inputService.OnShoot -= OnShoot;
            inputService.OnShootStop -= OnShootStop;
            inputService.OnScope -= OnScope;
        }

        private void LateUpdate()
        {
            currentSize = Mathf.Lerp(currentSize, (IsMoving() || isShooting) ? maxSize : restingSize, speedChangeSize * Time.deltaTime);
            reticle.sizeDelta = new Vector2(currentSize, currentSize);
        }

        private void ControlVisibleCrosshair(bool isVisible) => 
            UITool.State(ref crosshairCanvasGroup, isVisible);

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