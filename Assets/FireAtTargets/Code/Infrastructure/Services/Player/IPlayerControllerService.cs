using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Player
{
    public interface IPlayerControllerService
    {
        void Initialize();
        void SetPositionAndRotation(Transform spawnPoint);
    }
}