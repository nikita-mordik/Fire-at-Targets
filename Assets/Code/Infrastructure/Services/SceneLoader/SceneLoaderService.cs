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
        public void LoadScene(string nextScene, Action onLoaded = null)
        {
            if (SceneManager.GetActiveScene().name == nextScene)
            {
                onLoaded?.Invoke();
                return;
            }

            AsyncOperationHandle<SceneInstance> waitNextScene = Addressables.LoadSceneAsync(nextScene);

            while (!waitNextScene.IsDone) 
                UniTask.Delay(100);

            onLoaded?.Invoke();
        }
    }
}