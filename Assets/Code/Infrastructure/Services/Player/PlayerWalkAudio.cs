using Demo.Scripts.Runtime;
using Impact;
using Impact.Triggers;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player
{
    public class PlayerWalkAudio : MonoBehaviour
    {
        [SerializeField] private FPSController fpsController;
        
        [Header("Footsteps")]
        [SerializeField] private ImpactTag footstepLeftTag;
        [SerializeField] private ImpactTag footstepRightTag;
        [SerializeField] private float footstepInterval;
        [SerializeField] private Vector3 footOffset;
        
        private bool isSneaking;

        private RaycastHit hit;
        private Vector3 previousPosition;
        private float distanceTravelled;
        private int foot = 1;

        private void LateUpdate()
        {
            Vector3 currentPosition = transform.position;
            float distanceTravelledThisFrame = Vector3.Distance(previousPosition, currentPosition);

            distanceTravelled += distanceTravelledThisFrame;
            if (fpsController.IsGrounded && distanceTravelled > footstepInterval)
            {
                distanceTravelled = 0;
                foot = -foot;

                TriggerFootstep();
            }

            previousPosition = currentPosition;
        }
        
        private void TriggerFootstep()
        {
            //Velocity is a 0-1 value used to scale the volume of the footstep sounds
            float velocity = isSneaking ? 0.25f : 1;

            Ray ray = new Ray(transform.position + transform.rotation * footOffset, Vector3.down);
            if (Physics.Raycast(ray, out hit))
            {
                InteractionData data = new InteractionData()
                {
                    Velocity = Vector3.down * velocity,
                    CompositionValue = 1,
                    PriorityOverride = 100,
                    ThisObject = gameObject
                };

                data.TagMask = foot switch
                {
                    > 0 => footstepRightTag.GetTagMask(),
                    < 0 => footstepLeftTag.GetTagMask(),
                    _ => data.TagMask
                };

                ImpactRaycastTrigger.Trigger(data, hit, true);
            }
        }
    }
}