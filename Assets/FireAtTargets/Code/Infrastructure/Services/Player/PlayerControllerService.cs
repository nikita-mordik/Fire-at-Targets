using Demo.Scripts.Runtime.Character;
using FreedLOW.FireAtTargets.Code.Weapon;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player
{
    public class PlayerControllerService : MonoBehaviour, IPlayerControllerService
    {
        [SerializeField] private FPSController fpsController;

        public void SetPosition(Transform spawnPoint) => 
            transform.position = spawnPoint.position;

        public void InitializeWeapon(WeaponBehaviour weaponBehaviour)
        {
            var weapon = weaponBehaviour.GetComponent<Demo.Scripts.Runtime.Item.Weapon>();
            //fpsController.Weapons.Add(weapon);
        }

        public void EquipWeapon()
        {
            //fpsController.EquipWeapon();
        }
    }
}