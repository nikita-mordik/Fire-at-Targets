using UnityEngine.AddressableAssets;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Weapon
{
    public class SelectWeaponService : ISelectWeaponService
    {
        public AssetReferenceGameObject SelectedWeapon { get; private set; }

        public void SelectWeapon(AssetReferenceGameObject weapon)
        {
            SelectedWeapon = weapon;
        }
    }
}