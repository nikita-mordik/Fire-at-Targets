using FreedLOW.FireAtTargets.Code.StaticData;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    [CreateAssetMenu(fileName = "GameSceneInstaller", menuName = "FireAtTargets/GameSceneInstaller")]
    public class GameSceneInstaller : ScriptableObjectInstaller<GameSceneInstaller>
    {
        [SerializeField] private ShootingLevelStaticData _shootingLevel;
        
        public override void InstallBindings()
        {
            Container.BindInstance(_shootingLevel);
        }
    }
}