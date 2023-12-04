using UnityEngine;

namespace FreedLOW.FireAtTergets.Code.Infrastructure.Services.Input
{
    public class StandaloneInputService : InputService
    {
        private const string MouseX = "Mouse X";
        private const string MouseY = "Mouse Y";
        
        public override Vector2 MovementAxis
        {
            get
            {
                Vector2 axis = MobileMovementAxis();

                if (axis == Vector2.zero)
                    axis = UnityKeyboardAxis();
                return axis;
            }
        }
        public override Vector2 RotationAxis
        {
            get
            {
                Vector2 axis = MobileRotationAxis();

                if (axis == Vector2.zero)
                    axis = UnityMouseAxis();
                return axis;
            }
        }

        public override bool IsShootButtonDown() => 
            UnityEngine.Input.GetKeyDown(KeyCode.Mouse0);

        public override bool IsShootButtonUp() => 
            UnityEngine.Input.GetKeyUp(KeyCode.Mouse0);

        public override bool IsReloadButtonDown() => 
            UnityEngine.Input.GetKeyDown(KeyCode.R);

        public override bool IsScopeButtonDown() => 
            UnityEngine.Input.GetKeyDown(KeyCode.Mouse1);

        private static Vector2 UnityKeyboardAxis() => 
            new Vector2(UnityEngine.Input.GetAxisRaw(Horizontal), UnityEngine.Input.GetAxisRaw(Vertical));
        
        private static Vector2 UnityMouseAxis() => 
            new Vector2(UnityEngine.Input.GetAxis(MouseX), -UnityEngine.Input.GetAxis(MouseY));
    }
}