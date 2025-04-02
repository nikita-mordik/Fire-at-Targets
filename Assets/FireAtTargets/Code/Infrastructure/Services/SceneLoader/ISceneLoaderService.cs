using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.SceneLoader
{
    public interface ISceneLoaderService
    {
        bool IsSceneLoaded(string sceneName);
        UniTask LoadSceneAsync(string sceneName, Action onSceneLoad = null);
        void MoveGameObjectToScene(GameObject moveGameObject);
    }
}