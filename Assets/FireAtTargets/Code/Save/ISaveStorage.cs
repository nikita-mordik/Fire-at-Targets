namespace FreedLOW.FireAtTargets.Code.Save
{
    /// <summary>
    /// Raw persistence for the serialized save: a main copy plus the previous version as a backup.
    /// </summary>
    public interface ISaveStorage
    {
        bool TryRead(out string content);
        bool TryReadBackup(out string content);
        void Write(string content);
    }
}
