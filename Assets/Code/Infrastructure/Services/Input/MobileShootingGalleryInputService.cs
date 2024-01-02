using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input
{
    public class MobileShootingGalleryInputService : InputService
    {
        public override Vector2 MovementAxis => MobileMovementAxis();
        public override Vector2 RotationAxis => MobileRotationAxis();
    }
}