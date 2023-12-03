using System;

namespace FreedLOW.FireAtTergets.Code.Infrastructure.Services.SceneLoader
{
    public interface ISceneLoaderService
    {
        void LoadScene(string name, Action onLoaded = null);
    }
}