using System;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input
{
    public interface IInputService
    {
        Vector2 MovementAxis { get; }
        Vector2 RotationAxis { get; }

        event Action OnShoot;
        event Action OnShootStop;
        event Action OnScope;
        
        bool IsShootButtonDown();
        bool IsShootButtonUp();
        bool IsReloadButtonDown();
        bool IsScopeButtonDown();
        void InvokeOnShoot();
        void InvokeOnShootStop();
    }
}