using Cysharp.Threading.Tasks;
using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input
{
    public class InputServiceWithActions : InputService
    {
        public override Vector2 MovementAxis { get; }
        public override Vector2 RotationAxis { get; }
        
        protected InputAction MoveAction;
        protected InputAction LookAction;
        protected InputAction FireAction;
        protected InputAction ReloadAction;
        protected InputAction ScopeAction;

        private readonly IAssetProvider _assetProvider;

        protected InputServiceWithActions(IAssetProvider assetProvider)
        {
            _assetProvider = assetProvider;
            LoadInputActionAsset().Forget();
        }

        private async UniTaskVoid LoadInputActionAsset()
        {
            var asset = await _assetProvider.Load<InputActionAsset>(AssetName.InputAsset);
            if (!asset)
            {
                Debug.LogError("Failed to load InputActionAsset!");
                return;
            }

            var actionMap = asset.FindActionMap("Gameplay");
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
            ScopeAction.started += ScopeActionOnStarted;
            ScopeAction.canceled += ScopeActionOnCanceled;
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

        private void ScopeActionOnStarted(InputAction.CallbackContext callback)
        {
            onScope?.Invoke();
        }

        private void ScopeActionOnCanceled(InputAction.CallbackContext callback)
        {
            onScopeReleased?.Invoke();
        }
    }
}