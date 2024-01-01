// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public class RightHandIK : AnimLayer
    {
        public override void UpdateLayer()
        {
            LocRot offset = GetGunAsset().viewmodelOffset.rightHandOffset;

            GetRightHandIK().Offset(GetMasterPivot(), offset.position, smoothLayerAlpha);
            GetRightHandIK().Offset(GetMasterPivot(), offset.rotation, smoothLayerAlpha);
        }
    }
}
