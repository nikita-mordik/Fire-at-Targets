using UnityEngine;

namespace FreedLOW.FireAtTergets.Code.StaticData
{
    [CreateAssetMenu(fileName = "ShootingLevelData", menuName = "StaticData/ShootingLevel")]
    public class ShootingLevelStaticData : ScriptableObject
    {
        public float RoundTime;
        public int MaxMilitaryTargets;
    }
}