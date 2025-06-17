using System.Linq;

namespace FreedLOW.FireAtTargets.Code.SourceGenerator.Tools
{
    public class IndentGenerator
    {
        public static string GetIndent(int indentCount)
        {
            return string.Concat(Enumerable.Repeat(ToolConstants.DEFAULT_TAB_STRING, indentCount));
        }
    }
}