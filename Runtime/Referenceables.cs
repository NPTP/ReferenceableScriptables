using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    public static class Referenceables
    {
        public static bool TryLoad(string guid, out ReferenceableScriptable rs)
        {
            var container = Resources.Load<ScriptableReferenceContainer>(GetResourcesPath<ReferenceableScriptable>(guid));
            if (container == null || container.Reference is not ReferenceableScriptable tScriptable)
            {
                rs = null;
                return false;
            }

            rs = tScriptable;
            container.Loads++;
            return true;
        }

        public static void Unload(ReferenceableScriptable rs)
        {
            if (rs == null || !ScriptableReferenceContainer.IsContainerLoaded(rs.Guid))
            {
                return;
            }


            ScriptableReferenceContainer container = ScriptableReferenceContainer.GetByGUID(rs.Guid);
            if (container != null)
            {
                container.Loads--;
            }
        }

        private static string GetResourcesPath<T>(string guid)
        {
            // TODO: Use the "database" once implemented
            return $"ScriptableReferenceContainers/{typeof(T).Name}/{guid}";
        }
    }
}
