using FreedLOW.FireAtTergets.Code.Common;
using UnityEngine;

namespace FreedLOW.FireAtTergets.Code.Extensions
{
  public static class CollisionExtensions
  {
    public static bool IsOfLayer(this Collider2D collider, string layer) =>
      collider.gameObject.layer == LayerMask.NameToLayer(layer);

    public static bool IsOfLayer(this Collider2D collider, CollisionLayer layer) =>
      collider.gameObject.layer == (int) layer;

    public static bool Matches(this Collider2D collider, LayerMask layerMask) =>
      ((1 << collider.gameObject.layer) & layerMask) != 0;
  }
}
