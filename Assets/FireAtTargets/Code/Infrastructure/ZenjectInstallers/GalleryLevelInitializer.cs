using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.ZenjectInstallers
{
    public class GalleryLevelInitializer : MonoBehaviour, IInitializable
    {
        [Inject]
        private void Construct()
        {

        }
        
        public void Initialize()
        {
            
        }
    }
}