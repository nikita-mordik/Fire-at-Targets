using FreedLOW.FireAtTargets.Code.Common;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Extensions
{
  public static class CollisionExtensions
  {
    public static bool IsOfLayer(this Collider collider, string layer) =>
      collider.gameObject.layer == LayerMask.NameToLayer(layer);

    public static bool IsOfLayer(this Collider collider, CollisionLayer layer) =>
      collider.gameObject.layer == (int) layer;

    public static bool Matches(this Collider collider, LayerMask layerMask) =>
      ((1 << collider.gameObject.layer) & layerMask) != 0;
  }
}
