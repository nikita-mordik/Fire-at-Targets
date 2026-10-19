using System;
using FreedLOW.FireAtTargets.Code.Save;
using NUnit.Framework;

namespace FreedLOW.FireAtTargets.Tests.EditMode.Save
{
    public class SaveServiceTests
    {
        private const string Gallery = "ShootingGallery";
        private const string SmallGallery = "SmallGallery";

        private InMemorySaveStorage _storage;
        private SaveService _service;

        [SetUp]
        public void SetUp()
        {
            _storage = new InMemorySaveStorage();
            _service = new SaveService(_storage);
        }

        [Test]
        public void NoSavedData_ProvidesDefaults()
        {
            _service.Load();

            Assert.AreEqual(0, _service.GetBestScore(Gallery));
            Assert.AreEqual("", _service.SelectedWeaponKey);
            Assert.AreEqual(0, _service.SelectedGender);
            Assert.AreEqual(1f, _service.Settings.LookSensitivity);
            Assert.AreEqual(1f, _service.Settings.MasterVolume);
            Assert.AreEqual(60, _service.Settings.TargetFrameRate);
            Assert.AreEqual(-1, _service.Settings.QualityLevel);
            Assert.IsTrue(_service.Settings.Vibration);
        }

        [Test]
        public void FirstScoreOnLevel_IsNewBest()
        {
            Assert.IsTrue(_service.TrySubmitScore(Gallery, 120));
            Assert.AreEqual(120, _service.GetBestScore(Gallery));
        }

        [Test]
        public void LowerScore_DoesNotReplaceBest()
        {
            _service.TrySubmitScore(Gallery, 120);

            Assert.IsFalse(_service.TrySubmitScore(Gallery, 80));
            Assert.AreEqual(120, _service.GetBestScore(Gallery));
        }

        [Test]
        public void EqualScore_IsNotNewBest()
        {
            _service.TrySubmitScore(Gallery, 120);

            Assert.IsFalse(_service.TrySubmitScore(Gallery, 120));
        }

        [Test]
        public void HigherScore_ReplacesBest()
        {
            _service.TrySubmitScore(Gallery, 120);

            Assert.IsTrue(_service.TrySubmitScore(Gallery, 150));
            Assert.AreEqual(150, _service.GetBestScore(Gallery));
        }

        [Test]
        public void BestScores_AreKeptPerLevel()
        {
            _service.TrySubmitScore(Gallery, 120);
            _service.TrySubmitScore(SmallGallery, 40);

            Assert.AreEqual(120, _service.GetBestScore(Gallery));
            Assert.AreEqual(40, _service.GetBestScore(SmallGallery));
        }

        [Test]
        public void ZeroScore_IsNotABest()
        {
            Assert.IsFalse(_service.TrySubmitScore(Gallery, 0));
            Assert.AreEqual(0, _service.GetBestScore(Gallery));
        }

        [Test]
        public void EmptyLevelId_IsRejected()
        {
            Assert.Throws<ArgumentException>(() => _service.TrySubmitScore("", 10));
            Assert.Throws<ArgumentException>(() => _service.GetBestScore(null));
        }

        [Test]
        public void SavedData_IsRestoredByANewServiceInstance()
        {
            _service.TrySubmitScore(Gallery, 120);
            _service.SelectedWeaponKey = "AK12";
            _service.SelectedGender = 2;
            _service.UpdateSettings(s =>
            {
                s.LookSensitivity = 0.5f;
                s.MusicVolume = 0.25f;
                s.Vibration = false;
                s.Language = "uk";
            });
            _service.Save();

            var reloaded = new SaveService(_storage);
            reloaded.Load();

            Assert.AreEqual(120, reloaded.GetBestScore(Gallery));
            Assert.AreEqual("AK12", reloaded.SelectedWeaponKey);
            Assert.AreEqual(2, reloaded.SelectedGender);
            Assert.AreEqual(0.5f, reloaded.Settings.LookSensitivity);
            Assert.AreEqual(0.25f, reloaded.Settings.MusicVolume);
            Assert.IsFalse(reloaded.Settings.Vibration);
            Assert.AreEqual("uk", reloaded.Settings.Language);
        }

        [Test]
        public void CorruptSaveFile_FallsBackToBackup()
        {
            _storage.Main = "{ this is not json";
            _storage.Backup = "{\"Version\":1,\"Levels\":[{\"LevelId\":\"ShootingGallery\",\"BestScore\":50}]}";

            _service.Load();

            Assert.AreEqual(50, _service.GetBestScore(Gallery));
        }

        [Test]
        public void CorruptSaveFileWithoutBackup_StartsFromDefaults()
        {
            _storage.Main = "{ this is not json";

            Assert.DoesNotThrow(() => _service.Load());
            Assert.AreEqual(0, _service.GetBestScore(Gallery));
            Assert.AreEqual(1f, _service.Settings.MasterVolume);
        }

        [Test]
        public void SaveWithoutVersion_IsMigratedAndKeepsData()
        {
            _storage.Main = "{\"Levels\":[{\"LevelId\":\"ShootingGallery\",\"BestScore\":7}]}";

            _service.Load();
            _service.Save();

            Assert.AreEqual(7, _service.GetBestScore(Gallery));
            StringAssert.Contains("\"Version\":1", _storage.Main);
        }

        [Test]
        public void SaveIfDirty_WritesOnlyWhenSomethingChanged()
        {
            _service.Load();

            _service.SaveIfDirty();
            Assert.AreEqual(0, _storage.WriteCount);

            _service.TrySubmitScore(Gallery, 10);
            _service.SaveIfDirty();
            _service.SaveIfDirty();
            Assert.AreEqual(1, _storage.WriteCount);
        }

        [Test]
        public void SettingsChange_IsSavedBySaveIfDirty()
        {
            _service.Load();

            _service.UpdateSettings(s => s.SfxVolume = 0.3f);
            _service.SaveIfDirty();

            var reloaded = new SaveService(_storage);
            reloaded.Load();
            Assert.AreEqual(0.3f, reloaded.Settings.SfxVolume);
        }

        [Test]
        public void SelectingTheSameWeaponAgain_DoesNotRequireASave()
        {
            _service.SelectedWeaponKey = "AK12";
            _service.Save();

            _service.SelectedWeaponKey = "AK12";
            _service.SaveIfDirty();

            Assert.AreEqual(1, _storage.WriteCount);
        }
    }
}
