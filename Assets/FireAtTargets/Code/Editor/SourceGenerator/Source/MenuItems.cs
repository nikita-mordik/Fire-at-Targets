using FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Source.Settings;
using FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Source.Utils;
using UnityEditor;

namespace FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Source
{
    internal static class MenuItems
    {
        private const string MENU_GENERATE = "Tools/FreedLOW/Addressables/UnityCodeGen/Generate %G";
        private const string MENU_TOGGLE_AUTO_GENERATE = "Tools/FreedLOW/Addressables/UnityCodeGen/Auto-generate on Compile";

        [InitializeOnLoadMethod]
        private static void Init()
        {
            Menu.SetChecked(MENU_TOGGLE_AUTO_GENERATE, UnityCodeGenSettings.AutoGenerateOnCompile);
        }

        [MenuItem(MENU_GENERATE)]
        private static void Generate()
        {
            ScriptFileGenerator.Generate();
        }

        [MenuItem(MENU_TOGGLE_AUTO_GENERATE)]
        private static void ToggleAutoGenerate()
        {
            UnityCodeGenSettings.AutoGenerateOnCompile = !UnityCodeGenSettings.AutoGenerateOnCompile;
            Menu.SetChecked(MENU_TOGGLE_AUTO_GENERATE, UnityCodeGenSettings.AutoGenerateOnCompile);
        }
    }
}