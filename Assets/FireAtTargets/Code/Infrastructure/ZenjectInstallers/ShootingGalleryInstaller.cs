using System.Collections.Generic;
using FreedLOW.FireAtTargets.Code.Infrastructure.Services.Point;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    public class ShootingGalleryInstaller : MonoInstaller
    {
        public List<MonoBehaviour> Initializers;
        
        public override void InstallBindings()
        {
            foreach (var initializer in Initializers)
            {
                Container.BindInterfacesTo(initializer.GetType())
                    .FromInstance(initializer)
                    .AsSingle();
            }
            
            BindPointService();
        }

        private void BindPointService()
        {
            Container.Bind<IPointService>()
                .To<PointService>()
                .AsSingle();
        }
    }
}