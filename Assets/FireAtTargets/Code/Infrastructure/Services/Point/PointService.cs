using System;
using FreedLOW.FireAtTargets.Code.Target;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Point
{
    public class PointService : IPointService
    {
        public int CurrentPoints { get; private set; }
        
        public event Action<int> OnPointsChanged;

        public void AddPoint(TargetShootPointType pointType)
        {
            if (pointType == TargetShootPointType.None)
                throw new Exception("Not valid data!");

            CurrentPoints += (int) pointType;
            OnPointsChanged?.Invoke(CurrentPoints);
        }

        public void Reset()
        {
            CurrentPoints = 0;
        }
    }
}