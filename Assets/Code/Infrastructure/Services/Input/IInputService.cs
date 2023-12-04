using UnityEngine;

namespace FreedLOW.FireAtTergets.Code.Infrastructure.Services.Input
{
    public interface IInputService
    {
        Vector2 MovementAxis { get; }
        Vector2 RotationAxis { get; }
        
        bool IsShootButtonDown();
        bool IsShootButtonUp();
        bool IsReloadButtonDown();
        bool IsScopeButtonDown();
    }
}