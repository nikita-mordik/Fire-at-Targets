using FreedLOW.FireAtTergets.Code.Infrastructure.Services.Input;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTergets.Code.Infrastructure.ZenjectInstallers
{
    public class ShootingGalleryInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindInputService();
        }

        private void BindInputService()
        {
            Container.Bind<IInputService>()
                .To<InputService>()
                .FromInstance(GetInputService())
                .AsSingle();
        }

        private static InputService GetInputService()
        {
            if (Application.isEditor)
                return new StandaloneInputService();

            return new MobileShootingGalleryInputService();
        }
    }
}