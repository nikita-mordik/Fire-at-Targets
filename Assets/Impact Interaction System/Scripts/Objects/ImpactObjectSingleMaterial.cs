using Impact.Materials;
using UnityEngine;

namespace Impact.Objects
{
    /// <summary>
    /// Component for Impact Objects that have only a single material.
    /// </summary>
    [AddComponentMenu("Impact/Impact Object Single Material", 0)]
    public class ImpactObjectSingleMaterial : ImpactObjectBase
    {
        [SerializeField]
        private ImpactMaterialBase _material;

        /// <summary>
        /// The ImpactMaterialBase this object uses.
        /// </summary>
        public ImpactMaterialBase Material
        {
            get { return _material; }
            set
            {
                _material = value;
                hasMaterial = _material != null;
            }
        }

        private bool hasMaterial;


        protected virtual void Awake()
        {
            hasMaterial = Material != null;
        }

        public override int GetMaterialCompositionNonAlloc(Vector3 point, ImpactMaterialComposition[] results)
        {

#if UNITY_EDITOR
            if(Application.isPlaying)
            {
                //Use cache
                return getMaterialCompositionNonAllocInternal(point, results, true);
            }
            else
            {
                //Don't use cache
                return getMaterialCompositionNonAllocInternal(point, results, false);
            }
#else
            //Use cache
            return getMaterialCompositionNonAllocInternal(point, results, true);
#endif

        }

        private int getMaterialCompositionNonAllocInternal(Vector3 point, ImpactMaterialComposition[] results, bool fromCache)
        {
            if (fromCache && !hasMaterial)
            {
                Debug.LogError($"Cannot get material composition for ImpactObjectSingleMaterial {gameObject.name} because it has no Material.");
                return 0;
            }

            results[0] = new ImpactMaterialComposition(_material, 1);
            return 1;
        }

        public override IImpactMaterial GetPrimaryMaterial(Vector3 point)
        {
            return GetPrimaryMaterial();
        }

        public override IImpactMaterial GetPrimaryMaterial()
        {

#if UNITY_EDITOR
            if(Application.isPlaying)
            {
                //Use cache
                if (!hasMaterial)
                    Debug.LogError($"ImpactObjectSingleMaterial {gameObject.name} has no Material.");

                return _material;
            }
            else
            {
                //Don't use cache
                return _material;
            }
#else
            //Use cache
            if (!hasMaterial)
                Debug.LogError($"ImpactObjectSingleMaterial {gameObject.name} has no Material.");

            return _material;
#endif

        }
    }
}

