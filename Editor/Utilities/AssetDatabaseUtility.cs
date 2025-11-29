using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace NPTP.ReferenceableScriptables.Editor.Utilities
{
    internal static class AssetDatabaseUtility
    {
        internal static T[] GetAssetsOfTypeWithExclusions<T>(IEnumerable<Type> excludedTypes) where T : Object
        {
            return AssetDatabase.FindAssets($"t: {typeof(T).Name}", new[] { "Assets" })
                .Select(LoadAsset<T>)
                .Where(asset => !excludedTypes.Contains(asset.GetType()))
                .ToArray();
        }
        
        internal static string GetAssetGuid(this Object obj)
        {
            return obj == null
                ? string.Empty
                : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
        }

        private static T LoadAsset<T>(string guid) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }
    }
}