using System.Collections.Generic;
using FreedLOW.FireAtTargets.Code.Target;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.StaticData
{
    [CreateAssetMenu(fileName = "TargetData", menuName = "FireAtTargets/TargetData")]
    public class TargetData : ScriptableObject
    {
        public List<TargetPointData> TargetPoints;
    }

    [System.Serializable]
    public class TargetPointData
    {
        public TargetShootPointType ShootPointType;
        public int GivenPoint;
        public int GivenExtraPoint;
    }
}