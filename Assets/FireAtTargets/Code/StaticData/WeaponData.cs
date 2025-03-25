using KINEMATION.ProceduralRecoilAnimationSystem.Runtime;
using NaughtyAttributes;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.StaticData
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "FireAtTargets/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        public int Damage;
        public FireMode FireMode;
        public float FireRate;
        public bool SupportsBurst;
        [ShowIf("SupportsBurst")] public int BurstAmount;
        public int MaxAmmo;
        public int StartAmmo;
    }
}