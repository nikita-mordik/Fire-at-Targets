using System;
using Cysharp.Threading.Tasks;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.SceneLoader
{
    public interface ISceneLoaderService
    {
        bool IsSceneLoaded(string sceneName);
        UniTask LoadSceneAsync(string sceneName, Action onSceneLoad = null);
    }
}