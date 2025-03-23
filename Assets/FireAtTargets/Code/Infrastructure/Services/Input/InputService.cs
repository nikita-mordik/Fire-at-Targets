using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input
{
    public abstract class InputService : IInputService
    {
        protected Action onFire;
        protected Action onFireReleased;
        protected Action onScope;
        protected Action onScopeReleased;
        protected Action onReload;
        
        public abstract Vector2 MovementAxis { get; }
        public abstract Vector2 RotationAxis { get; }
        public InputActionAsset InputActionAsset { get; private protected set; }

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
        public event Action OnReload
        {
            add => onReload += value;
            remove => onReload -= value;
        }

        public virtual bool IsFireButtonDown()
        {
            onFire?.Invoke();
            return true;
        }

        public virtual bool IsFireButtonUp()
        {
            onFireReleased?.Invoke();
            return true;
        }

        public virtual bool IsReloadButtonDown()
        {
            onReload?.Invoke();
            return true;
        }

        public virtual bool IsScopeButtonDown()
        {
            onScope?.Invoke();
            return true;
        }
    }
}