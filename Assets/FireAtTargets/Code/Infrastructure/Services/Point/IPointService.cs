using System;
using FreedLOW.FireAtTargets.Code.Target;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Point
{
    public interface IPointService
    {
        int CurrentPoints { get; }

        event Action<int> OnPointsChanged; 

        void AddPoint(TargetShootPointType pointType);
        void Reset();
    }
}