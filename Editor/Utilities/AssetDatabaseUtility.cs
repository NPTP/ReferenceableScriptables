using System.Linq;
using UnityEditor;

namespace NPTP.ReferenceableScriptables.Editor.Utilities
{
    public static class AssetDatabaseUtility
    {
        public static T[] GetAssets<T>() where T : UnityEngine.Object
        {
            return AssetDatabase.FindAssets($"t: {typeof(T).Name}")
                .Select(LoadAsset<T>)
                .ToArray();
        }

        private static T LoadAsset<T>(string guid) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }
    }
}