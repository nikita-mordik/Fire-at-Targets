using FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Source.Utils;

namespace FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Source.Core
{
    public static class UnityCodeGenUtility
    {
        public const string DEFAULT_FOLDER_PATH = "Assets/FireAtTargets/Generated";

        public static void Generate()
        {
            ScriptFileGenerator.Generate();
        }
    }
}