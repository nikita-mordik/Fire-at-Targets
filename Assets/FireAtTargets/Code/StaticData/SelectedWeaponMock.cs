using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.StaticData
{
    [CreateAssetMenu(fileName = "SelectedWeapon", menuName = "FireAtTargets/SelectedWeapon")]
    public class SelectedWeaponMock : ScriptableObject
    {
        public GameObject SelectedWeapon;
    }
}