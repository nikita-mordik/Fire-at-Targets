using FreedLOW.FireAtTergets.Code.Target;

namespace FreedLOW.FireAtTergets.Code.Infrastructure.Services.Point
{
    public interface IPointService
    {
        int CurrentPoints { get; }

        void AddPoint(TargetShootPointType pointType);
        void Reset();
    }
}