using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace NPTP.ReferenceableScriptables.Editor
{
    public class BuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            ReferenceablesTable.Clean();
        }
    }
}