using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Source.Core;
using FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Source.Settings;
using UnityEditor;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Source.Utils
{
    internal static class ScriptFileGenerator
    {
        internal static void Generate()
        {
            IEnumerable<Type> generatorTypes = TypeCache.GetTypesDerivedFrom<ICodeGenerator>()
                .Where(x => !x.IsAbstract && x.GetCustomAttribute<GeneratorAttribute>() != null);

            bool changed = false;
            foreach (Type generatorType in generatorTypes)
            {
                ICodeGenerator generator = (ICodeGenerator)Activator.CreateInstance(generatorType);
                GeneratorContext context = new();
                generator.Execute(context);

                if (GenerateScriptFromContext(context))
                {
                    changed = true;
                }
            }

            if (!changed)
            {
                return;
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            if (UnityCodeGenSettings.AutoGenerateOnCompile)
            {
                Generate();
            }
        }

        private static bool GenerateScriptFromContext(GeneratorContext context)
        {
            bool changed = false;
            string folderPath = context.OverrideFolderPath ?? UnityCodeGenUtility.DEFAULT_FOLDER_PATH;

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            HashSet<string> expectedFilePaths = new HashSet<string>(
                context.CodeList.Select(code => Path.Combine(folderPath, code.FileName).Replace("\\", "/")));
            foreach (string existingFile in Directory.GetFiles(folderPath, "*.g.cs", SearchOption.AllDirectories))
            {
                string normalizedPath = existingFile.Replace("\\", "/");
                if (!expectedFilePaths.Contains(normalizedPath))
                {
                    File.Delete(existingFile);
                    Debug.Log($"🗑 Deleted outdated generated file: {normalizedPath}");
                    changed = true;
                }
            }

            foreach (CodeText code in context.CodeList)
            {
                string fullPath = Path.Combine(folderPath, code.FileName);
                string directory = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(fullPath))
                {
                    string existingText = File.ReadAllText(fullPath);
                    if (existingText == code.Text)
                    {
                        continue;
                    }
                }

                File.WriteAllText(fullPath, code.Text);
                Debug.Log($"📄 Wrote generated file: {fullPath}");
                changed = true;
            }

            return changed;
        }
    }
}