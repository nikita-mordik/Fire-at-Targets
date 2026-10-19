using FreedLOW.FireAtTargets.Code.Save;
using UnityEngine;
using Zenject;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.Save
{
    /// <summary>
    /// Flushes unsaved changes when the app goes to the background or quits.
    /// On Android, OnApplicationPause is the last reliable callback before the process can be killed.
    /// </summary>
    public class SaveLifecycleObserver : MonoBehaviour
    {
        private ISaveService _saveService;

        [Inject]
        private void Construct(ISaveService saveService)
        {
            _saveService = saveService;
        }

        private void Awake()
        {
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
                _saveService?.SaveIfDirty();
        }

        private void OnApplicationQuit()
        {
            _saveService?.SaveIfDirty();
        }
    }
}
