﻿using System;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Time
{
  public class UnityTimeService : ITimeService
  {
    public float DeltaTime => UnityEngine.Time.deltaTime;
    public float InGameTime => UnityEngine.Time.time;

    public DateTime UtcNow => DateTime.UtcNow;
  }
}