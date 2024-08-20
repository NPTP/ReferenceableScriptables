using UnityEngine;

namespace NPTP.ReferenceableScriptables.AssetTypes
{
    /// <summary>
    /// Container pointing to the desired ReferenceableScriptable, to be loaded in and out via Resources.
    /// </summary>
    public class ScriptableReferenceContainer : ScriptableObject
    {
        [SerializeField] private ReferenceableScriptable reference;
        public ReferenceableScriptable Reference => reference;
    }
}