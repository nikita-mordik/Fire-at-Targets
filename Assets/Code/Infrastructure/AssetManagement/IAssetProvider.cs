using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FreedLOW.FireAtTergets.Code.Infrastructure.AssetManagement
{
    public interface IAssetProvider
    {
        UniTask<GameObject> LoadAsset(string path);
    }
}