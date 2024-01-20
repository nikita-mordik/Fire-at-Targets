using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.SceneLoader
{
    public class SceneLoaderService : ISceneLoaderService
    {
        public bool IsSceneLoaded(string sceneName) => 
            SceneManager.GetSceneByName(sceneName).isLoaded;

        public async UniTask LoadSceneAsync(string sceneName, Action onSceneLoad = null)
        {
            AsyncOperationHandle<SceneInstance> handler = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single, false);
            await handler.ToUniTask();
            await handler.Result.ActivateAsync().ToUniTask();
            onSceneLoad?.Invoke();
        }
    }
}