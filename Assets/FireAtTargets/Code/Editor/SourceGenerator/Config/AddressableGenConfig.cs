using System.Collections.Generic;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Config
{
    [CreateAssetMenu(fileName = "AddressableSourceGen", menuName = "FireAtTargets/SourceGenerator")]
    public class AddressableGenConfig : ScriptableObject
    {
        public List<string> IncludedGroups;
        public string OutputPath = "Assets/FireAtTargets/Code/Generated/Addressables";
    }
}