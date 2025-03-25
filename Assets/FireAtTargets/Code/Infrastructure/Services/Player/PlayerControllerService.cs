using FreedLOW.FireAtTargets.Code.Character;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player
{
    public class PlayerControllerService : MonoBehaviour, IPlayerControllerService
    {
        [SerializeField] private CustomFPSController _fpsController;
        
        private PlayerInput _playerInput;
        
        private IInputService _inputService;

        [Inject]
        private void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }

        private void Start()
        {
            InitializeInput();
        }

        public void SetPositionAndRotation(Transform spawnPoint)
        {
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }

        private void InitializeInput()
        {
            _playerInput = gameObject.AddComponent<PlayerInput>();
            _playerInput.actions = _inputService.InputActionAsset;
            _playerInput.actions.Enable();
        }
    }
}