using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    /// <summary>
    /// Container pointing to the desired ReferenceableScriptable, to be loaded in and out via Resources.
    /// </summary>
    public class ScriptableReferenceContainer : ScriptableObject
    {
        [SerializeField] private ReferenceableScriptable reference;
        internal ReferenceableScriptable Reference => reference;
        public string ReferenceName => reference.name;
    }
}