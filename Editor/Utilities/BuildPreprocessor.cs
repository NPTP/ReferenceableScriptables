using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace NPTP.ReferenceableScriptables.Editor.Utilities
{
    public class BuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            Referenceables.Clean();
        }
    }
}