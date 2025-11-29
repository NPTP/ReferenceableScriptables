using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    /// <summary>
    /// Container pointing to the desired ReferenceableScriptable, to be loaded in and out via Resources.
    /// </summary>
    internal class ScriptableReferenceContainer : ScriptableObject
    {
        [SerializeField] private ScriptableObject reference;
        internal ScriptableObject Reference => reference;
    }
}