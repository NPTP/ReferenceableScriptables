using UnityEngine;

namespace NPTP.ReferenceableScriptables.Utilities
{
    public static class ScriptableObjectExtensions
    {
        public static bool TrySerialize<T>(this T scriptableObject, out ReferenceableScriptableID<T> serializationID) where T : ScriptableObject
        {
            if (scriptableObject == null)
            {
                Debug.LogWarning("Trying to serialize a null ScriptableObject.");
                serializationID = ReferenceableScriptableID<T>.Invalid;
                return false;
            }

            if (!ReferenceablesTable.TryGetGuidsByName(scriptableObject.name, out var guids))
            {
                Debug.LogWarning("Trying to serialize a non-referenceable ScriptableObject.");
                serializationID = ReferenceableScriptableID<T>.Invalid;
                return false;
            }

            if (guids.Length == 1)
            {
                serializationID = new ReferenceableScriptableID<T>(guids[0]);
                return true;
            }

            foreach (string guid in guids)
            {
                if (ReferenceablesTable.TryLoad(guid, out T loadedScriptable) && loadedScriptable == scriptableObject)
                {
                    serializationID = new ReferenceableScriptableID<T>(guid);
                    return true;
                }
            }

            Debug.LogWarning($"Unknown error trying to serialize ScriptableObject {scriptableObject.name}");
            serializationID = ReferenceableScriptableID<T>.Invalid;
            return false;
        }
    }
}