using System;
using FreedLOW.FireAtTergets.Code.Target;

namespace FreedLOW.FireAtTergets.Code.Infrastructure.Services.Point
{
    public interface IPointService
    {
        int CurrentPoints { get; }

        event Action<int> OnPointsChanged; 

        void AddPoint(TargetShootPointType pointType);
        void Reset();
    }
}