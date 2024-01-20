using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Common
{
  public enum CollisionLayer
  {
    Hero = 6,
    Projectiles = 7,
    Ground = 8,
    GroundTrigger = 9,
    Hittable = 10,
  }
  
  public static partial class CleanCodeExtensions
  {
    public static int AsMask(this CollisionLayer layer) => 1 << LayerMask.NameToLayer(layer.ToString());
  }
}