using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace NPTP.ReferenceableScriptables
{
    /// <summary>
    /// Note this code is taken from one of Nick's personal projects, pls don't steal it thx :)
    /// </summary>
    public class ScriptableReferenceContainer : ScriptableObject
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void ClearStaticHashSet() => loadedContainers.Clear();

        private static readonly HashSet<ScriptableReferenceContainer> loadedContainers = new();

        [FormerlySerializedAs("scriptableObjectReference")]
        [SerializeField] private ReferenceableScriptable reference;
        public ReferenceableScriptable Reference => reference;
        
        private class LoadCounter { public int count; }
        private LoadCounter loadCounter;

        public int Loads
        {
            get => loadCounter?.count ?? 0;
            set
            {
                if (loadCounter == null)
                {
                    loadCounter = new LoadCounter();
                    loadedContainers.Add(this);
                }
                loadCounter.count = value;
                if (loadCounter.count == 0)
                {
                    loadedContainers.Remove(this);
                    Resources.UnloadAsset(this);
                }
                else if (loadCounter.count < 0)
                {
                    throw new Exception($"Value cannot be negative: {nameof(ScriptableReferenceContainer)}.{nameof(Loads)} in {name}");
                }
            }
        }
        
        public static bool IsContainerLoaded(string guid)
        {
            foreach (ScriptableReferenceContainer container in loadedContainers)
            {
                if (container.name == guid)
                {
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        public void EDITOR_SetScriptableObjectReference(ReferenceableScriptable referenceableScriptable)
        {
            reference = referenceableScriptable;
        }
#endif
        public static ScriptableReferenceContainer GetByGUID(string referenceableScriptableReferenceContainerGuid)
        {
            foreach (ScriptableReferenceContainer container in loadedContainers)
            {
                if (container.name == referenceableScriptableReferenceContainerGuid)
                {
                    return container;
                }
            }

            return null;
        }
    }
}