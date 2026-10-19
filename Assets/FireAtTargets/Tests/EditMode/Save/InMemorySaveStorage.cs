using FreedLOW.FireAtTargets.Code.Save;

namespace FreedLOW.FireAtTargets.Tests.EditMode.Save
{
    /// <summary>
    /// Test double for the file system: keeps the main file and its backup in memory and counts writes.
    /// </summary>
    public class InMemorySaveStorage : ISaveStorage
    {
        public string Main;
        public string Backup;
        public int WriteCount { get; private set; }

        public bool TryRead(out string content)
        {
            content = Main;
            return Main != null;
        }

        public bool TryReadBackup(out string content)
        {
            content = Backup;
            return Backup != null;
        }

        public void Write(string content)
        {
            if (Main != null)
                Backup = Main;

            Main = content;
            WriteCount++;
        }
    }
}
