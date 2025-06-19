using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Config;
using FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Source.Core;
using FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Tools;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Generators
{
    [Generator]
    public class AddressableConstantsGenerator : ICodeGenerator
    {
        private const string ALL_KEYS_FIELD_NAME = "AllKeys";
        
        private AddressableGenConfig _config;
        
        public void Execute(GeneratorContext context)
        {
            _config = Resources.Load<AddressableGenConfig>("AddressableSourceGen");
            if (!_config)
            {
                Debug.LogError("❌ AddressableGenConfig not found in Resources!");
                return;
            }
            
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            var groupMap = new Dictionary<string, List<AddressableAssetEntry>>();
            var labelMap = new Dictionary<string, List<AddressableAssetEntry>>();
            var allLabels = new HashSet<string>();
            
            foreach (var group in settings.groups)
            {
                if (group == null || group.entries.Count == 0 || group.ReadOnly)
                    continue;

                var groupName = CleanGroupName(group.Name);
                if (!_config.IncludedGroups.Contains(groupName))
                    continue;

                foreach (var entry in group.entries.Where(e => !e.IsSubAsset))
                {
                    if (entry.labels != null && entry.labels.Count > 0)
                    {
                        foreach (var label in entry.labels)
                        {
                            if (!labelMap.ContainsKey(label))
                                labelMap[label] = new List<AddressableAssetEntry>();
                            
                            labelMap[label].Add(entry);
                            allLabels.Add(label);
                        }
                    }
                    else
                    {
                        if (!groupMap.ContainsKey(groupName))
                            groupMap[groupName] = new List<AddressableAssetEntry>();
                        
                        groupMap[groupName].Add(entry);
                    }
                }
            }

            context.SetOverrideFolderPath(_config.OutputPath);

            // Generate groups (without labels)
            foreach (var kvp in groupMap)
            {
                var classInstance = GenerateClassForEntries(kvp.Key, kvp.Value);
                context.AddFile($"{kvp.Key}.g.cs", classInstance.GetString());
            }

            // Generate labels
            foreach (var kvp in labelMap)
            {
                string safeLabelName = "Label_" + NormalizeConstName(kvp.Key);
                var classInstance = GenerateClassForEntries(safeLabelName, kvp.Value);
                context.AddFile($"{safeLabelName}.g.cs", classInstance.GetString());
            }

            // Generate file for all labels
            if (allLabels.Any())
            {
                var labelsClass = new ClassInstance()
                    .SetPublic()
                    .SetStatic()
                    .SetName("AddressableLabels");

                foreach (string label in allLabels)
                {
                    string constName = NormalizeConstName(label);
                    labelsClass.AddField(new FieldInstance()
                        .SetPublic()
                        .SetConst()
                        .SetStringType()
                        .SetName(constName)
                        .SetAssignedValue($"\"{label}\""));
                }

                context.AddFile("AddressableLabels.g.cs", labelsClass.GetString());
            }
        }
        
        private ClassInstance GenerateClassForEntries(string className, List<AddressableAssetEntry> entries)
        {
            var classInstance = new ClassInstance()
                .AddUsing("System.Collections.Generic")
                .SetPublic()
                .SetStatic()
                .SetName(className);

            var keys = new List<string>();

            foreach (var entry in entries)
            {
                string constName = NormalizeConstName(entry.address);
                classInstance.AddField(new FieldInstance()
                    .SetPublic()
                    .SetConst()
                    .SetStringType()
                    .SetName(constName)
                    .SetAssignedValue($"\"{entry.address}\""));

                keys.Add(entry.address);
            }

            classInstance.AddField(new FieldInstance()
                .SetPublic()
                .SetStatic()
                .SetListType("string")
                .SetName(ALL_KEYS_FIELD_NAME)
                .SetAssignedValue(GenerateListInitializer(keys, 2)));

            return classInstance;
        }

        private static string NormalizeConstName(string key)
        {
            key = Regex.Replace(key, "([a-z])([A-Z])", "$1_$2");
            key = Regex.Replace(key, @"[^a-zA-Z0-9_]", "_");
            return key.ToUpperInvariant();
        }

        private static string CleanGroupName(string name)
        {
            name = name.Replace(" ", "").Replace("-", "").Replace("(", "_").Replace(")", "");
            return char.IsLetter(name[0]) ? name : "_" + name;
        }

        private static string GenerateListInitializer(List<string> keys, int indentDepth)
        {
            var indent = IndentGenerator.GetIndent(indentDepth);
            var sb = new StringBuilder();
            sb.AppendLine("new() {");

            foreach (string key in keys)
                sb.AppendLine($"{indent}    \"{key}\",");

            sb.Append($"{indent}}}");
            return sb.ToString();
        }
    }
}