using System;
using UnityEngine;

namespace FreedLOW.FireAtTergets.Code.Infrastructure.Services.Input
{
    public interface IInputService
    {
        Vector2 MovementAxis { get; }
        Vector2 RotationAxis { get; }

        event Action OnShoot;
        event Action OnShootStop;
        
        bool IsShootButtonDown();
        bool IsShootButtonUp();
        bool IsReloadButtonDown();
        bool IsScopeButtonDown();
        void InvokeOnShoot();
        void InvokeOnShootStop();
    }
}