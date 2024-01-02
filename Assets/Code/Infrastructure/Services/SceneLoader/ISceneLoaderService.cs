using System;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.SceneLoader
{
    public interface ISceneLoaderService
    {
        void LoadScene(string name, Action onLoaded = null);
    }
}