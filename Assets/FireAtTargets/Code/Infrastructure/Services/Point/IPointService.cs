using FreedLOW.FireAtTargets.Code.Target;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Point
{
    public interface IPointService
    {
        void AddPoint(TargetShootPointType pointType);
        void AddExtraPoints(TargetShootPointType pointType);
    }
}
