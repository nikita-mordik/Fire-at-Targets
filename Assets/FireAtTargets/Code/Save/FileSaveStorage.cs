using System.IO;

namespace FreedLOW.FireAtTargets.Code.Save
{
    /// <summary>
    /// Stores the save as a file, writing through a temp file and keeping the previous version as ".bak",
    /// so an interrupted write never leaves the player without a readable save.
    /// </summary>
    public class FileSaveStorage : ISaveStorage
    {
        private readonly string _path;
        private readonly string _backupPath;
        private readonly string _tempPath;

        public FileSaveStorage(string path)
        {
            _path = path;
            _backupPath = path + ".bak";
            _tempPath = path + ".tmp";
        }

        public bool TryRead(out string content) => TryReadFile(_path, out content);

        public bool TryReadBackup(out string content) => TryReadFile(_backupPath, out content);

        public void Write(string content)
        {
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(_tempPath, content);

            if (File.Exists(_path))
            {
                File.Copy(_path, _backupPath, true);
                File.Delete(_path);
            }

            File.Move(_tempPath, _path);
        }

        private static bool TryReadFile(string path, out string content)
        {
            content = File.Exists(path) ? File.ReadAllText(path) : null;
            return content != null;
        }
    }
}
