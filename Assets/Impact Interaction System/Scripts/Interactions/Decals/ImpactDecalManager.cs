using System;
using System.Collections.Generic;
using UnityEngine;

namespace Impact.Interactions.Decals
{
    /// <summary>
    /// Component that manages decals that are attached to an object.
    /// </summary>
    [AddComponentMenu("Impact/Impact Decal Manager")]
    public class ImpactDecalManager : MonoBehaviour
    {
        private bool suppressDestroyEvent;
        private List<ImpactDecalBase> attachedDecals = new List<ImpactDecalBase>();

        /// <summary>
        /// Adds the decal to this object's list of attached decals. This does not change the parent of the decal.
        /// </summary>
        /// <param name="impactDecal">The decal to add.</param>
        public void AddDecal(ImpactDecalBase impactDecal)
        {
            attachedDecals.Add(impactDecal);
        }

        /// <summary>
        /// Removes the decal from this object's list of attached decals. This does not change the parent of the decal.
        /// </summary>
        /// <param name="impactDecal">The decal to remove</param>
        public void RemoveDecal(ImpactDecalBase impactDecal)
        {
            attachedDecals.Remove(impactDecal);
        }

        /// <summary>
        /// Releases all attached decals so that they are put back into their object pools.
        /// </summary>
        public void ReleaseAllDecals()
        {
            for (int i = 0; i < attachedDecals.Count; i++)
            {
                attachedDecals[i].MakeAvailable();
                i--;
            }

            attachedDecals.Clear();
        }

        private void OnApplicationQuit()
        {
            suppressDestroyEvent = true;
        }

        private void OnDestroy()
        {
            if (!suppressDestroyEvent)
                ReleaseAllDecals();
        }
    }
}

