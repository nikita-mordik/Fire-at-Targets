using FreedLOW.FireAtTargets.Code.Weapon;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player
{
    public interface IPlayerControllerService
    {
        void SetPosition(Transform spawnPoint);
        void InitializeWeapon(WeaponBehaviour weaponBehaviour);
        void EquipWeapon();
    }
}