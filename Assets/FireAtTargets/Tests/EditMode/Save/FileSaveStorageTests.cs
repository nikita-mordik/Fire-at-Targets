using System.IO;
using FreedLOW.FireAtTargets.Code.Save;
using NUnit.Framework;

namespace FreedLOW.FireAtTargets.Tests.EditMode.Save
{
    public class FileSaveStorageTests
    {
        private string _directory;
        private string _path;
        private FileSaveStorage _storage;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "FireAtTargetsSaveTests", Path.GetRandomFileName());
            _path = Path.Combine(_directory, "save.json");
            _storage = new FileSaveStorage(_path);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, true);
        }

        [Test]
        public void NoFile_ReadReturnsFalse()
        {
            Assert.IsFalse(_storage.TryRead(out _));
            Assert.IsFalse(_storage.TryReadBackup(out _));
        }

        [Test]
        public void Written_ContentCanBeReadBack()
        {
            _storage.Write("{\"Version\":1}");

            Assert.IsTrue(_storage.TryRead(out var content));
            Assert.AreEqual("{\"Version\":1}", content);
        }

        [Test]
        public void SecondWrite_KeepsPreviousContentAsBackup()
        {
            _storage.Write("first");
            _storage.Write("second");

            _storage.TryRead(out var main);
            _storage.TryReadBackup(out var backup);
            Assert.AreEqual("second", main);
            Assert.AreEqual("first", backup);
        }

        [Test]
        public void Write_LeavesOnlySaveAndBackupFiles()
        {
            _storage.Write("first");
            _storage.Write("second");

            var files = Directory.GetFiles(_directory);
            CollectionAssert.AreEquivalent(new[] { _path, _path + ".bak" }, files);
        }
    }
}
