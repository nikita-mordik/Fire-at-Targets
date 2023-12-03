using System;
using FreedLOW.FireAtTergets.Code.Target;

namespace FreedLOW.FireAtTergets.Code.Infrastructure.Services.Point
{
    public class PointService : IPointService
    {
        public int CurrentPoints { get; private set; }
        
        public void AddPoint(TargetShootPointType pointType)
        {
            switch (pointType)
            {
                case TargetShootPointType.None:
                    throw new Exception("Not valid data!");
                case TargetShootPointType.One:
                    CurrentPoints += 1;
                    break;
                case TargetShootPointType.Two:
                    CurrentPoints += 2;
                    break;
                case TargetShootPointType.Three:
                    CurrentPoints += 3;
                    break;
                case TargetShootPointType.Four:
                    CurrentPoints += 4;
                    break;
                case TargetShootPointType.Five:
                    CurrentPoints += 5;
                    break;
                case TargetShootPointType.Six:
                    CurrentPoints += 6;
                    break;
                case TargetShootPointType.Seven:
                    CurrentPoints += 7;
                    break;
                case TargetShootPointType.Eight:
                    CurrentPoints += 8;
                    break;
                case TargetShootPointType.Nine:
                    CurrentPoints += 9;
                    break;
                case TargetShootPointType.Ten:
                    CurrentPoints += 10;
                    break;
            }
        }

        public void Reset()
        {
            CurrentPoints = 0;
        }
    }
}