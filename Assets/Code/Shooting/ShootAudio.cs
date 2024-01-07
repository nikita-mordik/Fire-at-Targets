using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Shooting
{
    public class ShootAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource weaponAudioSource;
        
        private AudioClip clip;  // TODO: clip load from Addressable

        public void PlayShoot()
        {
            //weaponAudioSource.PlayOneShot(clip);
        }
    }
}