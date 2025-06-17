using System.Collections.Generic;

namespace FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Source.Core
{
    public sealed class GeneratorContext
    {
        private readonly List<CodeText> _codeList = new();
        
        internal string OverrideFolderPath { get; private set; } = "Assets/FireAtTargets/Code/Generated";
        internal IReadOnlyList<CodeText> CodeList => _codeList;
        
        public void AddFile(string fileName, string code)
        {
            _codeList.Add(new CodeText { FileName = fileName, Text = code });
        }

        public void SetOverrideFolderPath(string path)
        {
            OverrideFolderPath = path;
        }
    }
}