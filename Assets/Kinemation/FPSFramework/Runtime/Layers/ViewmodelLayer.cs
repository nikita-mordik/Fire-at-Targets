// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public class ViewmodelLayer : AnimLayer
    {
        public override void UpdateLayer()
        {
            LocRot offset = GetGunAsset().viewmodelOffset.poseOffset;
            GetMasterIK().Offset(GetRootBone(), offset.position / 100f);
            GetMasterIK().Offset(GetRootBone(), offset.rotation);

            offset = GetGunAsset().viewmodelOffset.rightHandOffset;
            GetRightHandIK().Offset(GetMasterPivot(), offset.position / 100f);
            GetRightHandIK().Offset(GetMasterPivot(), offset.rotation);
            
            offset = GetGunAsset().viewmodelOffset.leftHandOffset;
            GetLeftHandIK().Offset(GetMasterPivot(), offset.position / 100f);
            GetLeftHandIK().Offset(GetMasterPivot(), offset.rotation);
        }
    }
}
