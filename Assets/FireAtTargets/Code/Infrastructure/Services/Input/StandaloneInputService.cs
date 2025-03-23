using FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input
{
    public class StandaloneInputService : InputServiceWithActions
    {
        protected StandaloneInputService(IAssetProvider assetProvider) : base(assetProvider)
        {
        }
        
        public override Vector2 MovementAxis => MoveAction.ReadValue<Vector2>();
        public override Vector2 RotationAxis => LookAction.ReadValue<Vector2>();
        
        public override bool IsReloadButtonDown() => ReloadAction.triggered;
        public override bool IsScopeButtonDown() => ScopeAction.triggered;
    }
}