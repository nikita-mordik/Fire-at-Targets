using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.AssetManagement
{
    public interface IAssetProvider
    {
        UniTask<GameObject> LoadAsset(string path);
    }
}