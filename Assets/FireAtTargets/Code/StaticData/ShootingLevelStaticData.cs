using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.StaticData
{
    [CreateAssetMenu(fileName = "ShootingLevelData", menuName = "FireAtTargets/ShootingLevel")]
    public class ShootingLevelStaticData : ScriptableObject
    {
        [Tooltip("Stable id for saved best scores. Falls back to the asset name when empty.")]
        public string LevelId;
        public int CountdownTime = 3;
        public int CountdownTimeIncrement = 1;
        public int RoundTime;

        public string Id => string.IsNullOrEmpty(LevelId) ? name : LevelId;
    }
}