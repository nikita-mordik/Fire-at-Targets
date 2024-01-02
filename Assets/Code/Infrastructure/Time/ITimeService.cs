using System;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Time
{
  public interface ITimeService
  {
    float DeltaTime { get; }
    float InGameTime { get; }
    DateTime UtcNow { get; }
  }
}