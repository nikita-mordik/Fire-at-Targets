using System;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Save
{
    /// <summary>
    /// Local, offline-first player save. Loads lazily on first access; changes are kept in memory and
    /// written by <see cref="Save"/> / <see cref="SaveIfDirty"/>.
    /// </summary>
    public class SaveService : ISaveService
    {
        public const int CurrentVersion = 1;

        private readonly ISaveStorage _storage;

        private SaveData _data;
        private bool _isDirty;

        public SaveService(ISaveStorage storage)
        {
            _storage = storage;
        }

        public string SelectedWeaponKey
        {
            get => Data.SelectedWeaponKey;
            set
            {
                value ??= "";
                if (Data.SelectedWeaponKey == value)
                    return;

                Data.SelectedWeaponKey = value;
                _isDirty = true;
            }
        }

        public int SelectedGender
        {
            get => Data.SelectedGender;
            set
            {
                if (Data.SelectedGender == value)
                    return;

                Data.SelectedGender = value;
                _isDirty = true;
            }
        }

        /// <summary>Read-only view for consumers; change settings through <see cref="UpdateSettings"/>.</summary>
        public SettingsData Settings => Data.Settings;

        private SaveData Data
        {
            get
            {
                if (_data == null)
                    Load();

                return _data;
            }
        }

        public void Load()
        {
            _data = ReadSavedData() ?? new SaveData { Version = CurrentVersion };
            Migrate(_data);
            _isDirty = false;
        }

        public void Save()
        {
            _storage.Write(JsonUtility.ToJson(Data));
            _isDirty = false;
        }

        public void SaveIfDirty()
        {
            if (_isDirty)
                Save();
        }

        public int GetBestScore(string levelId)
        {
            ValidateLevelId(levelId);
            return FindLevel(levelId)?.BestScore ?? 0;
        }

        public bool TrySubmitScore(string levelId, int score)
        {
            ValidateLevelId(levelId);
            if (score <= 0)
                return false;

            var level = FindLevel(levelId);
            if (level != null && score <= level.BestScore)
                return false;

            if (level == null)
            {
                level = new LevelRecord { LevelId = levelId };
                Data.Levels.Add(level);
            }

            level.BestScore = score;
            _isDirty = true;
            return true;
        }

        public void UpdateSettings(Action<SettingsData> change)
        {
            if (change == null)
                throw new ArgumentNullException(nameof(change));

            change(Data.Settings);
            _isDirty = true;
        }

        private LevelRecord FindLevel(string levelId) =>
            Data.Levels.Find(level => level.LevelId == levelId);

        private SaveData ReadSavedData()
        {
            if (TryParse(_storage.TryRead, out var data))
                return data;

            if (TryParse(_storage.TryReadBackup, out data))
            {
                Debug.LogWarning("[SaveService] Save file is unreadable, restored the backup.");
                return data;
            }

            return null;
        }

        private delegate bool ReadContent(out string content);

        private static bool TryParse(ReadContent read, out SaveData data)
        {
            data = null;
            try
            {
                if (!read(out var json) || string.IsNullOrWhiteSpace(json))
                    return false;

                // Overwrite a fresh instance so fields missing from older saves keep their defaults.
                var parsed = new SaveData();
                JsonUtility.FromJsonOverwrite(json, parsed);
                data = parsed;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveService] Could not read save data: {exception.Message}");
                return false;
            }
        }

        private static void Migrate(SaveData data)
        {
            data.Levels ??= new();
            data.SelectedWeaponKey ??= "";
            data.Settings ??= new SettingsData();

            // v0 (no "Version" field) → v1: same layout, only the version marker is added.
            if (data.Version < CurrentVersion)
                data.Version = CurrentVersion;
        }

        private static void ValidateLevelId(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
                throw new ArgumentException("Level id is required.", nameof(levelId));
        }
    }
}
