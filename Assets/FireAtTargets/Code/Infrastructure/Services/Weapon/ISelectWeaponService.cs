using UnityEngine.AddressableAssets;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Weapon
{
    public interface ISelectWeaponService
    {
        AssetReferenceGameObject SelectedWeapon { get; }
        void SelectWeapon(AssetReferenceGameObject weapon);
    }
}