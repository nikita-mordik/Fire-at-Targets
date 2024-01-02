using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement
{
    public class AssetProvider : IAssetProvider
    {
        public async UniTask<GameObject> LoadAsset(string path) => 
            await Addressables.LoadAssetAsync<GameObject>(path).ToUniTask();
    }
}