using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FreedLOW.FireAtTargets.Code.StaticData
{
    [CreateAssetMenu(fileName = "SelectedWeapon", menuName = "FireAtTargets/SelectedWeapon")]
    public class SelectedWeaponMock : ScriptableObject
    {
        public AssetReferenceGameObject SelectedWeapon;
    }
}