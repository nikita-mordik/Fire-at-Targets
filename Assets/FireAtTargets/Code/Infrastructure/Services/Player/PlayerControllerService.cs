using FreedLOW.FireAtTargets.Code.Character;
using KINEMATION.FPSAnimationFramework.Runtime.Core;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player
{
    public class PlayerControllerService : MonoBehaviour, IPlayerControllerService
    {
        [SerializeField] private CustomFPSController _fpsController;
        [SerializeField] private FPSAnimator _fpsAnimator;
        
        public void SetPositionAndRotation(Transform spawnPoint)
        {
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }

        public void Initialize()
        {
            _fpsAnimator.Initialize();
        }
    }
}