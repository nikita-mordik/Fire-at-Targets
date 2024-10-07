using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input
{
    public class StandaloneInputService : InputService
    {
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction fireAction;
        private InputAction reloadAction;
        private InputAction scopeAction;
        
        public StandaloneInputService(IAssetProvider assetProvider)
        {
            LoadInputActionAsset(assetProvider);
        }

        public override Vector2 MovementAxis => moveAction.ReadValue<Vector2>();
        public override Vector2 RotationAxis => lookAction.ReadValue<Vector2>();

        public override bool IsReloadButtonDown() => 
            reloadAction.triggered;

        public override bool IsScopeButtonDown() => 
            scopeAction.triggered;
        
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
            moveAction = playerActionMap.FindAction("Move");
            lookAction = playerActionMap.FindAction("Look");
            fireAction = playerActionMap.FindAction("Fire");
            reloadAction = playerActionMap.FindAction("Reload");
            scopeAction = playerActionMap.FindAction("Aim");
        }

        private void SubscribeActions()
        {
            fireAction.performed += FireActionOnPerformed;
        }

        private void EnableInputActions()
        {
            moveAction.Enable();
            lookAction.Enable();
            fireAction.Enable();
            reloadAction.Enable();
            scopeAction.Enable();
        }

        private void FireActionOnPerformed(InputAction.CallbackContext callback)
        {
            if (callback.action.triggered)
            {
                InvokeOnFire();
            }

            if (callback.action.WasReleasedThisFrame())
            {
                InvokeOnFireReleased();
            }
        }
    }
}