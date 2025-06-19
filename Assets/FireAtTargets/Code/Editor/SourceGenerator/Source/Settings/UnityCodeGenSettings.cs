using UnityEditor;

namespace FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Source.Settings
{
    public static class UnityCodeGenSettings
    {
        private const string KEY_GENERATE_ON_COMPILE = "UnityCodeGen-AutoGenerateOnCompile";

        public static bool AutoGenerateOnCompile { get => GetResult(); set => SetResult(value); }

        private static void SetResult(bool value)
        {
            EditorUserSettings.SetConfigValue(KEY_GENERATE_ON_COMPILE, value.ToString());
        }

        private static bool GetResult()
        {
            if (bool.TryParse(EditorUserSettings.GetConfigValue(KEY_GENERATE_ON_COMPILE), out bool result))
            {
                return result;
            }

            return false;
        }
    }
}