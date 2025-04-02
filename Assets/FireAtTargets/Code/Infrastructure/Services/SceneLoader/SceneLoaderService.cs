using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.SceneLoader
{
    public class SceneLoaderService : ISceneLoaderService
    {
        public bool IsSceneLoaded(string sceneName) => 
            SceneManager.GetSceneByName(sceneName).isLoaded;

        public async UniTask LoadSceneAsync(string sceneName, Action onSceneLoad = null)
        {
            try
            {
                var handler = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single, false);
                await handler.ToUniTask();

                if (handler.Status == AsyncOperationStatus.Succeeded)
                {
                    await handler.Result.ActivateAsync().ToUniTask();
                    onSceneLoad?.Invoke();
                }
                else
                {
                    Debug.LogError($"[LoadSceneAsync] Failed to load scene: {sceneName} — Status: {handler.Status}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LoadSceneAsync] Exception while loading scene '{sceneName}': {ex}");
            }
        }
        
        public void MoveGameObjectToScene(GameObject moveGameObject)
        {
            if (!moveGameObject)
            {
                Debug.LogError("MoveGameObjectToScene: GameObject is null!");
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                Debug.LogError("MoveGameObjectToScene: Active scene is not valid!");
                return;
            }
            
            SceneManager.MoveGameObjectToScene(moveGameObject, activeScene);
        }
    }
}