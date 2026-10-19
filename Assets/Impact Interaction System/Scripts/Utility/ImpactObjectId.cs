using UnityEngine;

namespace Impact
{
    /// <summary>
    /// Integer object ids used by Impact as dictionary and pairing keys.
    /// Unity 6.3+ replaced instance IDs with the 64-bit EntityId, which can no longer be converted to int,
    /// so the id is derived from the EntityId hash (unique per object in practice, valid for the current session).
    /// </summary>
    internal static class ImpactObjectId
    {
        public static int GetImpactId(this Object obj)
        {
#if UNITY_6000_3_OR_NEWER
            return obj.GetEntityId().GetHashCode();
#else
            return obj.GetInstanceID();
#endif
        }
    }
}
