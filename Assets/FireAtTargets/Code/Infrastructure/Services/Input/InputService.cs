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

        protected Action onFire;
        protected Action onFireReleased;
        protected Action onScope;
        protected Action onScopeReleased;
        
        public abstract Vector2 MovementAxis { get; }
        public abstract Vector2 RotationAxis { get; }

        public event Action OnFire
        {
            add => onFire += value;
            remove => onFire -= value;
        }
        public event Action OnFireReleased
        {
            add => onFireReleased += value;
            remove => onFireReleased -= value;
        }
        public event Action OnScope
        {
            add => onScope += value;
            remove => onScope -= value;
        }
        public event Action OnScopeReleased
        {
            add => onScopeReleased += value;
            remove => onScopeReleased -= value;
        }

        public virtual bool IsFireButtonDown() => 
            SimpleInput.GetButtonDown(ShootButton);

        public virtual bool IsFireButtonUp() => 
            SimpleInput.GetButtonUp(ShootButton);

        public virtual bool IsReloadButtonDown() => 
            SimpleInput.GetButtonDown(ReloadButton);

        public virtual bool IsScopeButtonDown()
        {
            var isScopeButtonDown = SimpleInput.GetButtonDown(ScopeButton);
            if (!isScopeButtonDown) 
                return false;
            
            onScope?.Invoke();
            return true;
        }

        protected static Vector2 MobileMovementAxis() => 
            new Vector2(SimpleInput.GetAxis(Horizontal), SimpleInput.GetAxis(Vertical));
        
        protected static Vector2 MobileRotationAxis() => 
            new Vector2(SimpleInput.GetAxis(HorizontalRotation), SimpleInput.GetAxis(VerticalRotation));
    }
}