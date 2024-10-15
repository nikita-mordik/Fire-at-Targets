using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input
{
    public class StandaloneInputService : InputService
    {
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _fireAction;
        private InputAction _reloadAction;
        private InputAction _scopeAction;
        
        public StandaloneInputService(IAssetProvider assetProvider)
        {
            LoadInputActionAsset(assetProvider);
        }

        public override Vector2 MovementAxis => _moveAction.ReadValue<Vector2>();
        public override Vector2 RotationAxis => _lookAction.ReadValue<Vector2>();

        public override bool IsReloadButtonDown() => 
            _reloadAction.triggered;

        public override bool IsScopeButtonDown() => 
            _scopeAction.triggered;
        
        private async void LoadInputActionAsset(IAssetProvider assetProvider)
        {
            var inputActionAsset = await assetProvider.Load<InputActionAsset>(AssetName.InputAsset);
            if (inputActionAsset == null)
            {
                Debug.LogError("InputActionAsset not loaded!");
                return;
            }
            
            FindInputActions(inputActionAsset);
            SubscribeActions();
            EnableInputActions();
        }

        private void FindInputActions(InputActionAsset inputActionAsset)
        {
            var playerActionMap = inputActionAsset.FindActionMap("Gameplay");
            _moveAction = playerActionMap.FindAction("Move");
            _lookAction = playerActionMap.FindAction("Look");
            _fireAction = playerActionMap.FindAction("Fire");
            _reloadAction = playerActionMap.FindAction("Reload");
            _scopeAction = playerActionMap.FindAction("Aim");
        }

        private void SubscribeActions()
        {
            _fireAction.started += FireActionOnStarted;
            _fireAction.canceled += FireActionOnCanceled;
            _scopeAction.started += ScopeActionOnStarted;
            _scopeAction.canceled += ScopeActionOnCanceled;
        }

        private void EnableInputActions()
        {
            _moveAction.Enable();
            _lookAction.Enable();
            _fireAction.Enable();
            _reloadAction.Enable();
            _scopeAction.Enable();
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