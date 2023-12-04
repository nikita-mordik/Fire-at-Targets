using UnityEngine;

namespace FreedLOW.FireAtTergets.Code.Infrastructure.Services.Input
{
    public abstract class InputService : IInputService
    {
        protected const string Vertical = "Vertical";
        protected const string Horizontal = "Horizontal";
        private const string VerticalRotation = "VerticalRotation";
        private const string HorizontalRotation = "HorizontalRotation";
        private const string ShootButton = "Fire1";
        private const string ReloadButton = "Reload";
        private const string ScopeButton = "Scope";
        
        public abstract Vector2 MovementAxis { get; }
        public abstract Vector2 RotationAxis { get; }

        public virtual bool IsShootButtonDown() => 
            SimpleInput.GetButtonDown(ShootButton);

        public virtual bool IsShootButtonUp() => 
            SimpleInput.GetButtonUp(ShootButton);

        public virtual bool IsReloadButtonDown() => 
            SimpleInput.GetButtonDown(ReloadButton);

        public virtual bool IsScopeButtonDown() => 
            SimpleInput.GetButtonDown(ScopeButton);
        
        protected static Vector2 MobileMovementAxis() => 
            new Vector2(SimpleInput.GetAxis(Horizontal), SimpleInput.GetAxis(Vertical));
        
        protected static Vector2 MobileRotationAxis() => 
            new Vector2(SimpleInput.GetAxis(HorizontalRotation), SimpleInput.GetAxis(VerticalRotation));
    }
}