using System;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input
{
    public abstract class InputService : IInputService
    {
        protected const string Vertical = "Vertical";
        protected const string Horizontal = "Horizontal";
        private const string VerticalRotation = "VerticalRotation";
        private const string HorizontalRotation = "HorizontalRotation";
        private const string ShootButton = "Shoot";
        private const string ReloadButton = "Reload";
        private const string ScopeButton = "Scope";
        
        public abstract Vector2 MovementAxis { get; }
        public abstract Vector2 RotationAxis { get; }

        public event Action OnShoot;
        public event Action OnShootStop;
        public event Action OnScope;

        public virtual bool IsShootButtonDown() => 
            SimpleInput.GetButtonDown(ShootButton);

        public virtual bool IsShootButtonUp() => 
            SimpleInput.GetButtonUp(ShootButton);

        public virtual bool IsReloadButtonDown() => 
            SimpleInput.GetButtonDown(ReloadButton);

        public virtual bool IsScopeButtonDown()
        {
            var isScopeButtonDown = SimpleInput.GetButtonDown(ScopeButton);
            if (!isScopeButtonDown) return false;
            
            OnScope?.Invoke();
            return true;
        }

        public void InvokeOnShoot() => 
            OnShoot?.Invoke();

        public void InvokeOnShootStop() => 
            OnShootStop?.Invoke();

        protected static Vector2 MobileMovementAxis() => 
            new Vector2(SimpleInput.GetAxis(Horizontal), SimpleInput.GetAxis(Vertical));
        
        protected static Vector2 MobileRotationAxis() => 
            new Vector2(SimpleInput.GetAxis(HorizontalRotation), SimpleInput.GetAxis(VerticalRotation));
    }
}