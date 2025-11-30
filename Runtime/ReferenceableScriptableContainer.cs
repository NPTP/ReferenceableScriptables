using NPTP.ReferenceableScriptables.Attributes;
using UnityEngine;

namespace NPTP.ReferenceableScriptables
{
    /// <summary>
    /// Container pointing to the desired scriptable object to be loaded in and out via Resources.
    /// </summary>
    internal class ReferenceableScriptableContainer : ScriptableObject
    {
        [SerializeField][GUIDisabled] private ScriptableObject reference;
        internal ScriptableObject Reference => reference;
    }
}