using System;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Input
{
    public interface IInputService
    {
        Vector2 MovementAxis { get; }
        Vector2 RotationAxis { get; }

        event Action OnFire;
        event Action OnFireReleased;
        event Action OnScope;
        event Action OnScopeReleased;
        event Action OnReload;
        
        bool IsFireButtonDown();
        bool IsFireButtonUp();
        bool IsReloadButtonDown();
        bool IsScopeButtonDown();
    }
}