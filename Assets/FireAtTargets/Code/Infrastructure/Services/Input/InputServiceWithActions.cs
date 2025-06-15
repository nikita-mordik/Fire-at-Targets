using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input
{
    public class InputServiceWithActions : InputService
    {
        private readonly IAssetProvider _assetProvider;
        
        protected InputAction MoveAction;
        protected InputAction LookAction;
        protected InputAction FireAction;
        protected InputAction ReloadAction;
        protected InputAction ScopeAction;

        private bool _isAiming;

        public override Vector2 MovementAxis { get; }
        public override Vector2 RotationAxis { get; }

        protected InputServiceWithActions(IAssetProvider assetProvider)
        {
            _assetProvider = assetProvider;
            LoadInputActionAsset().Forget();
        }

        private async UniTaskVoid LoadInputActionAsset()
        {
            InputActionAsset = await _assetProvider.Load<InputActionAsset>(AddressableKeys.Common.PlayerControlActions);
            if (!InputActionAsset)
            {
                Debug.LogError("Failed to load InputActionAsset!");
                return;
            }

            var actionMap = InputActionAsset.FindActionMap("Gameplay");
            MoveAction = actionMap.FindAction("Move");
            LookAction = actionMap.FindAction("Look");
            FireAction = actionMap.FindAction("Fire");
            ReloadAction = actionMap.FindAction("Reload");
            ScopeAction = actionMap.FindAction("Aim");

            SubscribeActions();
            EnableInputActions();
        }

        protected virtual void SubscribeActions()
        {
            FireAction.started += FireActionOnStarted;
            FireAction.canceled += FireActionOnCanceled;
            ScopeAction.performed += ScopeActionOnPerformed;
            ReloadAction.started += ReloadActionOnStarted;
        }

        protected virtual void EnableInputActions()
        {
            MoveAction.Enable();
            LookAction.Enable();
            FireAction.Enable();
            ReloadAction.Enable();
            ScopeAction.Enable();
        }

        private void FireActionOnStarted(InputAction.CallbackContext callback)
        {
            onFire?.Invoke();
        }

        private void FireActionOnCanceled(InputAction.CallbackContext callback)
        {
            onFireReleased?.Invoke();
        }

        private void ScopeActionOnPerformed(InputAction.CallbackContext callback)
        {
            if (callback.interaction is PressInteraction)
            {
                _isAiming = !_isAiming;
                if (_isAiming)
                {
                    onScope?.Invoke();
                }
                else
                {
                    onScopeReleased?.Invoke();
                }
            }
        }

        private void ReloadActionOnStarted(InputAction.CallbackContext callback)
        {
            onReload?.Invoke();
        }
    }
}