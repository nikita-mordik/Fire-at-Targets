using System;
using System.Collections.Generic;

namespace FreedLOW.FireAtTargets.Code.Save
{
    /// <summary>
    /// Everything persisted for the player. Serialized with JsonUtility, so keep fields public and [Serializable].
    /// A save without a "Version" field is read as version 0 and migrated on load.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int Version;
        public List<LevelRecord> Levels = new();
        public string SelectedWeaponKey = "";
        public int SelectedGender;
        public SettingsData Settings = new();
    }

    [Serializable]
    public class LevelRecord
    {
        public string LevelId;
        public int BestScore;
    }

    [Serializable]
    public class SettingsData
    {
        public float LookSensitivity = 1f;
        public float AimSensitivity = 1f;
        public bool InvertY;
        public float MasterVolume = 1f;
        public float MusicVolume = 1f;
        public float SfxVolume = 1f;
        /// <summary>-1 keeps the device's default quality level.</summary>
        public int QualityLevel = -1;
        public int TargetFrameRate = 60;
        public bool Vibration = true;
        /// <summary>Locale code; empty means "use the system language".</summary>
        public string Language = "";
    }
}
