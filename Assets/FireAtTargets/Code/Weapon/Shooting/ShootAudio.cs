using Impact;
using Impact.Triggers;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Weapon.Shooting
{
    [RequireComponent(typeof(AudioSource))]
    public class ShootAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource weaponAudioSource;
        [SerializeField] private ImpactTag bulletTag;

        private const float BulletForce = 200f;
        
        private AudioClip clip;  // TODO: clip load from Addressable

        public void PlayShoot()
        {
            //weaponAudioSource.PlayOneShot(clip);
        }

        public void PlayHit(RaycastHit hit, Vector3 shootPointForward)
        {
            InteractionData data = new InteractionData()
            {
                Velocity = shootPointForward * BulletForce,
                CompositionValue = 1,
                PriorityOverride = 100,
                ThisObject = gameObject,
                TagMask = bulletTag.GetTagMask()
            };
            
            ImpactRaycastTrigger.Trigger(data, hit, false);
        }
    }
}