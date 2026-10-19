using System;

namespace FreedLOW.FireAtTargets.Code.Save
{
    public interface ISaveService
    {
        string SelectedWeaponKey { get; set; }
        int SelectedGender { get; set; }
        SettingsData Settings { get; }

        void Load();
        void Save();
        void SaveIfDirty();

        int GetBestScore(string levelId);
        bool TrySubmitScore(string levelId, int score);
        void UpdateSettings(Action<SettingsData> change);
    }
}
