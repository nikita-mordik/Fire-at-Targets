using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.StaticData
{
    [CreateAssetMenu(fileName = "ShootingLevelData", menuName = "FireAtTargets/ShootingLevel")]
    public class ShootingLevelStaticData : ScriptableObject
    {
        public float RoundTime;
        public int MaxMilitaryTargets;
    }
}