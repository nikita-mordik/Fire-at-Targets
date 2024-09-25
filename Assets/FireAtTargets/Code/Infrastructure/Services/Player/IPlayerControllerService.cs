using FreedLOW.FireAtTargets.Code.Weapon;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player
{
    public interface IPlayerControllerService
    {
        void InitializeWeapon(WeaponBehaviour weaponBehaviour);
        void EquipWeapon();
        void InitializePlayerGender(Gender gender);
        void SetPosition(Transform spawnPoint);
    }
}