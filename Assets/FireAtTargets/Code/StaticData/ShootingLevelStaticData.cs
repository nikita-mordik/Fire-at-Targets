using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.StaticData
{
    [CreateAssetMenu(fileName = "ShootingLevelData", menuName = "FireAtTargets/ShootingLevel")]
    public class ShootingLevelStaticData : ScriptableObject
    {
        public int CountdownTime = 3;
        public int CountdownTimeIncrement = 1;
        public int RoundTime;
    }
}