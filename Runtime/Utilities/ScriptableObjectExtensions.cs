using UnityEngine;

namespace NPTP.ReferenceableScriptables.Utilities
{
    public static class ScriptableObjectExtensions
    {
        public static bool TrySerialize<T>(this T scriptableObject, out ReferenceableScriptableID<T> serializationID) where T : ScriptableObject
        {
            if (scriptableObject == null)
            {
                Debug.LogWarning("Trying to serialize a null scriptable object.");
                serializationID = ReferenceableScriptableID<T>.Invalid;
                return false;
            }

            foreach (string guid in ReferenceablesTable.Guids)
            {
                if (!ReferenceablesTable.TryLoad(guid, out T loadedScriptable) ||
                    loadedScriptable != scriptableObject)
                {
                    continue;
                }
                
                serializationID = new ReferenceableScriptableID<T>(guid);
                return true;
            }
            
            Debug.LogWarning("Trying to serialize a non-referenceable scriptable object.");
            serializationID = ReferenceableScriptableID<T>.Invalid;
            return false;
        }
    }
}