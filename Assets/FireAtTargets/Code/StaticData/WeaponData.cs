using KINEMATION.FPSAnimationFramework.Runtime.Recoil;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.StaticData
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "StaticData/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        public int Damage;
        public FireMode FireMode;
        public float FireRate;
        public int BurstAmount;
        public int MaxAmmo;
        public int StartAmmo;
    }
}