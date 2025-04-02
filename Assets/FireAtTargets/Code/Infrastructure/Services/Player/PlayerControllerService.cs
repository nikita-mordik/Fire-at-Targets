using FreedLOW.FireAtTargets.Code.Character;
using KINEMATION.FPSAnimationFramework.Runtime.Core;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player
{
    public class PlayerControllerService : MonoBehaviour, IPlayerControllerService
    {
        [SerializeField] private CustomFPSController _fpsController;
        [SerializeField] private FPSAnimator _fpsAnimator;
        [SerializeField] private CharacterWeaponController _weaponController;

        public void Initialize()
        {
            _fpsAnimator.Initialize();
            _weaponController.Initialize();
            _weaponController.EquipWeapon();
        }
        
        public void SetPositionAndRotation(Transform spawnPoint)
        {
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }
    }
}