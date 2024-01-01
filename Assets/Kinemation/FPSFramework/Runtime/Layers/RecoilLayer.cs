// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Core.Components;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public class RecoilLayer : AnimLayer
    {
        [SerializeField] private bool useMeshSpace;
        
        public override void UpdateLayer()
        {
            var recoilAnim = core.characterData.recoilAnim;

            float aimWeight = GetRigData().aimWeight;
            Vector3 pivotOffset = GetGunAsset().adsRecoilOffset * aimWeight;
            pivotOffset = recoilAnim.rotation * pivotOffset - pivotOffset;
            recoilAnim.position += pivotOffset;
            
            if (useMeshSpace)
            {
                GetMasterIK().Offset(GetRootBone(), recoilAnim.position, smoothLayerAlpha);
                GetMasterIK().Offset(GetRootBone(), recoilAnim.rotation, smoothLayerAlpha);
                return;
            }
            
            GetMasterIK().Offset(recoilAnim.position, smoothLayerAlpha);
            GetMasterIK().Offset(recoilAnim.rotation, smoothLayerAlpha);
        }
    }
}
