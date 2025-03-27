using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Character
{
    public class CharacterInput : MonoBehaviour
    {
        private PlayerInput _playerInput;
        
        private IInputService _inputService;

        [Inject]
        private void Construct(IInputService inputService)
        {
            _inputService = inputService;
            InitializeInput();
        }
        
        private void InitializeInput()
        {
            _playerInput = gameObject.AddComponent<PlayerInput>();
            _playerInput.enabled = false;
            _playerInput.actions = _inputService.InputActionAsset;
            _playerInput.actions.Enable();
            _playerInput.enabled = true;
        }
    }
}